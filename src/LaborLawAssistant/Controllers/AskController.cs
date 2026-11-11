using Microsoft.AspNetCore.Mvc;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// Ask a labor law question. Placeholder for now; built in step 7.
/// </summary>
public class AskController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
