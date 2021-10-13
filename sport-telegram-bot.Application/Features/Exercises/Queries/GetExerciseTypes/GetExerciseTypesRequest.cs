using System.Collections.Generic;
using MediatR;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes
{
    public sealed record GetExerciseTypesRequest : IRequest<IReadOnlyList<string>>;
}