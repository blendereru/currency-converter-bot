using Telegram.Bot.Types;

namespace CurrencyBot.Example.Models;

public class UpdateDistributor
{
    private readonly Dictionary<long, CommandExecutor> _listeners = new Dictionary<long, CommandExecutor>();
    public async Task GetUpdate(Update update)
    {
        if (update.Message == null)
        {
            return;
        }

        var chatId = update.Message.Chat.Id;

        // Try to get an existing CommandExecutor for the chat, or create a new one
        if (!_listeners.TryGetValue(chatId, out var executor))
        {
            executor = new CommandExecutor();
            _listeners[chatId] = executor;
        }
        // Forward the update to the CommandExecutor
        await executor.GetUpdate(update);
    }
}