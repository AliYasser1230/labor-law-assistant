using Microsoft.AspNetCore.Mvc;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// List of past questions and answers. Placeholder for now; built in step 8.
/// </summary>
public class HistoryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
