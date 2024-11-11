# currency-converter-bot [![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
A telegram bot that allows converting from one currency to another. 
# Bot Specification
The bot maintains conversation through the various commands:
* `/start` command: starts the bot and greets the user
* `/help` command: lists the available commands in the bot
* `/convert` command: the main command which requires user to send the request in format `"100 USD to EUR"` or
`"50 GBP in JPY"`
# Code Specification
The entire class descriptions(e.g the purpose of each class) is described in [workflow.md](CurrencyBot.Example/docs/workflow.md)
of [CurrencyBot.Example](CurrencyBot.Example). We use [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) to map the incoming request to 
`Update` class.
```csharp
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new UnixDateTimeConverter()); // to derialize the message from unix date time format e.g(1731176585)
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});
```
And the usage:
```csharp
[HttpPost]
public async Task Post(Update update) // the message is deserialized internally by asp.net core here
{
    if (update.Message == null)
    {
        return;
    }
    await _updateDistributor.GetUpdate(update);
}
```
Instead of long polling we specify the `Webhook` communication type with `Telegram` service
```csharp
await bot.SetWebhook(builder.Configuration["ngrok:Connection"]!);
```
Telegram automatically sends updates to your bot's server URL as soon as they are available.
This is more efficient because the server does not need to make repeated API calls. The material about Webhooks and 
Long Polling [can be found here](https://www.svix.com/resources/faq/webhooks-vs-api-polling/#:~:text=Polling%20requests%20are%20made%20by,a%20new%20event%20or%20not.)
`SetWebhook` method registers a specific URL (provided in the ngrok:Connection configuration) where Telegram should send updates for your bot.
> [!IMPORTANT]
> Don't forget to update the `appsettings.json` configuration file based on your url

I use [ngrok](https://ngrok.com) as a tunnel for `Telegram` updates(as `Telegram` can't send request to localhost). The only
limitation that `ngrok` generates a new URL each time you start it, which means you have update the url every time in configuration
file. 
### Quick start for launching ngrok
1) Download `ngrok` by going [here](https://download.ngrok.com/)
2) specify the url address `ngrok` will serve as a tunnel to. It can be found in launchSettings and may look like this:
```json
"profiles": {
   "http": {
     "commandName": "Project",
     "dotnetRunMessages": true,
     "launchBrowser": true,
     "applicationUrl": "http://localhost:5184",
     "environmentVariables": {
     "ASPNETCORE_ENVIRONMENT": "Development"
     }
   }, 
   "https": {
     "commandName": "Project",
     "dotnetRunMessages": true,
     "launchBrowser": true,
     "applicationUrl": "https://localhost:7016",
     "environmentVariables": {
     "ASPNETCORE_ENVIRONMENT": "Development"
     }
   }
}
```
3) Pass the url to the `ngrok`:
```bash
ngrok http http(s)://localhost:<your-port> --authtoken your_authtoken
```
As a result you should see something like:
![ngrok.png](resources%2FImages%2Fngrok.png)
Then you paste your url to `appsettings.json`.
# Links
* ngrok: https://ngrok.com
* `Telegram bot` through `Webhook`(for rus. community): https://habr.com/ru/sandbox/188818/
* `.NET` Client for `Telegram Bot` API: https://telegrambots.github.io/book/
* `Newtonsoft.Json` documentation: https://www.newtonsoft.com/json/help/html/Introduction.htm
* `Webhooks` vs `Long Polling`: https://www.svix.com/resources/faq/webhooks-vs-long-polling/

# License
The project is under [Apache License](LICENSE)