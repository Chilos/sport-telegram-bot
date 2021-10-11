using MediatR;

namespace sport_telegram_bot.Application.Features.Users.Commands
{
    public sealed record CreateUserRequest(long TelegramId, string Name) : IRequest;

}