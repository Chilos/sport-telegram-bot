using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Commands.RemoveExerciseRecordById;

public class RemoveExerciseRecordByIdRequestHandler: IRequestHandler<RemoveExerciseRecordByIdRequest>
{
    private readonly IBotDbContext _botDbContext;

    public RemoveExerciseRecordByIdRequestHandler(IBotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<Unit> Handle(RemoveExerciseRecordByIdRequest request, CancellationToken cancellationToken)
    {
        var exercise = await _botDbContext.ExerciseRecord
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        _botDbContext.ExerciseRecord.Remove(exercise);
        await _botDbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}