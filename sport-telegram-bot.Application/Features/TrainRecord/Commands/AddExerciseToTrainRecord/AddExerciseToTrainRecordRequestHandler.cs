using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord
{
    public sealed class AddExerciseToTrainRecordRequestHandler: IRequestHandler<AddExerciseToTrainRecordRequest>
    {
        private readonly IBotDbContext _botDbContext;
        
        public AddExerciseToTrainRecordRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<Unit> Handle(AddExerciseToTrainRecordRequest request, CancellationToken cancellationToken)
        {
            var (exerciseId, trainId) = request;
            var exercise = await _botDbContext.Exercises
                    .FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);
            var trainRecord = await _botDbContext.TrainRecord
                .FirstOrDefaultAsync(t => t.Id == trainId, cancellationToken);
            //TODO: Надо запилить проверку на null
            _botDbContext.ExerciseRecord.Add(new Domain.ExerciseRecord
            {
                Exercise = exercise,
                TrainRecord = trainRecord,
                Repetitions = null,
                Weight = null
            });

            await _botDbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}