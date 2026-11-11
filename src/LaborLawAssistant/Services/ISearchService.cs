using LaborLawAssistant.Models;

namespace LaborLawAssistant.Services;

public record SearchHit(int ArticleId, string LawTitle, string ArticleNumber, int PageNumber, string Text, double Score);

public record SearchResult(bool Found, IReadOnlyList<SearchHit> Hits);

public interface ISearchService
{
    Task<SearchResult> SearchAsync(Country country, string question, CancellationToken cancellationToken = default);
}
