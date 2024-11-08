using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Models;

public class HelpCommand : ICommand
{

    public TelegramBotClient Client => Bot.GetTelegramBot();
    public bool IsActive => false;
    public string Name => "/help";
    public async Task ExecuteAsync(Update update)
    {
        if (update.Message == null)
        {
            return;
        }
        var message = "Here is a list of commands you can access:\n\n" +
                      "/start - Start the bot and get a welcome message\n" +
                      "/help - Display this list of available commands\n" +
                      "/convert - Convert an amount from one currency to another (e.g., '100 USD')\n" +
                      "/stop - Stop any ongoing process\n\n" +
                      "Just type the command to begin!";
        await Client.SendMessage(update.Message.Chat.Id, message);
    }
}