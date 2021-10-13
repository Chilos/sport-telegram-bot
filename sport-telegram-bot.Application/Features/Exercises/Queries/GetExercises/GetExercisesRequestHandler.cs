using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Exercises.Queries.GetExercises
{
    public sealed class GetExercisesRequestHandler: IRequestHandler<GetExercisesRequest, IReadOnlyList<Exercise>>
    {
        private readonly IBotDbContext _botDbContext;
        
        public GetExercisesRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        public async Task<IReadOnlyList<Exercise>> Handle(GetExercisesRequest request, CancellationToken cancellationToken)
        {
            if (request.Type is null)
                return await _botDbContext.Exercises.ToListAsync(cancellationToken);
            return await _botDbContext.Exercises.Where(e => e.Type == request.Type).ToListAsync(cancellationToken);
        }
    }
}