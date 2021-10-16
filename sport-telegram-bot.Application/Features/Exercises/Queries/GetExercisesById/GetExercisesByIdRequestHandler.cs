using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById
{
    public sealed class GetExercisesByIdRequestHandler: IRequestHandler<GetExercisesByIdRequest, Exercise>
    {
        private readonly IBotDbContext _botDbContext;
        
        public GetExercisesByIdRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        public async Task<Exercise> Handle(GetExercisesByIdRequest request, CancellationToken cancellationToken)
        {
            return await _botDbContext.Exercises
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        }
    }
}