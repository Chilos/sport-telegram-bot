using System;
using MediatR;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord
{
    public sealed record CreateTrainRecordRequest(User User, DateOnly DateAt) : IRequest;
}