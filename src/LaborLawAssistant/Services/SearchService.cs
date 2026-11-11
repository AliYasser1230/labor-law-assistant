using LaborLawAssistant.Data;
using LaborLawAssistant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Services;

public class SearchService(
    AppDbContext db,
    IEmbeddingService embeddingService,
    IOptions<SearchOptions> options,
    ILogger<SearchService> logger) : ISearchService
{
    public async Task<SearchResult> SearchAsync(Country country, string question, CancellationToken cancellationToken = default)
    {
        var articles = await db.LawArticles
            .Where(article => article.LawDocument!.Country == country && article.Embedding != null)
            .Select(article => new
            {
                article.Id,
                LawTitle = article.LawDocument!.Title,
                article.ArticleNumber,
                article.PageNumber,
                article.Text,
                Embedding = article.Embedding!,
            })
            .ToListAsync(cancellationToken);

        if (articles.Count == 0)
        {
            return new SearchResult(Found: false, []);
        }

        var questionEmbedding = await embeddingService.EmbedQueryAsync(question, cancellationToken);

        var hits = articles
            .Where(article => article.Embedding.Length == questionEmbedding.Length)
            .Select(article => new SearchHit(
                article.Id,
                article.LawTitle,
                article.ArticleNumber,
                article.PageNumber,
                article.Text,
                CosineSimilarity(questionEmbedding, article.Embedding)))
            .OrderByDescending(hit => hit.Score)
            .Take(options.Value.TopK)
            .ToList();

        var bestScore = hits.Count > 0 ? hits[0].Score : 0;
        logger.LogInformation("Search ({Country}): {Count} articles compared, best score {Score:F3} (article {Article})",
            country, articles.Count, bestScore, hits.Count > 0 ? hits[0].ArticleNumber : "-");

        return bestScore >= options.Value.MinScore
            ? new SearchResult(Found: true, hits)
            : new SearchResult(Found: false, []);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, lengthA = 0, lengthB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
    }
}
