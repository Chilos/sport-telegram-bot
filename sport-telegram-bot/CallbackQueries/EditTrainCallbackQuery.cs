using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesById;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExerciseTypes;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.AddExerciseToTrainRecord;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordById;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public sealed class EditTrainCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly User _user;
    private readonly Dictionary<long, List<(string messageText, int messageId)>> _exercises;

    public EditTrainCallbackQuery(ITelegramBotClient client, IMediator mediator, long chatId,
        Dictionary<long, List<(string messageText, int messageId)>> exercises, User user)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _exercises = exercises;
        _user = user;
    }
    
    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var trainId = int.Parse(payloadArray.First());
        var train = await _mediator.Send(new GetTrainRecordByIdRequest(trainId));
        foreach (var exercise in train.Exercises)
        {
            var notFormattingText = $"В тренировку добавлено упражнение: {Environment.NewLine}" +
                                    $"<a href=\"{exercise.Exercise.ImageUrl}\">{exercise.Exercise.Description}</a>";
            var message = await _client.SendTextMessageAsync(_chatId,
                notFormattingText,
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Удалить", $"remove_exercise:{trainId}:{exercise.Id}")
                }), 
                cancellationToken: cancellationToken);
            if (!_exercises.ContainsKey(_user.TelegramId))
            {
                _exercises[_user.TelegramId] = new List<(string messageText, int messageId)>();
            }
            _exercises[_user.TelegramId].Add((notFormattingText, message.MessageId));
        }
        
        await _client.SendTextMessageAsync(_chatId,
            "Выбирете упражнение для добавления в тренеровку.",
            replyMarkup: await TrainTypeChooseMenuAsync(trainId, cancellationToken),
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