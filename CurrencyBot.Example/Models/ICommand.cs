using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Example.Models;

public interface ICommand
{
    TelegramBotClient Client { get; }
    string Name { get; set; }
    bool IsActive { get; }
    Task ExecuteAsync(Update update);
}