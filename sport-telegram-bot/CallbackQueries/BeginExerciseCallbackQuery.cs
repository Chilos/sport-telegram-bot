using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetLastCompletedTrainByUser;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetTrainRecordById;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.CallbackQueries;

public class BeginExerciseCallbackQuery
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly User _user;
    private readonly Dictionary<long, (int exerciseId, int previosMessageId)>  _questions;
    private readonly string _messageText;
    private readonly long _chatId;
    private readonly int _messageId;

    public BeginExerciseCallbackQuery(ITelegramBotClient client, IMediator mediator, 
        User user, string messageText, long chatId, int messageId,
        Dictionary<long, (int exerciseId, int previosMessageId)> questions)
    {
        _client = client;
        _mediator = mediator;
        _user = user;
        _messageText = messageText;
        _chatId = chatId;
        _messageId = messageId;
        _questions = questions;
    }

    public async Task Execute(string[] payloadArray, CancellationToken cancellationToken)
    {
        var (beginExerciseId, beginExerciseTrainId) = ParsePayload(payloadArray);
        var beginTrain = await _mediator.Send(new GetTrainRecordByIdRequest(beginExerciseTrainId), cancellationToken);
        var beginExercise = beginTrain.Exercises.FirstOrDefault(e => e.Id == beginExerciseId);
        var lastTrain = await _mediator.Send(new GetLastCompletedTrainByUserRequest(_user.Id.Value, beginExercise.Exercise.Id), cancellationToken);
        await _client.DeleteMessageAsync(_chatId, _messageId, cancellationToken);
        var message = await _client.SendPhotoAsync(_chatId, new InputOnlineFile(beginExercise!.Exercise.ImageUrl),
            caption: $"{beginExercise!.Exercise.Description} {Environment.NewLine}" +
                     $" {Environment.NewLine}" +
                     $"Прошлый результат: {Environment.NewLine}" +
                     $"число повторений: {lastTrain?.Exercises.Find(e => e.Exercise.Id == beginExercise.Exercise.Id).Repetitions} {Environment.NewLine}" +
                     $"вес: {lastTrain?.Exercises.Find(e => e.Exercise.Id == beginExercise.Exercise.Id).Weight} {Environment.NewLine}" +
                     $" {Environment.NewLine}" +
                     $"Введите вес и число повторений по примеру:{Environment.NewLine}" +
                     $"число повторений-вес",
            replyMarkup: BeginExercisesChooseMenu(beginTrain),
            cancellationToken: cancellationToken);
        _questions[_user.TelegramId] = (beginExerciseId, message.MessageId);
    }
    
    private (int beginExerciseId, int beginExerciseTrainId) ParsePayload(string[] payload)
    {
        try
        {
            var beginExerciseId = int.Parse(payload[0]);
            var beginExerciseTrainId = int.Parse(payload[1]);

            return (beginExerciseId, beginExerciseTrainId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
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