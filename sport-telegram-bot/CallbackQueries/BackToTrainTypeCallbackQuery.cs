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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public class BackToTrainTypeCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly int _messageId;

    public BackToTrainTypeCallbackQuery(ITelegramBotClient client, IMediator mediator, long chatId, int messageId)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _messageId = messageId;
    }
    
    public async Task Execute(string[] payload, CancellationToken cancellationToken)
    {
        var trainId = long.Parse(payload.First());
        var trainTypeChooseMenu = await TrainTypeChooseMenuAsync(trainId, cancellationToken);
        
        await _client.EditMessageTextAsync(_chatId,
            _messageId,
            "Выбирете упражнение для добавления в тренеровку.",
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