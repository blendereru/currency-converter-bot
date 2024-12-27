using Azure.Identity;
using CurrencyBot.Clients;
using CurrencyBot.Configurations;
using CurrencyBot.Services;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["AzureKeyVaultUri"]!),
    new DefaultAzureCredential()); // remove this conf if you are on Development mode
var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
var apiConfigSection = builder.Configuration.GetSection("ApiConfiguration");
builder.Services.Configure<ApiConfiguration>(apiConfigSection);
builder.Services.Configure<BotConfiguration>(botConfigSection);
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));
builder.Services.AddHttpClient<ExchangeRateClient>("currencyApi", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}); 
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.ConfigureTelegramBotMvc();
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();