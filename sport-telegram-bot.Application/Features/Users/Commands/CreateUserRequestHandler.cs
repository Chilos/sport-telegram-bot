using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Users.Commands
{
    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest>
    {
        private readonly IBotDbContext _botDbContext;

        public CreateUserRequestHandler(IBotDbContext botDbContext)
        {
            _botDbContext = botDbContext;
        }

        public async Task<Unit> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
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