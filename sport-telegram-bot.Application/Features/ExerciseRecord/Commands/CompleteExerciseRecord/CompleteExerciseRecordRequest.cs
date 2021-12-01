using MediatR;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Commands.CompleteExerciseRecord
{
    public record CompleteExerciseRecordRequest(int Id, int Repetitions, int Weight) : IRequest;
}