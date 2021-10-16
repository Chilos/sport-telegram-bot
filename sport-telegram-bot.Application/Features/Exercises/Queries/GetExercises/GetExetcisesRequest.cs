using System.Collections.Generic;
using MediatR;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercises
{
    public sealed record GetExercisesRequest: IRequest<IReadOnlyList<Exercise>>;
}