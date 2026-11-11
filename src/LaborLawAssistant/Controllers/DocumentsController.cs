using Microsoft.AspNetCore.Mvc;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// Upload and list law PDFs. Placeholder for now; built in step 3.
/// </summary>
public class DocumentsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
