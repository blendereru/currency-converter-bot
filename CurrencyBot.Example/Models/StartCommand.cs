using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Example.Models;

public class StartCommand : ICommand
{
    public TelegramBotClient Client => Bot.GetTelegramBot();
    public bool IsActive => false;
    public string Name { get; set; } = "/start";
    public async Task ExecuteAsync(Update update)
    {
        var chatId = update.Message.Chat.Id;
        await Client.SendTextMessageAsync(chatId, "Hello!");
    }
}