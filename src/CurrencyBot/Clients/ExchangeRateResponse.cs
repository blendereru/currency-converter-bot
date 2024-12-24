using System.Text.Json.Serialization;

namespace CurrencyBot.Clients;

public class ExchangeRateResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal>? ConversionRates { get; set; }
}