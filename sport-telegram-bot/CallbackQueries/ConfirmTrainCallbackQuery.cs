using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace sport_telegram_bot.CallbackQueries;

public class ConfirmTrainCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly long _chatId;
    private readonly int _messageId;
    private readonly string _messageText;

    public ConfirmTrainCallbackQuery(ITelegramBotClient client, long chatId, int messageId, string messageText)
    {
        _client = client;
        _chatId = chatId;
        _messageId = messageId;
        _messageText = messageText;
    }

    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        await _client.EditMessageTextAsync(_chatId, 
            _messageId, 
            $"{_messageText} {Environment.NewLine}" +
            $"Тренировка запланирована!",
            cancellationToken: cancellationToken);
    }
}