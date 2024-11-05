using CurrencyBot.Example.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyBot.Example.Controllers;

[ApiController] 
[Route("/")]
public class BotController : Controller
{
    private readonly TelegramBotClient _bot = Bot.GetTelegramBot();
    private readonly UpdateDistributor _updateDistributor;
    private readonly ILogger<BotController> _logger;
    public BotController(ILogger<BotController> logger, UpdateDistributor updateDistributor)
    {
        _updateDistributor = updateDistributor;
        _logger = logger;
    }
    [HttpPost]
    public async Task Post(Update update) //updates will come here
    {
        if (update.Message == null)
        {
            return;
        }
        await _updateDistributor.GetUpdate(update);
    }
    [HttpGet]
    public string Get() 
    {
        return "Telegram bot was started";
    }
}
