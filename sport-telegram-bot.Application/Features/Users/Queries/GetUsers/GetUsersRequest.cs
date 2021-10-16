using System.Collections.Generic;
using MediatR;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.Users.Queries.GetUsers
{
    public sealed record GetUsersRequest(long? TelegtamId): IRequest<User>;
}