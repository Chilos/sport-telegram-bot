using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordById
{
    public sealed record GetTrainRecordByIdRequest(int TrainId) : IRequest<Domain.TrainRecord>;

}