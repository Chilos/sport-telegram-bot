using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Application.Features.TrainRecord.Queries.GetActiveTrainsByUser
{
    public sealed class GetActiveTrainsByUserRequestHandler: 
        IRequestHandler<GetActiveTrainsByUserRequest, IReadOnlyList<Domain.TrainRecord>>
    {
        private readonly IBotDbContext _botDbContext;

        public GetActiveTrainsByUserRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        
        public async Task<IReadOnlyList<Domain.TrainRecord>> Handle(GetActiveTrainsByUserRequest request, 
            CancellationToken cancellationToken)
        {
            return await _botDbContext.TrainRecord
                .Where(t => t.User.Id == request.UserId && t.DateAt >= DateTime.Now.Date)
                .ToListAsync(cancellationToken);
        }
    }
}