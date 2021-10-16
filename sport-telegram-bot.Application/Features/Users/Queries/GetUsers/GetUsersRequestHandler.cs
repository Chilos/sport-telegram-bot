using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersRequestHandler : IRequestHandler<GetUsersRequest, User>
    {
        private readonly IBotDbContext _botDbContext;
        
        public GetUsersRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }
        public async Task<User> Handle(GetUsersRequest request, CancellationToken cancellationToken)
        {
            return await _botDbContext.Users
                .FirstOrDefaultAsync(u => u.TelegramId == request.TelegtamId, cancellationToken);
        }
    }
}