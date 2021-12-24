using MediatR;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Commands.RemoveExerciseRecordById;

public sealed record RemoveExerciseRecordByIdRequest(int Id) : IRequest;
