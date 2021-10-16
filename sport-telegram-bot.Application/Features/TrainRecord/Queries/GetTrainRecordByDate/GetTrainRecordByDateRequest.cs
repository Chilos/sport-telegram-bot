using System;
using System.Collections.Generic;
using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate
{
    public sealed record GetTrainRecordByDateRequest(DateTime date) : IRequest<Domain.TrainRecord>;
}