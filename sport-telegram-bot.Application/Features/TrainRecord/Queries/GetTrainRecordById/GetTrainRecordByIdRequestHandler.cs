using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordById
{
    public class GetTrainRecordByIdRequestHandler: IRequestHandler<GetTrainRecordByIdRequest, Domain.TrainRecord>
    {
        private readonly IBotDbContext _botDbContext;

        public GetTrainRecordByIdRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }

        public Task<Domain.TrainRecord> Handle(GetTrainRecordByIdRequest request, CancellationToken cancellationToken)
        {
            return _botDbContext.TrainRecord
                .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == request.TrainId, cancellationToken);
        }
    }
}