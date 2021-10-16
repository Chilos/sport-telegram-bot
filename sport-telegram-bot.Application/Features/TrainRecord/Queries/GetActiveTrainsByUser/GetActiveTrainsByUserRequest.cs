using System.Collections.Generic;
using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetActiveTrainsByUser
{
    public sealed record GetActiveTrainsByUserRequest(int UserId) : IRequest<IReadOnlyList<Domain.TrainRecord>>;
}