using MediatR;

namespace sport_telegram_bot.Application.Features.Users.Commands
{
    public sealed class CreateUserRequest : IRequest
    {
        public long TelegramId { get; }
        public string Name { get; }

        public CreateUserRequest(long telegramId, string name)
        {
            TelegramId = telegramId;
            Name = name;
        }
    }

}