using Telegram.Bot;

namespace CurrencyBot.Example.Models;

public class Bot
{
    // non-lazy, non-asynchronous singleton
    private static TelegramBotClient? client { get; set; }
    public static TelegramBotClient GetTelegramBot()
    {
        if (client != null)
        {
            return client;
        }
        client = new TelegramBotClient("your_telegram_token");
        return client;
    }
}