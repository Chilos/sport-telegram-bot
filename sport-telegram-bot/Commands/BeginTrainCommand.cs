using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.Commands;

public sealed class BeginTrainCommand
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly User _user;

    public BeginTrainCommand(ITelegramBotClient client, IMediator mediator, long chatId, User user)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _user = user;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        if (_user.Id == null)
            return;
        var beginTrain = await _mediator
            .Send(new GetTrainRecordByDateRequest(DateOnly.FromDateTime(DateTime.Now), _user.Id.Value),
                cancellationToken);
        if (beginTrain == null)
        {
            await _client.SendTextMessageAsync(_chatId,
                "На сегодня нет запланированных тренировок",
                cancellationToken: cancellationToken);
            return;
        }

        var exercisesChooseMenu = BeginExercisesChooseMenu(beginTrain);
        if (!exercisesChooseMenu.InlineKeyboard.Any())
        {
            await _client.SendTextMessageAsync(_chatId,
                "Вы завершили тренирку на сегодня, поздравляем",
                cancellationToken: cancellationToken);
            return;
        }
        await _client.SendTextMessageAsync(_chatId,
            "Выберите упражнение", 
            replyMarkup: exercisesChooseMenu,
            cancellationToken: cancellationToken);
    }
    
    private InlineKeyboardMarkup BeginExercisesChooseMenu(TrainRecord train)
    {
            
        var buttons = train.Exercises
            .Where(e => e.Weight == null && e.Repetitions == null)
            .Select(e => new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(e.Exercise.Description, $"begin_exercise:{e.Id}:{train.Id}")
            }).ToList();

        return new InlineKeyboardMarkup(buttons);
    }
}