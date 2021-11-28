using MediatR;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Queries.GetExerciseRecordById
{
    public sealed record GetExerciseRecordByIdRequest(int Id) : IRequest<Domain.ExerciseRecord>;
}