using System.Text.RegularExpressions;
using CurrencyBot.Clients;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ExchangeRateClient _exchangeRateClient;
    private readonly Dictionary<long, string> _previousCommandName;
    public UpdateHandler(ITelegramBotClient bot, ExchangeRateClient client)
    {
        _bot = bot;
        _exchangeRateClient = client;
        _previousCommandName = new Dictionary<long, string>();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: {} message } => OnMessage(message),
            { CallbackQuery: {} callbackQuery} => OnCallbackQuery(callbackQuery)
        });
    }
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Log.Information("HandleError: {Exception}", exception);
        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    private async Task OnMessage(Message msg)
    {
        Log.Information("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
        {
            return;
        }
        var sentMessage = await (messageText switch
        {
            "/start" => OnStartCommand(msg),
            "/convert" => OnConvertCurrencyCommand(msg),
            "/help" => OnHelpCommand(msg),
            "/listcurrencies" => OnListCurrenciesCommand(msg),
            "/searchcurrency" => OnSearchCurrencyCommand(msg),
            _ => OnUnknownCommand(msg)
        });
        Log.Information("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        Log.Information("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await _bot.AnswerCallbackQuery(callbackQuery.Id);
        Log.Information("Received callback data {CallbackQueryData}", callbackQuery.Data);
        var sentMessage = await (callbackQuery.Data switch
        {
            "/convert" => OnConvertCurrencyCommand(callbackQuery),
            "/listcurrencies" => OnListCurrenciesCommand(callbackQuery),
            "/searchcurrency" => OnSearchCurrencyCommand(callbackQuery)
        });
        Log.Information("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }
    private async Task<Message> OnStartCommand(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddButton("Convert currency", "/convert")
            .AddNewRow()
            .AddButton("List of available currencies", "/listcurrencies");
        return await _bot.SendMessage(msg.Chat,
            "Welcome to the Currency Converter Bot! Tap the button below to start a conversion.",
            replyMarkup: inlineMarkup);
    }

    private async Task<Message> OnHelpCommand(Message msg)
    {
        var message = "Here is a list of commands you can access:\n\n" +
                                    "/start - Start the bot and get a welcome message\n" +
                                    "/help - Display this list of available commands\n" +
                                    "/convert - Convert an amount from one currency to another (e.g., '100 USD')\n" +
                                    "/listcurrencies - List the currencies that we support" +
                                    "/searchcurrency - Find the short name of the specific currency or vice versa" +
                                    "Just type the command to begin!";
        return await _bot.SendMessage(msg.Chat, message);
    }
    private async Task<Message> OnConvertCurrencyCommand(CallbackQuery callbackQuery)
    {
        _previousCommandName[callbackQuery.Message!.Chat.Id] = callbackQuery.Data!;
        return await _bot.SendMessage(callbackQuery.Message!.Chat,
            "Please enter the amount and currencies (e.g., '100 USD to EUR').");
    }

    private async Task<Message> OnListCurrenciesCommand(CallbackQuery callbackQuery)
    {
        var currencies = string.Join("\n", Currencies.All.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        var inlineMarkup = new InlineKeyboardMarkup().AddButton("Find specific currency", "/searchcurrency");
        return await _bot.SendMessage(callbackQuery.Message!.Chat, currencies, replyMarkup: inlineMarkup);
    }
    private async Task<Message> OnListCurrenciesCommand(Message msg)
    {
        var currencies = string.Join("\n", Currencies.All.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        var inlineMarkup = new InlineKeyboardMarkup().AddButton("Find specific currency", "/searchcurrency");
        return await _bot.SendMessage(msg.Chat, currencies, replyMarkup: inlineMarkup);
    }
    private async Task<Message> OnSearchCurrencyCommand(CallbackQuery callbackQuery)
    {
        _previousCommandName[callbackQuery.Message!.Chat.Id] = callbackQuery.Data!;
        return await _bot.SendMessage(callbackQuery.Message!.Chat, "Send me the currency that you are looking for.");
    }
    private async Task<Message> OnSearchCurrencyCommand(Message msg)
    {
        _previousCommandName[msg.Chat.Id] = msg.Text!;
        return await _bot.SendMessage(msg.Chat, "Send me the currency that you are looking for.");
    }
    private async Task<Message> OnConvertCurrencyCommand(Message msg)
    {
        _previousCommandName[msg.Chat.Id] = msg.Text!;
        return await _bot.SendMessage(msg.Chat,
            "Please enter the amount and currencies (e.g., '100 USD to EUR').");
    }

    private async Task<Message> OnSearchCurrencyCommand(Chat chat, string key, string value)
    {
        return await _bot.SendMessage(chat, $"Found currency: {key} - {value}");
    }
    private async Task<Message> OnConvertCurrencyCommand(Chat chat, decimal amount, string sourceCurrency, string targetCurrency)
    {
        try
        {
            var response = await _exchangeRateClient.GetExchangeRate(sourceCurrency);
            if (response == null || response.ConversionRates == null)
            {
                Log.Warning($"Failed to fetch exchange rate for {sourceCurrency}");
                return await _bot.SendMessage(chat, $"Sorry, I couldn't fetch the exchange rate for {sourceCurrency}. Please try again later.");
            }
            if (!response.ConversionRates.TryGetValue(targetCurrency, out var rate))
            {
                Log.Warning($"Target currency {targetCurrency} not found in exchange rates.");
                return await _bot.SendMessage(chat, $"Sorry, I couldn't find the target currency {targetCurrency}.");
            }
            var convertedAmount = amount * rate;
            return await _bot.SendMessage(
                chat,
                $"{amount} {sourceCurrency} equals {convertedAmount:F2} {targetCurrency} (Rate: {rate:F4}).");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while fetching exchange rate.");
            return await _bot.SendMessage(chat, "An error occurred while processing your request. Please try again later.");
        }
    }
    private async Task<Message> OnUnknownCommand(Message msg)
    {
        if (_previousCommandName.TryGetValue(msg.Chat.Id, out var previousCommand))
        {
            if (previousCommand == "/convert")
            {
                if (TryParseText(msg.Text!, out var amount, out var sourceCurrency, out var targetCurrency))
                {
                    if (sourceCurrency == null || targetCurrency == null)
                    {
                        Log.Information("Unable to parse currency input.");
                        return await _bot.SendMessage(msg.Chat, "Sorry, it seems like you entered an invalid currency format. Please use the format '100 USD to EUR'.");
                    }
                    return await OnConvertCurrencyCommand(msg.Chat, amount, sourceCurrency, targetCurrency);
                }
                return await _bot.SendMessage(msg.Chat, "Please use the format '100 USD to EUR'.");
            }
            if (previousCommand == "/searchcurrency")
            {
                if (TryFindCurrencyValue(msg.Text!, out var key, out var value))
                {
                    if (key == null || value == null)
                    {
                        Log.Information("Unable to parse currency input.");
                        return await _bot.SendMessage(msg.Chat, "Sorry, it seems like you entered an invalid currency.");
                    }
                    return await OnSearchCurrencyCommand(msg.Chat, key, value);
                }
                Log.Information("Couldn't find the currency input: {Currency}", msg.Text);
                return await _bot.SendMessage(msg.Chat, "The currency is not found");
            }
        }
        Log.Information("Unknown command was received: {Command}", msg.Text);
        return await _bot.SendMessage(msg.Chat,
            "Sorry, I didn't understand your command. You can:\n" +
            "- Use `/help to list the commands supported by this bot");
    }
    private bool TryParseText(string message, out decimal amount, out string? sourceCurrency, out string? targetCurrency)
    {
        var regex = new Regex(@"(\d+(\.\d{1,2})?)\s+([A-Z]{3})\s+(to|in)\s+([A-Z]{3})", RegexOptions.IgnoreCase);
        var match = regex.Match(message);
        if (match.Success)
        {
            amount = decimal.Parse(match.Groups[1].Value);
            sourceCurrency = match.Groups[3].Value.ToUpper();
            targetCurrency = match.Groups[5].Value.ToUpper();
            return true;
        }
        amount = 0;
        sourceCurrency = null;
        targetCurrency = null;
        return false;
    }

    private bool TryFindCurrencyValue(string target, out string? key, out string? value)
    {
        foreach (var kvp in Currencies.All)
        {
            if (kvp.Key.Equals(target, StringComparison.OrdinalIgnoreCase) || 
                kvp.Value.Contains(target, StringComparison.OrdinalIgnoreCase))
            {
                key = kvp.Key;
                value = kvp.Value;
                return true;
            }
        }
        key = null;
        value = null;
        return false;
    }
}