using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.ExerciseRecord.Commands.RemoveExerciseRecordById;
using sport_telegram_bot.Application.Features.TrainRecord.Commands.RemoveTrainRecord;
using sport_telegram_bot.Domain;
using Telegram.Bot;

namespace sport_telegram_bot.CallbackQueries;

public class RemoveExerciseCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly User _user;
    private readonly long _chatId;
    private readonly int _messageId;
    private readonly Dictionary<long, List<(string messageText, int messageId)>> _exercises;

    public RemoveExerciseCallbackQuery(ITelegramBotClient client, IMediator mediator, long chatId, int messageId,
        Dictionary<long, List<(string messageText, int messageId)>> exercises, User user)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _messageId = messageId;
        _exercises = exercises;
        _user = user;
    }
    
    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var (trainId, exerciseId) = ParsePayload(payloadArray);
        await _mediator.Send(new RemoveExerciseRecordByIdRequest(exerciseId), cancellationToken);
        await _client.DeleteMessageAsync(_chatId, 
            _messageId,
            cancellationToken);
        _exercises[_user.TelegramId]
            .Remove(_exercises[_user.TelegramId].FirstOrDefault(e => e.messageId == _messageId));
    }
    
    private (int trainId, int exerciseId) ParsePayload(string[] payload)
    {
        try
        {
            var trainId = int.Parse(payload[0]);
            var exerciseId = int.Parse(payload[1]);

            return (trainId, exerciseId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}