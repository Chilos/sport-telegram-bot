using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord
{
    public sealed class CreateTrainRecordRequestHandler: IRequestHandler<CreateTrainRecordRequest>
    {
        private readonly IBotDbContext _botDbContext;
        
        public CreateTrainRecordRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<Unit> Handle(CreateTrainRecordRequest request, CancellationToken cancellationToken)
        {
            var (user, dateTime) = request;
            
            _botDbContext.TrainRecord.Add(new Domain.TrainRecord
            {
                User = user,
                DateAt = dateTime
            });
            await _botDbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}