using CurrencyBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new UnixDateTimeConverter());
    options.SerializerSettings.Converters.Add(new StringEnumConverter()); // Add this line
    options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<UpdateDistributor>();
var app = builder.Build();
using var scope = app.Services.CreateScope();
var bot = Bot.GetTelegramBot();
await bot.SetWebhook(builder.Configuration["ngrok:Connection"]!);
app.UseHttpsRedirection();
app.MapControllers();
app.Run();