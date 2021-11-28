using System.Threading;
using System.Threading.Tasks;
using MediatR;
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

        public Task<Unit> Handle(CompleteExerciseRecordRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}