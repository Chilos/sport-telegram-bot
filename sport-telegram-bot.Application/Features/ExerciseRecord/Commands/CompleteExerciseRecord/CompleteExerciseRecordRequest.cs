using MediatR;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Commands.CompleteExerciseRecord
{
    public record CompleteExerciseRecordRequest(int id, int Repetitions, int Weight) : IRequest;
}