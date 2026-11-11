using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaborLawAssistant.Data;
using LaborLawAssistant.Models;

namespace LaborLawAssistant.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    // GET /: the welcome page. It lists the countries that have an uploaded law ("Available now: ...").
    public async Task<IActionResult> Index()
    {
        var countries = await db.LawDocuments
            .Select(document => document.Country)
            .Distinct()
            .ToListAsync();

        return View(countries);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
