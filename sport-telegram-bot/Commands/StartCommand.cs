using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace sport_telegram_bot.Commands;

public sealed class StartCommand
{
    private readonly ITelegramBotClient _client;
    private readonly long _chatId;
    public StartCommand(ITelegramBotClient client, long chatId)
    {
        _client = client;
        _chatId = chatId;
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        return _client.SendTextMessageAsync(_chatId,
            "приветствие и всякая фигня что бот может",
            cancellationToken: cancellationToken);
    }
}