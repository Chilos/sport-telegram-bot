using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.ExerciseRecord.Commands.CompleteExerciseRecord;
using sport_telegram_bot.Domain;
using sport_telegram_bot.Dtos;
using Telegram.Bot;

namespace sport_telegram_bot.Commands;

public class EnterExerciseParamMessage
{
    private readonly Dictionary<long, (int exerciseId, int previosMessageId)>  _questions;
    private readonly string _message;
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly User _user;

    public EnterExerciseParamMessage(
        ITelegramBotClient client,
        long chatId,
        IMediator mediator,
        User user,
        Dictionary<long, (int exerciseId, int previosMessageId)> questions,
        string message)
    {
        _client = client;
        _chatId = chatId;
        _mediator = mediator;
        _user = user;
        _questions = questions;
        _message = message;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        var telegramId = _user.TelegramId;
        
        if (!_questions.ContainsKey(telegramId)) return;
        
        var exerciseParam = ExerciseParamDto.Parse(_message);
        var (exerciseId, previousMessageId) = _questions[telegramId];
        
        var request = new CompleteExerciseRecordRequest(
            exerciseId,
            exerciseParam.Repetitions,
            exerciseParam.Weight);
        
        await _mediator.Send(request, cancellationToken);
        
        await _client.DeleteMessageAsync(_chatId,
            previousMessageId, cancellationToken);
        await _client.SendTextMessageAsync(_chatId,
            "Сохранено!",
            cancellationToken: cancellationToken);
        
        _questions.Remove(telegramId);
    }
}