using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace sport_telegram_bot.CallbackQueries;

public class ConfirmTrainCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly User _user;
    private readonly Dictionary<long, List<(string messageText, int messageId)>> _exercises;
    private readonly long _chatId;
    private readonly int _messageId;
    private readonly string _messageText;

    public ConfirmTrainCallbackQuery(ITelegramBotClient client, User user,
        Dictionary<long, List<(string messageText, int messageId)>> exercises,
        long chatId, int messageId, string messageText)
    {
        _client = client;
        _user = user;
        _exercises = exercises;
        _chatId = chatId;
        _messageId = messageId;
        _messageText = messageText;
    }

    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        foreach (var message in _exercises[_user.TelegramId])
        {
            await _client.EditMessageTextAsync(_chatId, 
                message.messageId,
                message.messageText,
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        _exercises.Remove(_user.TelegramId);
        await _client.EditMessageTextAsync(_chatId, 
            _messageId, 
            $"{_messageText} {Environment.NewLine}" +
            $"Тренировка запланирована!",
            cancellationToken: cancellationToken);
    }
}