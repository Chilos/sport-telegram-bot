using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace sport_telegram_bot.Commands;

public sealed class AddTrainCommand
{
    private readonly ITelegramBotClient _client;
    private readonly long _chatId;
    
    public AddTrainCommand(ITelegramBotClient client, long chatId)
    {
        _client = client;
        _chatId = chatId;
    }
    
    public Task Execute(CancellationToken cancellationToken)
    {
        return _client.SendTextMessageAsync(_chatId,
            "Выберите день тренировки", 
            replyMarkup: DateChooseMenu(),  
            cancellationToken: cancellationToken);
    }
    
    private InlineKeyboardMarkup DateChooseMenu()
    {
        var buttons = new List<InlineKeyboardButton>();
        for (var i = 0; i < 7; i++)
        {
            var currentDate = DateTime.Now.AddDays(i);
            var data = $"train_date:{currentDate:d}";
            buttons.Add(InlineKeyboardButton.WithCallbackData($"{currentDate:dd.MM}", data));
        }
            
        return new InlineKeyboardMarkup(buttons);
    }
}