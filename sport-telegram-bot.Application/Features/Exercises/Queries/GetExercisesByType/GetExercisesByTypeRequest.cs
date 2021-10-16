using System.Collections.Generic;
using MediatR;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesByType
{
    public sealed record GetExercisesByTypeRequest(string Type): IRequest<IReadOnlyList<Exercise>>;
}