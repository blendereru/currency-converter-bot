# CurrencyBot.Example

[example bot](./currency-converter-bot/resources/example_workflow.mp4)
The project has a single controller which is responsible for receiving updates from telegram. The `post` action method
is essential because telegram sends incoming updates as POST requests to the URL specified in `SetWebHookAsync()`:
```csharp
var bot = Bot.GetTelegramBot();
await bot.SetWebhookAsync(builder.Configuration["ngrok:Connection"]!);
```
```csharp
[HttpPost]
public async Task Post(Update update) //updates will come here
{
    if (update.Message == null)
    {
       return;
    }
    await _updateDistributor.GetUpdate(update);
}
```
This `post` action method injects `Update` class which contains all information about the incoming message. It is 
wrapped around the `Update` class using the `Newtonsoft.Json` package, configured in Program.cs:
```csharp
builder.Services.AddControllers().AddNewtonsoftJson();
```
`Post` action method calls for class `UpdateDistributor` and its method `GetUpdate()`. The purpose of the `UpdateDistributor`
class is to distribute the updates to specific `listener`. Here, the `_listeners` is a dictionary which uses specific 
`chatId` as its key and calls `GetUpdate()` of appropriate listener(user):
```csharp
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
```
If the `listener` exists in a dictionary, it immediately calls for `GetUpdate()`. Otherwise, the `listener` is added to a 
dictionary and `GetUpdate()` on a new listener is called. The purpose of `CommandExecutor` is to execute a specific command
based on the text sent by user. If user sends `/register` command, `CommandExecutor` passes the control to the `RegisterCommand`
handler, which is responsible for handling messages after user sent `/register` command:
```csharp
public async Task GetUpdate(Update update)
{
    // If there’s an active command, continue its execution
    if (...)
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
```
Inside the `if` block, we check if `_activeCommand` is not null, and if it isn't, we execute the command on the existing
field:
```csharp
if (_activeCommand != null && _activeCommand.IsActive)
{
    await _activeCommand.ExecuteAsync(update);
    // If the command has completed, reset _activeCommand
    if (!_activeCommand.IsActive)
    {
        _activeCommand = null;
    }
}
```
What is the purpose of this ? You see, after the user entered `/register` command and sent it to us, we don't want to ask
him all the time to re-enter this command to get his phone/name. We just ask him to send the command once, just like usual ask.

The workflow, in the context of `RegisterCommand`, is following:
* Set `IsActive` property to true, if it is false. This step is essential as we might have commands that require sequential
interaction with user(e.g. asking for phone number after asking his name and etc.).
* Ask for user's phone number
* If phone number was given, ask for his name
* Lastly, complete registration be resetting both `_phone` and `_name` fields.
> You could actually save these data to db, but in my case, it's just nothing more than an example.

## Overall workflow
1. User sends `/register` command
2. `Telegram` makes call to our web-site(check [appsettings](./currency-converter-bot/CurrencyBot.Example/appsettings.json))
3. The call is made to post action method in BotController
4. BotController calls `GetUpdate()` of `UpdateDistributor`(which is singleton as we don't need `UpdateDistributor`
to be newly created all the time the request is sent)
5. `UpdateDistributor` calls for `CommandExecutor`'s `GetUpdate()`
6. `CommandExecutor` looks for appropriate `Command` class  to handle the user's command.
7. If it successes, `CommandExecutor` calls for `ExecuteAsync()` of the appropriate command.



