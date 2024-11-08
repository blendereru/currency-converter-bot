using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyBot.Models;

public class StartCommand : ICommand
{
    public TelegramBotClient Client => Bot.GetTelegramBot();
    public bool IsActive => false;
    public string Name => "/start";
    
    public async Task ExecuteAsync(Update update)
    {
        if (update.Message == null)
        {
            return;
        }
        var chatId = update.Message.Chat.Id;
        var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton("Convert Currency");
        
        await Client.SendMessage(
            chatId: chatId,
            text: "Welcome to the Currency Converter Bot! Tap the button below to start a conversion.",
            replyMarkup: replyMarkup
        );
    }
}