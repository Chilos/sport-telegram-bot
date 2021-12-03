using System.Collections.Generic;
using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetLastCompletedTrainByUser;

public sealed record GetLastCompletedTrainByUserRequest(int UserId, long ExerciseId) : IRequest<Domain.TrainRecord>;