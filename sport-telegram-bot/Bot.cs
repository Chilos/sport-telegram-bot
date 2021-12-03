using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sport_telegram_bot.Application.Features.ExerciseRecord.Commands.CompleteExerciseRecord;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesByType;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetActiveTrainsByUser;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetLastCompletedTrainByUser;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordById;
using sport_telegram_bot.Application.Features.Users.Commands.CreateUser;
using sport_telegram_bot.Application.Features.Users.Queries.GetUsers;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot
{
    public class Bot : IHostedService
    {
        private readonly ILogger<Bot> _logger;
        private readonly TelegramBotClient _client;
        private readonly IMediator _mediator;
        private Dictionary<long, (int, int)> _questions = new Dictionary<long, (int, int)>();

        public Bot(ILogger<Bot> logger, TelegramBotClient client, IMediator mediator)
        {
            _logger = logger;
            _client = client;
            _mediator = mediator;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = {} // receive all update types
            };
            _client.StartReceiving(HandleUpdateAsync,HandleErrorAsync, receiverOptions, cancellationToken);
            _logger.LogInformation("Start Bot");
            await _client.SetMyCommandsAsync(new[]
            {
                new BotCommand
                {
                    Command = "add_train",
                    Description = "Добавить тренировку"
                },
                new BotCommand
                {
                    Command = "remove_train",
                    Description = "Удалить тренировку"
                },
                new BotCommand
                {
                    Command = "begin_train",
                    Description = "Начать тренировку"
                }
            },BotCommandScope.AllPrivateChats(), cancellationToken: cancellationToken);
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
                        case "/begin_train":
                            var beginTrainUser = await _mediator.Send(new GetUsersRequest(update.Message.From!.Id),
                                cancellationToken);
                            var beginTrain = await _mediator
                                .Send(new GetTrainRecordByDateRequest(DateTime.Now.Date, beginTrainUser.Id!.Value),
                                cancellationToken);
                            if (beginTrain == null)
                            {
                                await botClient.SendTextMessageAsync(update.Message.Chat,
                                    "На сегодня нет запланированных тренировок",
                                    cancellationToken: cancellationToken);
                                break;
                            }

                            var buttons = BeginExercisesChooseMenu(beginTrain);
                            if (!buttons.InlineKeyboard.Any())
                            {
                                await botClient.SendTextMessageAsync(update.Message.Chat,
                                    "Вы завершили тренирку на сегодня, поздравляем",
                                    cancellationToken: cancellationToken);
                                break;
                            }
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                "Выберите упражнение", 
                                replyMarkup: buttons,
                                cancellationToken: cancellationToken);
                            break;
                        default:
                            var telegramId = update.Message.From!.Id;
                            if (!_questions.ContainsKey(telegramId))
                            {
                                break;
                            }
                            var tuple = _questions[telegramId];
                            var res = update.Message.Text!.Split("-");
                            var request = new CompleteExerciseRecordRequest(
                                    tuple.Item1,
                                    int.Parse(res[0]), 
                                    int.Parse(res[1]));
                            await _mediator.Send(request, cancellationToken);
                            await botClient.DeleteMessageAsync(update.Message!.Chat.Id,
                                tuple.Item2, cancellationToken);
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                "Сохранено!",
                                cancellationToken: cancellationToken);
                            _questions.Remove(telegramId);
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
                            var user = await _mediator
                                .Send(new GetUsersRequest(update.CallbackQuery.From.Id), cancellationToken);
                            var train = await _mediator
                                .Send(new GetTrainRecordByDateRequest(date, user.Id!.Value), cancellationToken);
                            if (train is not null)
                            {
                                await botClient.SendTextMessageAsync(update.CallbackQuery.Message!.Chat.Id,
                                    "На данную дату тренировка уже запланирована",
                                    cancellationToken: cancellationToken);
                                break;
                            }
                            
                            await _mediator.Send(new CreateTrainRecordRequest(user, date), cancellationToken);
                            train = await _mediator
                                .Send(new GetTrainRecordByDateRequest(date, user.Id!.Value), cancellationToken);
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
                        case "confirmTrain":
                            await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id, 
                                update.CallbackQuery.Message.MessageId, 
                                $"{update.CallbackQuery.Message!.Text} {Environment.NewLine}" +
                                $"Тренировка запланирована!",
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
                        case "beginExercise":
                            var beginExerciseId = int.Parse(res[1]);
                            var beginExerciseTrainId = int.Parse(res[2]);
                            var beginExerciseUser = await _mediator
                                .Send(new GetUsersRequest(update.CallbackQuery.From.Id), cancellationToken);
                            var beginTrain = await _mediator
                                .Send(new GetTrainRecordByIdRequest(beginExerciseTrainId), cancellationToken);
                            
                            var beginExercise = beginTrain
                                .Exercises
                                .FirstOrDefault(e => e.Id == beginExerciseId);
                            var lastTrain = await _mediator.Send(
                                new GetLastCompletedTrainByUserRequest(beginExerciseUser.Id.Value, beginExercise.Exercise.Id), cancellationToken);
                            await botClient.DeleteMessageAsync(update.CallbackQuery.Message!.Chat.Id,
                                update.CallbackQuery.Message.MessageId, cancellationToken);
                            var message = await botClient.SendPhotoAsync(update.CallbackQuery.Message!.Chat.Id,
                                new InputOnlineFile(beginExercise!.Exercise.ImageUrl),
                                caption: $"{beginExercise!.Exercise.Description} {Environment.NewLine}" +
                                         $" {Environment.NewLine}" +
                                         $"Прошлый результат: {Environment.NewLine}" +
                                         $"число повторений: {lastTrain?.Exercises.Find(e => e.Exercise.Id == beginExercise.Exercise.Id).Repetitions} {Environment.NewLine}" +
                                         $"вес: {lastTrain?.Exercises.Find(e => e.Exercise.Id == beginExercise.Exercise.Id).Weight} {Environment.NewLine}" +
                                         $" {Environment.NewLine}" +
                                         $"Введите вес и число повторений по примеру:{Environment.NewLine}" +
                                         $"число повторений-вес",
                                replyMarkup: BeginExercisesChooseMenu(beginTrain),
                                cancellationToken: cancellationToken);
                            _questions[update.CallbackQuery.From.Id] = (beginExerciseId, message.MessageId);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("message error: {e}",e);
            }
        }
        
        private InlineKeyboardMarkup BeginExercisesChooseMenu(TrainRecord train)
        {
            
            var buttons = train.Exercises
                .Where(e => e.Weight == null && e.Repetitions == null)
                .Select(e => new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(e.Exercise.Description, $"beginExercise_{e.Id}_{train.Id}")
                }).ToList();

            return new InlineKeyboardMarkup(buttons);
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
            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Готово", $"confirmTrain_{trainId}")
            });

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