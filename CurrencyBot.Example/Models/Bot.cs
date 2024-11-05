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
        client = new TelegramBotClient("7616666641:AAEkKxP0g5diKhkkHDiOyo2alqOl5DLNYBY");
        return client;
    }
}