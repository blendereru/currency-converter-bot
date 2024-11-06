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
        client = new TelegramBotClient("7616666641:AAFdZ6c1Mm6v8SaWZcv84W3dzh0Cz9Rzp0A");
        return client;
    }
}