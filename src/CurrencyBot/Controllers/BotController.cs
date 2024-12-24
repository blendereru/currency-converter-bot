using CurrencyBot.Configurations;
using CurrencyBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Controllers;
[ApiController]
[Route("/")]
public class BotController : Controller
{
    private readonly IOptions<BotConfiguration> _config;
    public BotController(IOptions<BotConfiguration> config)
    {
        _config = config;
    }
    [HttpGet("bot/setWebhook")]
    public async Task<string> SetWebHook([FromServices] ITelegramBotClient bot, CancellationToken ct)
    {
        var webhookUrl = _config.Value.BotWebhookUrl.AbsoluteUri;
        await bot.SetWebhook(webhookUrl, secretToken: _config.Value.SecretToken, cancellationToken: ct);
        return $"Webhook set to {webhookUrl}";
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] ITelegramBotClient bot, [FromServices] UpdateHandler handleUpdateService, CancellationToken ct)
    {
        if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != _config.Value.SecretToken)
        {
            return Forbid();
        }
        try
        {
            await handleUpdateService.HandleUpdateAsync(bot, update, ct);
        }
        catch (Exception exception)
        {
            await handleUpdateService.HandleErrorAsync(bot, exception, Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError, ct);
        }
        return Ok();
    }
}