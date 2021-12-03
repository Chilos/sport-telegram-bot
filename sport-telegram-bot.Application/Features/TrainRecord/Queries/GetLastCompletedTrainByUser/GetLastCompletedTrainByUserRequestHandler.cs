using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetLastCompletedTrainByUser;

public class GetLastCompletedTrainByUserRequestHandler : 
    IRequestHandler<GetLastCompletedTrainByUserRequest, Domain.TrainRecord>
{
    private readonly IBotDbContext _botDbContext;

    public GetLastCompletedTrainByUserRequestHandler(IBotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<Domain.TrainRecord> Handle(GetLastCompletedTrainByUserRequest request, CancellationToken cancellationToken)
    {
        return await _botDbContext.TrainRecord
            .Include(t => t.Exercises)
            .ThenInclude(e => e.Exercise)
            .Where(t => t.User.Id == request.UserId && 
                        t.Exercises.Any(e => e.Exercise.Id == request.ExerciseId && 
                                             e.Weight != null && 
                                             e.Repetitions != null))
            .OrderByDescending(t=>t.DateAt)
            .Take(1)
            .FirstOrDefaultAsync(cancellationToken);
    }
}