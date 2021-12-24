using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Exercises.Queries.GetExercisesByType;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public class TrainTypeCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly string _messageText;
    private readonly long _chatId;
    private readonly int _messageId;

    public TrainTypeCallbackQuery(ITelegramBotClient client, IMediator mediator, long chatId, int messageId, string messageText)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _messageId = messageId;
        _messageText = messageText;
    }

    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var payload = ParsePayload(payloadArray);
        
        await _client.EditMessageTextAsync(_chatId, 
            _messageId, 
            _messageText, 
            replyMarkup: await ExerciseChooseMenuAsync(payload.type, payload.id, cancellationToken),
            cancellationToken: cancellationToken);
    }

    private (string type, long id) ParsePayload(string[] payload)
    {
        try
        {
            var trainType = payload[0];
            var trainId = long.Parse(payload[1]);

            return (trainType, trainId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    private async Task<InlineKeyboardMarkup> ExerciseChooseMenuAsync(string type, long trainId, CancellationToken cancellationToken)
    {
        var exercises = await _mediator.Send(new GetExercisesByTypeRequest(type), cancellationToken);
        var buttons = exercises.Select(e => new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(e.Description, $"add_exercise:{e.Id}:{trainId}")
        }).ToList();
        buttons.Add(new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("Назад", $"back_to_train_type:{trainId}")
        });

        return new InlineKeyboardMarkup(buttons);
    }
}