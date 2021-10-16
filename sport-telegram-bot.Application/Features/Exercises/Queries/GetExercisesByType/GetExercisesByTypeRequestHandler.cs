using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesByType
{
    public sealed class GetExercisesByTypeRequestHandler: IRequestHandler<GetExercisesByTypeRequest, IReadOnlyList<Exercise>>
    {
        private readonly IBotDbContext _botDbContext;
        
        public GetExercisesByTypeRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        public async Task<IReadOnlyList<Exercise>> Handle(GetExercisesByTypeRequest request, CancellationToken cancellationToken)
        {
            return await _botDbContext.Exercises
                .Where(e => e.Type == request.Type)
                .ToListAsync(cancellationToken);
        }
    }
}