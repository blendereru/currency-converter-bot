# currency-converter-bot
[![CI/CD](https://github.com/blendereru/currency-converter-bot/actions/workflows/CD.yml/badge.svg)](https://github.com/blendereru/currency-converter-bot/actions/workflows/CD.yml)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A telegram bot that allows converting from one currency to another. The project is a `ASP.NET Core` application, that
provides webhook endpoint for the Telegram.Bot.
# Bot Specification
The bot maintains conversation through the various commands:
* `/start` command: starts the bot and greets the user
* `/help` command: lists the available commands in the bot
* `/convert` command: the main command which requires user to send the request in format `"100 USD to EUR"` or
`"50 GBP in JPY"`
* `/listcurrencies` command: lists the currencies supported by API
* `/searchcurrency` command: find the short name of the currency
Instead of long polling we specify the `Webhook` communication type with `Telegram` service
```csharp
await bot.SetWebhook(webhookUrl);
```
Telegram automatically sends updates to your bot's server URL as soon as they are available.
This is more efficient because the server does not need to make repeated API calls.
## Appsettings
In the `appsettings` we define configurations for the bot and the api to which we will make requests to access currency rates.
```json
"BotConfiguration": {
    "BotToken": "YOUR-BOT-TOKEN",
    "BotWebhookUrl": "YOUR-WEBHOOK-URL",
    "SecretToken": "SOME-SECRET-STRING"
  },
"ApiConfiguration": {
    "ApiKey": "YOUR-API-KEY"
  }
```
I use [ExchangeRate-API](https://www.exchangerate-api.com/) which gives more than 1000 requests for `free-plan` users. 
After you received your `API Key` you can make requests by following address(USD is used as an example currency)
```
https://v6.exchangerate-api.com/v6/<your-api-key>/latest/USD
```
and receive `json` as a response.
```json
{
 "result":"success",
 "documentation":"https://www.exchangerate-api.com/docs",
 "terms_of_use":"https://www.exchangerate-api.com/terms",
 "time_last_update_unix":1734998402,
 "time_last_update_utc":"Tue, 24 Dec 2024 00:00:02 +0000",
 "time_next_update_unix":1735084802,
 "time_next_update_utc":"Wed, 25 Dec 2024 00:00:02 +0000",
 "base_code":"USD",
 "conversion_rates":{
  "USD":1,
  "AED":3.6725,
  "AFN":70.0202,
  "ALL":94.5987,
  "AMD":395.2964,
  "ANG":1.7900,
  etc...
 }
}
```
I use [ngrok](https://ngrok.com) as a tunnel for `Telegram` updates(as `Telegram` can't send requests to localhost). The only
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
     "launchUrl": "bot/setWebhook",
     "applicationUrl": "https://localhost:8443",
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
The project was built by looking up to [Telegram.Bot.Examples](https://github.com/TelegramBots/Telegram.Bot.Examples)
# License
The project is under [Apache License v2](LICENSE)
