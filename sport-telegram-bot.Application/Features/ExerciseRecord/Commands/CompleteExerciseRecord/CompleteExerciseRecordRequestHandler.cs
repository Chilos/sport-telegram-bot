using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Commands.CompleteExerciseRecord
{
    public class CompleteExerciseRecordRequestHandler: IRequestHandler<CompleteExerciseRecordRequest>
    {
        private readonly IBotDbContext _botDbContext;

        public CompleteExerciseRecordRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }

        public async Task<Unit> Handle(CompleteExerciseRecordRequest request, CancellationToken cancellationToken)
        {
            var (id, repetitions, weight) = request;
            var exerciseRecord = await _botDbContext
                .ExerciseRecord
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);
            exerciseRecord.Repetitions = repetitions;
            exerciseRecord.Weight = weight;
            await _botDbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}