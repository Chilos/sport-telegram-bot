using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public sealed class AddExerciseCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly User _user;
    private readonly long _chatId;
    private readonly int _messageId;
    private readonly Dictionary<long, List<(string messageText, int messageId)>> _exercises;

    public AddExerciseCallbackQuery(ITelegramBotClient client, IMediator mediator, User user, long chatId, int messageId,
        Dictionary<long, List<(string messageText, int messageId)>> exercises)
    {
        _client = client;
        _mediator = mediator;
        _user = user;
        _chatId = chatId;
        _messageId = messageId;
        _exercises = exercises;
    }
    
    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var (exerciseId, trainId) = ParsePayload(payloadArray);
        var exercise = await _mediator.Send(new GetExercisesByIdRequest(exerciseId), cancellationToken);
        var exerciseRecordId = await _mediator.Send(new AddExerciseToTrainRecordRequest(exerciseId, trainId), cancellationToken);
        var notFormattingText = $"В тренировку добавлено упражнение: {Environment.NewLine}" +
                                $"<a href=\"{exercise.ImageUrl}\">{exercise.Description}</a>";
        var message = await _client.EditMessageTextAsync(_chatId, 
            _messageId,
            notFormattingText,
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Удалить", $"remove_exercise:{trainId}:{exerciseRecordId}")
            }), 
            cancellationToken: cancellationToken);
        await _client.SendTextMessageAsync(_chatId,
            "Выбирете упражнение для добавления в тренеровку.",
            replyMarkup: await TrainTypeChooseMenuAsync(trainId, cancellationToken),
            cancellationToken: cancellationToken);
        
        if (!_exercises.ContainsKey(_user.TelegramId))
        {
            _exercises[_user.TelegramId] = new List<(string messageText, int messageId)>();
        }
        _exercises[_user.TelegramId].Add((notFormattingText, message.MessageId));
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