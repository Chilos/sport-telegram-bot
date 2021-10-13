using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes
{
    public class GetExerciseTypesRequestHandler: IRequestHandler<GetExerciseTypesRequest, IReadOnlyList<string>>
    {
        private readonly IBotDbContext _botDbContext;
        
        public GetExerciseTypesRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<IReadOnlyList<string>> Handle(GetExerciseTypesRequest request, CancellationToken cancellationToken)
        {
            return await _botDbContext.Exercises.Select(e => e.Type).Distinct().ToListAsync(cancellationToken);
        }
    }
}