using LaborLawAssistant.Data;
using LaborLawAssistant.Models;
using LaborLawAssistant.Services;
using LaborLawAssistant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// Law documents: the list of uploaded PDFs and the upload form.
/// Dependency injection: ASP.NET Core gives us the database and the file storage in the constructor.
/// </summary>
public class DocumentsController(AppDbContext db, IDocumentStorage storage) : Controller
{
    // GET /Documents: the table of uploaded laws.
    public async Task<IActionResult> Index()
    {
        var documents = await db.LawDocuments
            .OrderByDescending(document => document.Id)
            .Select(document => new DocumentListItemViewModel(
                document.Title,
                document.Country,
                document.FileName,
                document.PageCount,
                document.Articles.Count,
                document.UploadedAt))
            .ToListAsync();

        return View(documents);
    }

    // GET /Documents/Upload: the empty form.
    [HttpGet]
    public IActionResult Upload()
    {
        return View(new UploadDocumentViewModel());
    }

    // POST /Documents/Upload: the form was sent.
    // Model binding: ASP.NET Core creates "model" from the form fields (Country, Title, File).
    // Model validation: the attributes in UploadDocumentViewModel run, and the result is in ModelState.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Show the same form again, with the user's values and the error messages.
            return View(model);
        }

        var file = model.File!; // not null here: [Required] passed
        var storedFileName = await storage.SaveAsync(file);

        db.LawDocuments.Add(new LawDocument
        {
            Country = model.Country,
            Title = model.Title.Trim(),
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            PageCount = 0, // filled in step 4, when we read the PDF
            UploadedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        // Post/Redirect/Get: redirect after a successful POST, so refreshing the page doesn't upload twice.
        // TempData keeps a value for the next request only (here: the success message).
        TempData["UploadedTitle"] = model.Title.Trim();
        return RedirectToAction(nameof(Index));
    }
}
