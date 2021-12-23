using MediatR;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord
{
    public sealed record AddExerciseToTrainRecordRequest(int ExerciseId, int TrainId) : IRequest<int>;
}