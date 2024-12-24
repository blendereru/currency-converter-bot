using System.Text.Json;
using CurrencyBot.Configurations;
using Microsoft.Extensions.Options;
namespace CurrencyBot.Clients;

public class ExchangeRateClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ApiConfiguration> _conf;
    public ExchangeRateClient(HttpClient httpClient, IOptions<ApiConfiguration> conf)
    {
        _httpClient = httpClient;
        _conf = conf;
    }
    public async Task<ExchangeRateResponse?> GetExchangeRate(string baseCurrency)
    {
        var url = $"https://v6.exchangerate-api.com/v6/{_conf.Value.ApiKey}/latest/{baseCurrency}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ExchangeRateResponse>(jsonResponse);
    }
}