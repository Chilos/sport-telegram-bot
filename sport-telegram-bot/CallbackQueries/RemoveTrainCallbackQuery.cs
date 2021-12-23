using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord;
using Telegram.Bot;

namespace sport_telegram_bot.CallbackQueries;

public class RemoveTrainCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly int _messageId;

    public RemoveTrainCallbackQuery(ITelegramBotClient client, IMediator mediator, long chatId, int messageId)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _messageId = messageId;
    }

    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var removeTrainId = long.Parse(payloadArray.First());
        await _mediator.Send(new RemoveTrainRecordRequest(removeTrainId), cancellationToken);
        await _client.EditMessageTextAsync(_chatId, 
            _messageId, 
            "Тренировка удалена!",
            cancellationToken: cancellationToken);
    }
}