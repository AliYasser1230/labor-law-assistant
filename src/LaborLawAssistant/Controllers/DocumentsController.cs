using LaborLawAssistant.Data;
using LaborLawAssistant.Services;
using LaborLawAssistant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// Law documents: the list of uploaded PDFs, the upload form, and the articles of one law.
/// Dependency injection: ASP.NET Core gives us these services in the constructor.
/// </summary>
public class DocumentsController(
    AppDbContext db,
    ILawDocumentImporter importer,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    // GET /Documents: the table of uploaded laws.
    public async Task<IActionResult> Index()
    {
        var documents = await db.LawDocuments
            .OrderByDescending(document => document.Id)
            .Select(document => new DocumentListItemViewModel(
                document.Id,
                document.Title,
                document.Country,
                document.FileName,
                document.PageCount,
                document.Articles.Count,
                document.UploadedAt))
            .ToListAsync();

        return View(documents);
    }

    private const int ArticlesPerPage = 12;

    // GET /Documents/Details/5?page=2: one law and one page of its articles.
    // Model binding: "id" comes from the URL path, "page" from the query string (?page=2).
    public async Task<IActionResult> Details(int id, int page = 1)
    {
        var document = await db.LawDocuments
            .Where(document => document.Id == id)
            .Select(document => new { document.Title, document.Country, document.PageCount, ArticleCount = document.Articles.Count })
            .FirstOrDefaultAsync();

        if (document is null)
        {
            return NotFound();
        }

        // Keep the page number in range (e.g. ?page=99 shows the last page).
        var totalPages = Math.Max(1, (int)Math.Ceiling(document.ArticleCount / (double)ArticlesPerPage));
        page = Math.Clamp(page, 1, totalPages);

        // Load only the 12 articles of this page from the database (Skip + Take = SQL LIMIT/OFFSET).
        var articles = await db.LawArticles
            .Where(article => article.LawDocumentId == id)
            .OrderBy(article => article.Id) // the order they appear in the PDF
            .Skip((page - 1) * ArticlesPerPage)
            .Take(ArticlesPerPage)
            .Select(article => new { article.ArticleNumber, article.PageNumber, article.Text })
            .ToListAsync();

        var items = articles
            .Select(article => ArticleItemViewModel.Create(article.ArticleNumber, article.PageNumber, article.Text))
            .ToList();

        return View(new DocumentDetailsViewModel(
            document.Title,
            document.Country,
            document.PageCount,
            new PagedResult<ArticleItemViewModel>(items, page, ArticlesPerPage, document.ArticleCount)));
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

        var title = model.Title.Trim();
        var result = await importer.ImportAsync(model.Country, title, model.File!); // File is not null: [Required] passed

        if (!result.Success)
        {
            // The file looked like a PDF, but we could not read it or found no articles: show the reason under the file field.
            ModelState.AddModelError(nameof(model.File), localizer[result.Error!]);
            return View(model);
        }

        // Post/Redirect/Get: redirect after a successful POST, so refreshing the page doesn't upload twice.
        // TempData keeps a value for the next request only (here: the success message).
        TempData["UploadedTitle"] = title;
        TempData["ArticleCount"] = result.ArticleCount;
        return RedirectToAction(nameof(Index));
    }
}
