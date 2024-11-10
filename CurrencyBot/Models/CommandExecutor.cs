using CurrencyBot.Models;
using CurrencyBot.Models;
using Telegram.Bot.Types;

namespace CurrencyBot.Models;
public class CommandExecutor
{
    private readonly List<ICommand> _commands;
    private ICommand? _activeCommand;

    public CommandExecutor(HttpClient httpClient, IConfiguration configuration)
    {
        _commands = new List<ICommand>()
        {
            new StartCommand(),
            new HelpCommand(),
            new ConvertCurrencyCommand(httpClient, configuration)
        };
    }

    public async Task GetUpdate(Update update)
    {
        Console.WriteLine("command executor is initted");
        var msg = update.Message;
        if (msg == null || string.IsNullOrEmpty(msg.Text))
        {
            return;
        }
        if (msg.Text == "Convert Currency")
        {
            var convertCommand = _commands.OfType<ConvertCurrencyCommand>().FirstOrDefault();
            if (convertCommand != null)
            {
                _activeCommand = convertCommand;
                await _activeCommand.ExecuteAsync(update);
                return; // Exit after handling "Convert Currency"
            }
        }
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
            foreach (var command in _commands)
            {
                if (command.Name == msg.Text)
                {
                    _activeCommand = command;
                    await _activeCommand.ExecuteAsync(update);
                    break;
                }
            }
        }
    }
}
