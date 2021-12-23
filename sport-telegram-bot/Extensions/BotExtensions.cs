using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.Users.Commands.CreateUser;
using sport_telegram_bot.Application.Features.Users.Queries.GetUsers;
using sport_telegram_bot.CallbackQueries;
using sport_telegram_bot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace sport_telegram_bot.Extensions;

public static class BotExtensions
{
    public static async Task HandleMessageAsync(this ITelegramBotClient client, Message message, IMediator mediator,
        Dictionary<long, (int exerciseId, int previosMessageId)>  questions,
        Dictionary<long, List<(string messageText, int messageId)>> exercises,  CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUsersRequest(message.From!.Id), cancellationToken);
        if (user == null)
        {
            await mediator.Send(new CreateUserRequest(message.From!.Id, message.From.Username), cancellationToken);
            user = await mediator.Send(new GetUsersRequest(message.From!.Id), cancellationToken);
        }
        var chatId = message.Chat.Id;

        var task = message.Text switch
        {
            "/start" => new StartCommand(client, chatId)
                .Execute(cancellationToken),
            "/add_train" => new AddTrainCommand(client, chatId)
                .Execute(cancellationToken),
            "/remove_train" => new RemoveTrainCommand(client, mediator, chatId, user)
                .Execute(cancellationToken),
            "/begin_train" => new BeginTrainCommand(client, mediator, chatId, user)
                .Execute(cancellationToken),
            _ => new EnterExerciseParamMessage(client, chatId, mediator, user, questions, message.Text)
                .Execute(cancellationToken)
        };
        
        await task;
    }

    public static async Task HandleCallbackQuery(this ITelegramBotClient client, CallbackQuery callbackQuery,
        IMediator mediator, Dictionary<long, (int exerciseId, int previosMessageId)>  questions,
        Dictionary<long, List<(string messageText, int messageId)>> exercises,
        CancellationToken cancellationToken)
    {
        var messageId = callbackQuery.Message!.MessageId;
        var messageText = callbackQuery.Message!.Text;
        var data = callbackQuery.Data!.Split(":");
        if (data.Length < 2) return;
        var callbackType = data[0];
        var payload = data.Skip(1).ToArray();
        
        var chatId = callbackQuery.Message!.Chat.Id;
        var userTelegramId = callbackQuery.From!.Id;
        var user = await mediator.Send(new GetUsersRequest(userTelegramId), cancellationToken);
        if (user == null)
        {
            await mediator.Send(new CreateUserRequest(userTelegramId, callbackQuery.From.Username), cancellationToken);
            user = await mediator.Send(new GetUsersRequest(userTelegramId), cancellationToken);
        }
        
        var task = callbackType switch
        {
            "train_date" => new TrainDateCallbackQuery(user, chatId, messageId, mediator, client)
                .Execute(payload, cancellationToken),
            "train_type" => new TrainTypeCallbackQuery(client, mediator, chatId, messageId, messageText)
                .Execute(payload, cancellationToken),
            "add_exercise" => new AddExerciseCallbackQuery(client, mediator, user, chatId, messageId, exercises)
                .Execute(payload, cancellationToken),
            "confirm_train" => new ConfirmTrainCallbackQuery(client, user, exercises, chatId, messageId, messageText)
                .Execute(payload, cancellationToken),
            "remove_train" => new RemoveTrainCallbackQuery(client, mediator, chatId, messageId)
                .Execute(payload, cancellationToken),
            "begin_exercise" => new BeginExerciseCallbackQuery(client, mediator, user, messageText, chatId, messageId, questions)
                .Execute(payload, cancellationToken),
            "back_to_train_type" => new BackToTrainTypeCallbackQuery(client, mediator, chatId, messageId)
                .Execute(payload, cancellationToken),
            _ => throw new Exception($"not found CallbackQuery {callbackType}")
        };
        
        await task;
        
    }
}