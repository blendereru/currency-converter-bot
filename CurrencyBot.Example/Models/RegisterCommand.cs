using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Example.Models;

public class RegisterCommand : ICommand
{
    public TelegramBotClient Client => Bot.GetTelegramBot();
    public string Name { get; set; } = "/register";
    
    private bool _isActive;
    public bool IsActive => _isActive;
    
    private string? _phone;
    private string? _name;

    public async Task ExecuteAsync(Update update)
    {
        if (update.Message == null)
        {
            return;
        } 

        var chatId = update.Message.Chat.Id;

        if (!_isActive)
        {
            _isActive = true; // Start registration process
            await Client.SendTextMessageAsync(chatId, "Enter phone number!");
        }
        else if (_phone == null) // Ask for name
        {
            _phone = update.Message.Text;
            await Client.SendTextMessageAsync(chatId, "Enter your name!");
        }
        else // Complete registration
        {
            _name = update.Message.Text;
            await Client.SendTextMessageAsync(chatId, "Congrats on registration!");
            _isActive = false; // End registration process
            Reset();
        }
    }

    private void Reset()
    {
        _phone = null;
        _name = null;
    }
}
