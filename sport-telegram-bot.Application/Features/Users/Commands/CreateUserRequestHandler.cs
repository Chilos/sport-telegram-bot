using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Users.Commands
{
    public sealed class CreateUserRequestHandler : IRequestHandler<CreateUserRequest>
    {
        private readonly IBotDbContext _botDbContext;

        public CreateUserRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }

        public async Task<Unit> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var fundedUser = await _botDbContext.Users.FirstOrDefaultAsync(u => u.TelegramId == request.TelegramId,
                cancellationToken: cancellationToken);
            if (fundedUser is not null)
            {
                return Unit.Value;
            }
            var user = new User
            {
                Id = null,
                Name = request.Name,
                TelegramId = request.TelegramId
            };

            await _botDbContext.Users.AddAsync(user, cancellationToken);
            await _botDbContext.SaveChangesAsync(cancellationToken);
            
            return Unit.Value;
        }
    }
}