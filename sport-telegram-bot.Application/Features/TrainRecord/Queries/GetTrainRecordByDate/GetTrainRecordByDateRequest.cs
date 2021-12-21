using System;
using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate
{
    public sealed record GetTrainRecordByDateRequest(DateOnly Date, int UserId) : IRequest<Domain.TrainRecord>;
}