using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using LaborLawAssistant.Models;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Services;

public class ClaudeAnswerService(
    AnthropicClient client,
    IOptions<AnthropicOptions> options,
    ILogger<ClaudeAnswerService> logger) : IAnswerService
{
    public async Task<LawAnswer> AnswerAsync(Country country, string question, IReadOnlyList<SearchHit> articles, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new AnswerException("The Anthropic API key is missing. Set it with: dotnet user-secrets set \"Anthropic:ApiKey\" \"your-key\"");
        }

        var content = new List<BetaContentBlockParam>();
        foreach (var article in articles)
        {
            content.Add(new BetaRequestDocumentBlock
            {
                Source = new BetaPlainTextSource { Data = article.Text },
                Title = $"{article.LawTitle} - Article {article.ArticleNumber} (page {article.PageNumber})",
                Citations = new BetaCitationsConfigParam { Enabled = true },
            });
        }

        content.Add(new BetaTextBlockParam { Text = question });

        var parameters = new MessageCreateParams
        {
            Model = settings.Model,
            MaxTokens = 16000,
            System = BuildSystemPrompt(country),
            OutputConfig = new BetaOutputConfig { Effort = Effort.Medium },
            Betas = [AnthropicBeta.ServerSideFallback2026_07_01],
            Fallbacks = new Default(),
            Messages = [new BetaUserMessageParam { Content = content }],
        };

        BetaMessage response;
        try
        {
            response = await client.Beta.Messages.Create(parameters, cancellationToken);
        }
        catch (AnthropicUnauthorizedException exception)
        {
            throw new AnswerException("The Anthropic API key is not valid.", exception);
        }
        catch (AnthropicRateLimitException exception)
        {
            throw new AnswerException("Too many requests to Claude right now. Please wait a minute.", exception);
        }
        catch (AnthropicApiException exception)
        {
            throw new AnswerException("Claude returned an error.", exception);
        }
        catch (AnthropicIOException exception)
        {
            throw new AnswerException("Could not reach Claude (network error).", exception);
        }

        var inputTokens = (int)response.Usage.InputTokens;
        var outputTokens = (int)response.Usage.OutputTokens;
        var cost = inputTokens * settings.InputPricePerMillionTokens / 1_000_000m
                   + outputTokens * settings.OutputPricePerMillionTokens / 1_000_000m;

        logger.LogInformation("Claude ({Model}): stop {StopReason}, {InputTokens} input + {OutputTokens} output tokens, ${Cost:F4}",
            response.Model, response.StopReason, inputTokens, outputTokens, cost);

        if (response.StopReason == "refusal")
        {
            return new LawAnswer([], Refused: true, response.Model, inputTokens, outputTokens, cost);
        }

        var parts = new List<AnswerPart>();
        foreach (var block in response.Content)
        {
            if (!block.TryPickText(out var text))
            {
                continue;
            }

            var sourceNumbers = new SortedSet<int>();
            foreach (var citation in text.Citations ?? [])
            {
                if (citation.TryPickCitationCharLocation(out var location))
                {
                    sourceNumbers.Add((int)location.DocumentIndex + 1);
                }
            }

            parts.Add(new AnswerPart(text.Text, sourceNumbers.ToList()));
        }

        return new LawAnswer(parts, Refused: false, response.Model, inputTokens, outputTokens, cost);
    }

    private static string BuildSystemPrompt(Country country) =>
        $"""
        You answer questions about the labor law of {country.GetDisplayName()}. The user's message contains articles of the law, given as documents, followed by the user's question.

        Rules:
        - Answer only from these articles. Do not use other knowledge. If the articles do not answer the question, say clearly that the law articles you have do not answer it, and do not guess.
        - Base each statement on the articles; the citations show the reader where each part comes from.
        - Reply in the language of the question: Arabic for an Arabic question, English for an English question.
        - Start with a short, direct answer (one or two sentences), then the important details and conditions.
        - Write plain text without Markdown (no #, no **, no tables). Use short paragraphs or simple numbered lines.
        - This is general information, not legal advice. If the question is about a personal dispute, add one sentence suggesting the official labor authority or a lawyer.
        """;
}
