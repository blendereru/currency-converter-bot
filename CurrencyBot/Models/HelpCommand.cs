using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Models;

public class HelpCommand : ICommand
{

    public TelegramBotClient Client => Bot.GetTelegramBot();
    public string Name => "/help";
    public async Task ExecuteAsync(Update update)
    {
        if (string.IsNullOrEmpty(update.Message.Text))
        {
            return;
        }

        var message = "Here is a list of commands you can access:";
        await Client.SendMessage(update.Message.Chat.Id, message);
    }
}