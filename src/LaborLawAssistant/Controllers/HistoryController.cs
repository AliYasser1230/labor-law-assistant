using Microsoft.AspNetCore.Mvc;

namespace LaborLawAssistant.Controllers;

public class HistoryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
