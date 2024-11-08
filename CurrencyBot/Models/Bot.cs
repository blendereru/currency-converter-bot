using Telegram.Bot;

namespace CurrencyBot.Models;

public static class Bot
{
    // non-lazy, non-asynchronous singleton
    private static TelegramBotClient? Client { get; set; }
    public static TelegramBotClient GetTelegramBot()
    {
        if (Client != null)
        {
            return Client;
        }
        Client = new TelegramBotClient("your_currency_bot");
        return Client;
    }
}