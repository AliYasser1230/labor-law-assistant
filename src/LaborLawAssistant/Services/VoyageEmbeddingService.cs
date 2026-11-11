using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Services;

public class VoyageEmbeddingService(
    HttpClient httpClient,
    IOptions<VoyageAIOptions> options,
    ILogger<VoyageEmbeddingService> logger) : IEmbeddingService
{
    private const int MaxTextsPerRequest = 128;
    private const int MaxCharactersPerRequest = 30_000;

    private const int MaxAttempts = 6;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        var embeddings = new List<float[]>(texts.Count);
        foreach (var batch in MakeBatches(texts))
        {
            embeddings.AddRange(await EmbedBatchAsync(batch, "document", cancellationToken));
        }

        return embeddings;
    }

    public async Task<float[]> EmbedQueryAsync(string question, CancellationToken cancellationToken = default)
    {
        var embeddings = await EmbedBatchAsync([question], "query", cancellationToken);
        return embeddings[0];
    }

    private async Task<List<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, string inputType, CancellationToken cancellationToken)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new EmbeddingException("The Voyage AI API key is missing. Set it with: dotnet user-secrets set \"VoyageAI:ApiKey\" \"your-key\"");
        }

        for (var attempt = 1; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "embeddings")
            {
                Content = JsonContent.Create(new EmbeddingRequest(texts, options.Value.Model, inputType), options: JsonOptions),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException
                                              || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
            {
                throw new EmbeddingException("Could not reach Voyage AI (network error or timeout).", exception);
            }

            using (response)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxAttempts)
                {
                    var wait = TimeSpan.FromSeconds(Math.Min(20 * attempt, 60));
                    logger.LogWarning("Voyage AI rate limit (429). Waiting {Seconds} seconds, then trying again", wait.TotalSeconds);
                    await Task.Delay(wait, cancellationToken);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new EmbeddingException($"Voyage AI returned {(int)response.StatusCode} {response.StatusCode}: {body}");
                }

                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, cancellationToken)
                             ?? throw new EmbeddingException("Voyage AI returned an empty response.");

                logger.LogInformation("Voyage AI: {Count} texts embedded, {Tokens} tokens used", texts.Count, result.Usage.TotalTokens);

                return result.Data.OrderBy(item => item.Index).Select(item => item.Embedding).ToList();
            }
        }
    }

    private static IEnumerable<List<string>> MakeBatches(IReadOnlyList<string> texts)
    {
        var batch = new List<string>();
        var characters = 0;
        foreach (var text in texts)
        {
            if (batch.Count > 0 && (batch.Count == MaxTextsPerRequest || characters + text.Length > MaxCharactersPerRequest))
            {
                yield return batch;
                batch = [];
                characters = 0;
            }

            batch.Add(text);
            characters += text.Length;
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    private record EmbeddingRequest(IReadOnlyList<string> Input, string Model, string InputType);

    private record EmbeddingResponse(List<EmbeddingData> Data, EmbeddingUsage Usage);

    private record EmbeddingData(float[] Embedding, int Index);

    private record EmbeddingUsage(int TotalTokens);
}
