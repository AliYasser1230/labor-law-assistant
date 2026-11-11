using LaborLawAssistant.Models;

namespace LaborLawAssistant.Services;

/// <summary>The result of an import: success + number of articles, or an error message (an English key for SharedResource).</summary>
public record ImportResult(bool Success, string? Error, int ArticleCount)
{
    public static ImportResult Succeeded(int articleCount) => new(true, null, articleCount);
    public static ImportResult Failed(string error) => new(false, error, 0);
}

/// <summary>
/// Part A of the system: turns an uploaded law PDF into saved articles.
/// </summary>
public interface ILawDocumentImporter
{
    Task<ImportResult> ImportAsync(Country country, string title, IFormFile file);
}
