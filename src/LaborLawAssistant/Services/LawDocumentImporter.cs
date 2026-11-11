using LaborLawAssistant.Data;
using LaborLawAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Services;

public class LawDocumentImporter(
    AppDbContext db,
    IDocumentStorage storage,
    IPdfTextExtractor pdfTextExtractor,
    IArticleSplitter articleSplitter,
    IEmbeddingService embeddingService,
    ILogger<LawDocumentImporter> logger) : ILawDocumentImporter
{
    public async Task<ImportResult> ImportAsync(Country country, string title, IFormFile file)
    {
        var storedFileName = await storage.SaveAsync(file);

        IReadOnlyList<PdfPageText> pages;
        try
        {
            pages = pdfTextExtractor.ExtractPages(storage.GetFullPath(storedFileName));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not read PDF {FileName}", file.FileName);
            storage.Delete(storedFileName);
            return ImportResult.Failed("This PDF could not be read. It may be damaged or password-protected.");
        }

        var articles = articleSplitter.Split(pages);
        if (articles.Count == 0)
        {
            storage.Delete(storedFileName);
            return ImportResult.Failed("No articles were found in this PDF. Is it a text PDF (not a scanned image)?");
        }

        var document = new LawDocument
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
        };
        db.LawDocuments.Add(document);
        await db.SaveChangesAsync();
        logger.LogInformation("Imported {Title}: {Pages} pages, {Articles} articles", title, pages.Count, articles.Count);

        var embedding = await EmbedMissingArticlesAsync(document.Id);
        return ImportResult.Succeeded(articles.Count, embedding.Failed);
    }

    public async Task<EmbeddingRunResult> EmbedMissingArticlesAsync(int documentId)
    {
        var documentTitle = await db.LawDocuments
            .Where(document => document.Id == documentId)
            .Select(document => document.Title)
            .FirstAsync();

        var articles = await db.LawArticles
            .Where(article => article.LawDocumentId == documentId && article.Embedding == null)
            .OrderBy(article => article.Id)
            .ToListAsync();

        if (articles.Count == 0)
        {
            return new EmbeddingRunResult(0, Failed: false);
        }

        var embeddedCount = 0;
        try
        {
            foreach (var group in articles.Chunk(ArticlesPerGroup))
            {
                var texts = group
                    .Select(article => $"{documentTitle}\nArticle {article.ArticleNumber}\n{article.Text}")
                    .ToList();

                var embeddings = await embeddingService.EmbedDocumentsAsync(texts);
                for (var i = 0; i < group.Length; i++)
                {
                    group[i].Embedding = embeddings[i];
                }

                await db.SaveChangesAsync();
                embeddedCount += group.Length;
            }

            return new EmbeddingRunResult(embeddedCount, Failed: false);
        }
        catch (EmbeddingException exception)
        {
            logger.LogWarning(exception, "Could not create embeddings for document {DocumentId} ({Done} done)", documentId, embeddedCount);
            return new EmbeddingRunResult(embeddedCount, Failed: true);
        }
    }

    private const int ArticlesPerGroup = 20;
}
