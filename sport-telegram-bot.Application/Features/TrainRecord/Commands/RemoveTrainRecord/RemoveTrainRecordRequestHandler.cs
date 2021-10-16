using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord
{
    public sealed class RemoveTrainRecordRequestHandler: IRequestHandler<RemoveTrainRecordRequest>
    {
        private readonly IBotDbContext _botDbContext;
        
        public RemoveTrainRecordRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<Unit> Handle(RemoveTrainRecordRequest request, CancellationToken cancellationToken)
        {
            var train = await _botDbContext.TrainRecord
                .FirstOrDefaultAsync(t => t.Id == request.TrainId, cancellationToken);
            _botDbContext.TrainRecord.Remove(train);
            await _botDbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}