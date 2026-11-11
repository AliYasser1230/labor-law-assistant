using LaborLawAssistant.Data;
using LaborLawAssistant.Services;
using LaborLawAssistant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Controllers;

public class DocumentsController(
    AppDbContext db,
    ILawDocumentImporter importer,
    IStringLocalizer<SharedResource> localizer) : Controller
{
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
                document.Articles.Count(article => article.Embedding != null),
                document.UploadedAt))
            .ToListAsync();

        return View(documents);
    }

    private const int ArticlesPerPage = 12;

    public async Task<IActionResult> Details(int id, int page = 1)
    {
        var document = await db.LawDocuments
            .Where(document => document.Id == id)
            .Select(document => new
            {
                document.Title,
                document.Country,
                document.PageCount,
                ArticleCount = document.Articles.Count,
                EmbeddedCount = document.Articles.Count(article => article.Embedding != null),
            })
            .FirstOrDefaultAsync();

        if (document is null)
        {
            return NotFound();
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling(document.ArticleCount / (double)ArticlesPerPage));
        page = Math.Clamp(page, 1, totalPages);

        var articles = await db.LawArticles
            .Where(article => article.LawDocumentId == id)
            .OrderBy(article => article.Id)
            .Skip((page - 1) * ArticlesPerPage)
            .Take(ArticlesPerPage)
            .Select(article => new { article.ArticleNumber, article.PageNumber, article.Text })
            .ToListAsync();

        var items = articles
            .Select(article => ArticleItemViewModel.Create(article.ArticleNumber, article.PageNumber, article.Text))
            .ToList();

        return View(new DocumentDetailsViewModel(
            id,
            document.Title,
            document.Country,
            document.PageCount,
            document.EmbeddedCount,
            new PagedResult<ArticleItemViewModel>(items, page, ArticlesPerPage, document.ArticleCount)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Embed(int id)
    {
        if (!await db.LawDocuments.AnyAsync(document => document.Id == id))
        {
            return NotFound();
        }

        var result = await importer.EmbedMissingArticlesAsync(id);
        if (result.Failed)
        {
            TempData["EmbeddingsFailed"] = true;
        }
        else
        {
            TempData["EmbeddedCount"] = result.EmbeddedCount;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View(new UploadDocumentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var title = model.Title.Trim();
        var result = await importer.ImportAsync(model.Country, title, model.File!);

        if (!result.Success)
        {
            ModelState.AddModelError(nameof(model.File), localizer[result.Error!]);
            return View(model);
        }

        TempData["UploadedTitle"] = title;
        TempData["ArticleCount"] = result.ArticleCount;
        TempData["EmbeddingsFailed"] = result.EmbeddingsFailed;
        return RedirectToAction(nameof(Index));
    }
}
