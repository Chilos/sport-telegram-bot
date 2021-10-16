using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesByType;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetActiveTrainsByUser;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate;
using sport_telegram_bot.Application.Features.Users.Commands.CreateUser;
using sport_telegram_bot.Application.Features.Users.Queries.GetUsers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot
{
    public class Bot : IHostedService
    {
        private readonly ILogger<Bot> _logger;
        private readonly TelegramBotClient _client;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public Bot(ILogger<Bot> logger, TelegramBotClient client, IConfiguration configuration, IMediator mediator)
        {
            _logger = logger;
            _client = client;
            _configuration = configuration;
            _mediator = mediator;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = {} // receive all update types
            };
            _client.StartReceiving(HandleUpdateAsync,HandleErrorAsync, receiverOptions, cancellationToken);
            _logger.LogInformation("Start Bot");
            return Task.CompletedTask;
        }
        private async Task HandleUpdateAsync(ITelegramBotClient botClient,
            Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    if (update.Message is null)
                        return;
                    switch (update.Message.Text)
                    {
                        case "/start":
                            await _mediator.Send(new CreateUserRequest(update.Message!.From!.Id, update.Message.From.Username),
                                cancellationToken);
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                "Добро пожаловать!",
                                cancellationToken: cancellationToken);
                            break;
                        case "/add_train":
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                "Выберите день тренировки", 
                                replyMarkup: DateChooseMenu(),  
                                cancellationToken: cancellationToken);
                            break;
                        case "/remove_train":
                            var user = await _mediator.Send(new GetUsersRequest(update.Message.From!.Id),
                                cancellationToken);
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                "Выберите день тренировки для удаления", 
                                replyMarkup: await ActiveTrainMenu(user.Id!.Value, cancellationToken),  
                                cancellationToken: cancellationToken);
                            break;
                        default: 
                            await botClient.SendTextMessageAsync(update.Message.Chat, 
                                "Всякое разное описание",
                                cancellationToken: cancellationToken);
                            break;
                    }
                }

                if (update.Type == UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery is null)
                        return;
                    var res = update.CallbackQuery.Data!.Split("_");
                    switch (res![0])
                    {
                        //TODO: Отрефакторить этот ужас
                        case "trainDate":
                            var date = DateTime.Parse(res[1]);
                            var train = await _mediator
                                .Send(new GetTrainRecordByDateRequest(date), cancellationToken);
                            if (train is not null)
                            {
                                await botClient.SendTextMessageAsync(update.Message!.Chat,
                                    "На данную дату тренировка уже запланирована",
                                    cancellationToken: cancellationToken);
                            }
                            var user = await _mediator
                                .Send(new GetUsersRequest(update.CallbackQuery.From.Id), cancellationToken);
                            await _mediator.Send(new CreateTrainRecordRequest(user, date), cancellationToken);
                            train = await _mediator
                                .Send(new GetTrainRecordByDateRequest(date), cancellationToken);
                            await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id,
                                update.CallbackQuery.Message.MessageId, 
                                $"Дата тренировки выбрана: {date:dd.MM} {Environment.NewLine}" +
                                $"Добавьте упражнения:", 
                                replyMarkup: await TrainTypeChooseMenuAsync(train.Id, cancellationToken), 
                                cancellationToken: cancellationToken);
                            break;
                    
                        case "trainType":
                            var trainType = res[1];
                            var trainId = long.Parse(res[2]);
                            await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id, 
                                update.CallbackQuery.Message.MessageId, 
                                update.CallbackQuery.Message!.Text!, 
                                replyMarkup: await ExerciseChooseMenuAsync(trainType, trainId, cancellationToken),
                                cancellationToken: cancellationToken);
                            break;
                        case "addExercise":
                            var exerciseId = int.Parse(res[1]);
                            var id = int.Parse(res[2]);
                            var exercise = await _mediator.Send(new GetExercisesByIdRequest(exerciseId), cancellationToken);
                            await _mediator.Send(new AddExerciseToTrainRecordRequest(exerciseId, id), cancellationToken);
                            await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id, 
                                update.CallbackQuery.Message.MessageId, 
                                $"{update.CallbackQuery.Message!.Text} {Environment.NewLine}" +
                                $"{exercise.Description}",
                                replyMarkup: await TrainTypeChooseMenuAsync(id, cancellationToken), 
                                cancellationToken: cancellationToken);
                            break;
                        case "removeTrain":
                            var removeTrainId = long.Parse(res[1]);
                            await _mediator.Send(new RemoveTrainRecordRequest(removeTrainId), cancellationToken);
                            await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id, 
                                update.CallbackQuery.Message.MessageId, 
                                "Тренировка удалена!",
                                cancellationToken: cancellationToken);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("message error: {e}",e);
                throw;
            }
        }
        
        private InlineKeyboardMarkup DateChooseMenu()
        {
            var buttons = new List<InlineKeyboardButton>();
            var currentDate = DateTime.Now;
            for (var i = 0; i < 7; i++)
            {
                currentDate = currentDate.AddDays(i);
                var data = $"trainDate_{currentDate:d}";
                buttons.Add(InlineKeyboardButton.WithCallbackData($"{currentDate:dd.MM}", data));
            }
            
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ActiveTrainMenu(int userId, CancellationToken cancellationToken)
        {
            var trains = await _mediator
                .Send(new GetActiveTrainsByUserRequest(userId), cancellationToken);
            var buttons = new List<InlineKeyboardButton>();
            foreach (var train in trains)
            {
                var data = $"removeTrain_{train.Id}";
                buttons.Add(InlineKeyboardButton.WithCallbackData($"{train.DateAt:dd.MM}", data));
            }
            
            return new InlineKeyboardMarkup(buttons);
        }
        private async Task<InlineKeyboardMarkup> TrainTypeChooseMenuAsync(long trainId, CancellationToken cancellationToken)
        {
            var trainTypes = await _mediator.Send(new GetExerciseTypesRequest(), cancellationToken);
            var buttons = trainTypes.Select(type => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(type, $"trainType_{type}_{trainId}")
            }).ToList();

            return new InlineKeyboardMarkup(buttons);
        }
        
        private async Task<InlineKeyboardMarkup> ExerciseChooseMenuAsync(string type, long trainId, CancellationToken cancellationToken)
        {
            var exercises = await _mediator.Send(new GetExercisesByTypeRequest(type), cancellationToken);
            var buttons = exercises.Select(e => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(e.Description, $"addExercise_{e.Id}_{trainId}")
            }).ToList();

            return new InlineKeyboardMarkup(buttons);
        }
        
        static async Task HandleErrorAsync(ITelegramBotClient botClient,
            Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                await botClient.SendTextMessageAsync(123, 
                    apiRequestException.ToString(),
                    cancellationToken: cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("End Bot");
            return Task.CompletedTask;
        }
    }
}