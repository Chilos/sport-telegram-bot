using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public sealed class AddExerciseCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly string _messageText;
    private readonly long _chatId;
    private readonly int _messageId;

    public AddExerciseCallbackQuery(ITelegramBotClient client, IMediator mediator, string messageText, long chatId, int messageId)
    {
        _client = client;
        _mediator = mediator;
        _messageText = messageText;
        _chatId = chatId;
        _messageId = messageId;
    }
    
    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var (exerciseId, trainId) = ParsePayload(payloadArray);
        var exercise = await _mediator.Send(new GetExercisesByIdRequest(exerciseId), cancellationToken);
        await _mediator.Send(new AddExerciseToTrainRecordRequest(exerciseId, trainId), cancellationToken);
        await _client.EditMessageTextAsync(_chatId, 
            _messageId, 
            $"{_messageText} {Environment.NewLine}" +
            $"{exercise.Description}",
            replyMarkup: await TrainTypeChooseMenuAsync(trainId, cancellationToken), 
            cancellationToken: cancellationToken);
    }
    
    private (int exerciseId, int trainId) ParsePayload(string[] payload)
    {
        try
        {
            var exerciseId = int.Parse(payload[0]);
            var trainId = int.Parse(payload[1]);

            return (exerciseId, trainId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
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