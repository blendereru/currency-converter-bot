using CurrencyBot.Example.Models;
using Telegram.Bot.Types;

namespace CurrencyBot.Example.Models;
public class CommandExecutor
{
    private readonly List<ICommand> _commands;
    private ICommand? _activeCommand; // Track the active command

    public CommandExecutor()
    {
        _commands = new List<ICommand>()
        {
            new StartCommand(),
            new RegisterCommand()
        };
    }

    public async Task GetUpdate(Update update)
    {
        // If there’s an active command, continue its execution
        if (_activeCommand != null && _activeCommand.IsActive)
        {
            await _activeCommand.ExecuteAsync(update);

            // If the command has completed, reset _activeCommand
            if (!_activeCommand.IsActive)
            {
                _activeCommand = null;
            }
        }
        else
        {
            // Check if any command should be initiated
            var msg = update.Message;
            if (msg == null || string.IsNullOrEmpty(msg.Text))
            {
                return;
            }
        
            foreach (var command in _commands)
            {
                if (command.Name == msg.Text)
                {
                    _activeCommand = command; // Set as active command
                    await command.ExecuteAsync(update);
                    break;
                }
            }
        }
    }
}