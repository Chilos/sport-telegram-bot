using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sport_telegram_bot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace sport_telegram_bot
{
    public class Bot : IHostedService
    {
        private readonly ILogger<Bot> _logger;
        private readonly TelegramBotClient _client;
        private readonly IMediator _mediator;
        private readonly Dictionary<long, (int exerciseId, int previosMessageId)> _questions = new();
        private readonly Dictionary<long, List<(string messageText, int messageId)>> _exercises = new();

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
        
        private async Task HandleUpdateAsync(ITelegramBotClient client,
            Update update, CancellationToken cancellationToken)
        {
            var task = update.Type switch
            {
                UpdateType.Message => client.HandleMessageAsync(update.Message, _mediator, _questions, _exercises, cancellationToken),
                UpdateType.CallbackQuery => client.HandleCallbackQuery(update.CallbackQuery, _mediator, _questions, _exercises, cancellationToken),
                _ => Task.CompletedTask
            };

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
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