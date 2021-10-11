using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sport_telegram_bot.Application.Features.Users.Commands;
using Telegram.Bot;
using Telegram.Bot.Args;
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
        private IMediator _mediator;

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
            if (update.Type == UpdateType.Message)
            {
                if (update.Message is null)
                    return;
                switch (update.Message.Text)
                {
                    case "/start":
                        await _mediator.Send(new CreateUserRequest(update.Message.From.Id, update.Message.From.Username),
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
                    case "/database":
                        await botClient.SendTextMessageAsync(update.Message.Chat, 
                            $"База данных: {_configuration["DATABASE_URL"]}", 
                            cancellationToken: cancellationToken);
                        break;
                    case "/photo":
                        await botClient.SendPhotoAsync(update.Message.Chat, 
                            $"База данных: {_configuration["DATABASE_URL"]}", 
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
                    case "trainDate":
                        var date = DateTime.Parse(res[1]);
                        await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id,
                            update.CallbackQuery.Message.MessageId, 
                            $"Дата тренировки выбрана: {date:dd.MM}", 
                            cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message!.Chat.Id, 
                            "Выберите тип тренировки", 
                            replyMarkup: TrainChooseMenu(date), 
                            cancellationToken: cancellationToken);
                        break;
                    
                    case "trainType":
                        var typeId = int.Parse(res[1]);
                        var trainDate = DateTime.Parse(res[2]);
                        await botClient.EditMessageTextAsync(update.CallbackQuery.Message!.Chat.Id, 
                            update.CallbackQuery.Message.MessageId, 
                            $"На {trainDate:dd.MM}, выбрана тренировка типа: {typeId}", 
                            cancellationToken: cancellationToken);
                        break;
                }
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
        private InlineKeyboardMarkup TrainChooseMenu(DateTime date)
        {
            var buttons = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Грудь и трицепс", $"trainType_1_{date}"),
                InlineKeyboardButton.WithCallbackData("Спина и бицепс", $"trainType_2_{date}"),
                InlineKeyboardButton.WithCallbackData("Плечи и ноги", $"trainType_3_{date}")
            };

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