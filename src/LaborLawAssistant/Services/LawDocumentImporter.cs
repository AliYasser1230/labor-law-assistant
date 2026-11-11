using LaborLawAssistant.Data;
using LaborLawAssistant.Models;

namespace LaborLawAssistant.Services;

/// <summary>
/// The "recipe" for importing a law: save the file → read the pages → cut into articles → save in the database.
/// It only coordinates; each job is done by its own service (dependency injection).
/// Step 5 will add one more job here: embeddings.
/// </summary>
public class LawDocumentImporter(
    AppDbContext db,
    IDocumentStorage storage,
    IPdfTextExtractor pdfTextExtractor,
    IArticleSplitter articleSplitter,
    ILogger<LawDocumentImporter> logger) : ILawDocumentImporter
{
    public async Task<ImportResult> ImportAsync(Country country, string title, IFormFile file)
    {
        // 1. Save the PDF on disk.
        var storedFileName = await storage.SaveAsync(file);

        // 2. Read the text of every page.
        IReadOnlyList<PdfPageText> pages;
        try
        {
            pages = pdfTextExtractor.ExtractPages(storage.GetFullPath(storedFileName));
        }
        catch (Exception exception)
        {
            // PdfPig throws for damaged or password-protected files.
            logger.LogWarning(exception, "Could not read PDF {FileName}", file.FileName);
            storage.Delete(storedFileName);
            return ImportResult.Failed("This PDF could not be read. It may be damaged or password-protected.");
        }

        // 3. Cut the text into articles.
        var articles = articleSplitter.Split(pages);
        if (articles.Count == 0)
        {
            // For example a scanned PDF (only images, no text), or a file that is not a law.
            storage.Delete(storedFileName);
            return ImportResult.Failed("No articles were found in this PDF. Is it a text PDF (not a scanned image)?");
        }

        // 4. Save the document and all its articles. One SaveChanges = one transaction: all rows or none.
        db.LawDocuments.Add(new LawDocument
        {
            Country = country,
            Title = title,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            PageCount = pages.Count,
            UploadedAt = DateTime.UtcNow,
            Articles = articles
                .Select(article => new LawArticle
                {
                    ArticleNumber = article.ArticleNumber,
                    Text = article.Text,
                    PageNumber = article.PageNumber,
                })
                .ToList(),
        });
        await db.SaveChangesAsync();

        logger.LogInformation("Imported {Title}: {Pages} pages, {Articles} articles", title, pages.Count, articles.Count);
        return ImportResult.Succeeded(articles.Count);
    }
}
