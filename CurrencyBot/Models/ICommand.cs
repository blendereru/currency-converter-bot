using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Models;

public interface ICommand
{
    TelegramBotClient Client { get; }
    string Name { get; }
    Task ExecuteAsync(Update update);
}