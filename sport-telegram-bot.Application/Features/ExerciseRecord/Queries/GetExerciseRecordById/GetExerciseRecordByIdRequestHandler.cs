using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.ExerciseRecord.Queries.GetExerciseRecordById
{
    public sealed class GetExerciseRecordByIdRequestHandler: IRequestHandler<GetExerciseRecordByIdRequest, Domain.ExerciseRecord>
    {
        private readonly IBotDbContext _botDbContext;

        public GetExerciseRecordByIdRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }

        public Task<Domain.ExerciseRecord> Handle(GetExerciseRecordByIdRequest request, CancellationToken cancellationToken)
        {
            return _botDbContext.ExerciseRecord
                .Include(e => e.Exercise)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        }
    }
}