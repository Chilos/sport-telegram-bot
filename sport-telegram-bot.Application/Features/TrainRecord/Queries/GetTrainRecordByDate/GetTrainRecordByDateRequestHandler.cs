using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate
{
    public sealed class GetTrainRecordByDateRequestHandler: 
        IRequestHandler<GetTrainRecordByDateRequest, Domain.TrainRecord>
    {
        private readonly IBotDbContext _botDbContext;

        public GetTrainRecordByDateRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<Domain.TrainRecord> Handle(GetTrainRecordByDateRequest request, 
            CancellationToken cancellationToken)
        {
            return await _botDbContext.TrainRecord
                .FirstOrDefaultAsync(t => t.DateAt == request.date, cancellationToken);
        }
    }
}