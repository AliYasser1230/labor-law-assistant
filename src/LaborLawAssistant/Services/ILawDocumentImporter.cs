using LaborLawAssistant.Models;

namespace LaborLawAssistant.Services;

public record ImportResult(bool Success, string? Error, int ArticleCount, bool EmbeddingsFailed)
{
    public static ImportResult Succeeded(int articleCount, bool embeddingsFailed) => new(true, null, articleCount, embeddingsFailed);
    public static ImportResult Failed(string error) => new(false, error, 0, false);
}

public record EmbeddingRunResult(int EmbeddedCount, bool Failed);

public interface ILawDocumentImporter
{
    Task<ImportResult> ImportAsync(Country country, string title, IFormFile file);

    Task<EmbeddingRunResult> EmbedMissingArticlesAsync(int documentId);
}
