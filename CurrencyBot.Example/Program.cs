using CurrencyBot.Example.Models;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSingleton<UpdateDistributor>();
var app = builder.Build();
using var scope = app.Services.CreateScope();
var bot = Bot.GetTelegramBot();
await bot.SetWebhookAsync(builder.Configuration["ngrok:Connection"]!);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();