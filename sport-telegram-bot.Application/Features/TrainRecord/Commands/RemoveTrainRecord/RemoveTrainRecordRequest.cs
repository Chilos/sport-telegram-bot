using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord
{
    public sealed record RemoveTrainRecordRequest(long TrainId) : IRequest;
}