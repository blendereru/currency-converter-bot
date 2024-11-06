using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace CurrencyBot.Controllers;
[Route("/")]
public class BotController : Controller
{
    //ToDo: initialize the post and get methods 
    [HttpPost]
    public string Post(Update update)
    {
        
    }
}