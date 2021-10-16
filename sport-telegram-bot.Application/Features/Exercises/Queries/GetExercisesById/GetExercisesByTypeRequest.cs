using MediatR;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById
{
    public sealed record GetExercisesByIdRequest(long Id): IRequest<Exercise>;
}