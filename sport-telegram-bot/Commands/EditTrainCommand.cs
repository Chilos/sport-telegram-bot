using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using sport_telegram_bot.Application.Features.TrainRecord.Queries.GetActiveTrainsByUser;
using sport_telegram_bot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.Commands;

public sealed class EditTrainCommand
{
    private readonly ITelegramBotClient _client;
    private readonly IMediator _mediator;
    private readonly long _chatId;
    private readonly User _user;

    public EditTrainCommand(ITelegramBotClient client, IMediator mediator, long chatId, User user)
    {
        _client = client;
        _mediator = mediator;
        _chatId = chatId;
        _user = user;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        if (_user.Id != null)
        {
            var menu = await ActiveTrainMenu(_user.Id.Value, cancellationToken);
            
            await _client.SendTextMessageAsync(_chatId,
                "Выберите день тренировки для редактирования", 
                replyMarkup: menu,  
                cancellationToken: cancellationToken);
        }
    }
    
    private async Task<InlineKeyboardMarkup> ActiveTrainMenu(int userId, CancellationToken cancellationToken)
    {
        var trains = await _mediator
            .Send(new GetActiveTrainsByUserRequest(userId), cancellationToken);
        var buttons = new List<InlineKeyboardButton>();
        foreach (var train in trains)
        {
            var data = $"edit_train:{train.Id}";
            buttons.Add(InlineKeyboardButton.WithCallbackData($"{train.DateAt:dd.MM}", data));
        }
            
        return new InlineKeyboardMarkup(buttons);
    }
}