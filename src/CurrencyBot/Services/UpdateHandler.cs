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
    public UpdateHandler(ITelegramBotClient bot, ExchangeRateClient client)
    {
        _bot = bot;
        _exchangeRateClient = client;
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
            _ => OnConvertCurrencyCommand(msg)
        });
        Log.Information("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        Log.Information("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await _bot.AnswerCallbackQuery(callbackQuery.Id, $"Received {callbackQuery.Data}");
        var sentMessage = await (callbackQuery.Data switch
        {
            "/convert" => OnConvertCurrencyCommand(callbackQuery)
        });
        Log.Information("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }
    private async Task<Message> OnStartCommand(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddButton("Convert currency", "/convert");
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
                                    "Just type the command to begin!";
        return await _bot.SendMessage(msg.Chat, message);
    }
    private async Task<Message> OnConvertCurrencyCommand(CallbackQuery callbackQuery)
    {
        Log.Information("Received callback data {CallbackQueryData}", callbackQuery.Data);
        return await _bot.SendMessage(callbackQuery.Message!.Chat,
            "Please enter the amount and currencies (e.g., '100 USD to EUR').");
    }

    private async Task<Message> OnConvertCurrencyCommand(Message msg)
    {
        if (TryParseText(msg.Text!, out var amount, out var sourceCurrency, out var targetCurrency))
        {
            if (sourceCurrency == null || targetCurrency == null)
            {
                Log.Information("Unable to parse currency input.");
                return await _bot.SendMessage(msg.Chat, "Sorry, it seems like you entered an invalid currency format. Please use the format '100 USD to EUR'.");
            }
            try
            {
                var response = await _exchangeRateClient.GetExchangeRate(sourceCurrency);
        
                if (response == null || response.ConversionRates == null)
                {
                    Log.Warning($"Failed to fetch exchange rate for {sourceCurrency}");
                    return await _bot.SendMessage(msg.Chat, $"Sorry, I couldn't fetch the exchange rate for {sourceCurrency}. Please try again later.");
                }
                if (!response.ConversionRates.TryGetValue(targetCurrency, out var rate))
                {
                    Log.Warning($"Target currency {targetCurrency} not found in exchange rates.");
                    return await _bot.SendMessage(msg.Chat, $"Sorry, I couldn't find the target currency {targetCurrency}.");
                }
                var convertedAmount = amount * rate;
                return await _bot.SendMessage(
                    msg.Chat,
                    $"{amount} {sourceCurrency} equals {convertedAmount:F2} {targetCurrency} (Rate: {rate:F4})."
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while fetching exchange rate.");
                return await _bot.SendMessage(msg.Chat, "An error occurred while processing your request. Please try again later.");
            }
        }
        return await _bot.SendMessage(msg.Chat,"Please use the format '100 USD to EUR'.");
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
}