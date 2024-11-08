using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Models;

public class ConvertCurrencyCommand : ICommand
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public ConvertCurrencyCommand(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    public TelegramBotClient Client => Bot.GetTelegramBot();
    public bool IsActive { get; private set; }
    public string Name => "/convert";

    public async Task ExecuteAsync(Update update)
    {
        if (update.Message == null || string.IsNullOrEmpty(update.Message.Text))
        {
            return;
        }

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        if (!IsActive)
        {
            IsActive = true;
            await Client.SendMessage(chatId, "Please enter the amount and currencies (e.g., '100 USD to EUR').");
        }
        else
        {
            if (TryParseText(messageText, out var amount, out var sourceCurrency, out var targetCurrency))
            {
                var url = $"https://v6.exchangerate-api.com/v6/{_configuration["ExchangeRate:ApiKey"]}/latest/{sourceCurrency}";

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Headers =
                    {
                        { "Accept", "application/json" }
                    }
                };
                requestMessage.Content = new StringContent(string.Empty);
                requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    await Client.SendMessage(chatId, $"API Response: {jsonResponse}");
                }
                else
                {
                    await Client.SendMessage(chatId, "Failed to fetch the exchange rate. Please try again.");
                }

                IsActive = false; 
            }
            else
            {
                await Client.SendMessage(chatId, "Invalid input. Please enter the amount and currencies in the format '100 USD to EUR'.");
            }
        }
    }


    private bool TryParseText(string message, out decimal amount, out string? sourceCurrency, out string? targetCurrency)
    {
        // Regex to match input like "100 USD to EUR" or "50 GBP in JPY"
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
