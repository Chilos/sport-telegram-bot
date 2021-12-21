using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.CreateTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordByDate;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public class TrainDateCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly User _user;
    private readonly int _messageId;

    public TrainDateCallbackQuery(User user, long chatId, int messageId, IMediator mediator, ITelegramBotClient client)
    {
        _user = user;
        _chatId = chatId;
        _messageId = messageId;
        _mediator = mediator;
        _client = client;
    }

    public async Task Execute(string[] payload, CancellationToken cancellationToken)
    {
        
        var date = DateOnly.Parse(payload.First());
        var train = await _mediator
            .Send(new GetTrainRecordByDateRequest(date, _user.Id!.Value), cancellationToken);
        if (train is not null)
        {
            await _client.SendTextMessageAsync(_chatId,
                "На данную дату тренировка уже запланирована",
                cancellationToken: cancellationToken);
            return;
        }
                            
        await _mediator.Send(new CreateTrainRecordRequest(_user, date), cancellationToken);
        train = await _mediator
            .Send(new GetTrainRecordByDateRequest(date, _user.Id!.Value), cancellationToken);
        var trainTypeChooseMenu = await TrainTypeChooseMenuAsync(train.Id, cancellationToken);
        
        await _client.EditMessageTextAsync(_chatId,
            _messageId, 
            $"Дата тренировки выбрана: {date:dd.MM} {Environment.NewLine}" +
            $"Добавьте упражнения:", 
            replyMarkup: trainTypeChooseMenu, 
            cancellationToken: cancellationToken);
    }
    
    private async Task<InlineKeyboardMarkup> TrainTypeChooseMenuAsync(long trainId, CancellationToken cancellationToken)
    {
        var trainTypes = await _mediator.Send(new GetExerciseTypesRequest(), cancellationToken);
        var buttons = trainTypes.Select(type => new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(type, $"train_type:{type}:{trainId}")
        }).ToList();
        buttons.Add(new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("Готово", $"confirm_train:{trainId}")
        });

        return new InlineKeyboardMarkup(buttons);
    }
    
    
}