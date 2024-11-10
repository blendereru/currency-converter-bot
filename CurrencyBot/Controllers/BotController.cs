using CurrencyBot.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CurrencyBot.Controllers;
[ApiController]
[Route("/")]
public class BotController : Controller
{
    private readonly UpdateDistributor _updateDistributor;
    public BotController(UpdateDistributor updateDistributor)
    {
        _updateDistributor = updateDistributor;
    }
    [HttpPost]
    public async Task Post(Update update)
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