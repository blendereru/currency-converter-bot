using Telegram.Bot;

namespace CurrencyBot.Models;

public static class Bot
{
    // non-lazy, non-asynchronous singleton
    private static TelegramBotClient? client { get; set; }
    public static TelegramBotClient GetTelegramBot()
    {
        if (client != null)
        {
            return client;
        }
        client = new TelegramBotClient("your_currency_bot");
        return client;
    }
}