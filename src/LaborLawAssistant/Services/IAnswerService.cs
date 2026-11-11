using LaborLawAssistant.Models;

namespace LaborLawAssistant.Services;

public record AnswerPart(string Text, IReadOnlyList<int> SourceNumbers);

public record LawAnswer(
    IReadOnlyList<AnswerPart> Parts,
    bool Refused,
    string Model,
    int InputTokens,
    int OutputTokens,
    decimal CostUsd)
{
    public string ToPlainText() =>
        string.Concat(Parts.Select(part =>
            part.Text + string.Concat(part.SourceNumbers.Select(number => $" [{number}]"))));
}

public interface IAnswerService
{
    Task<LawAnswer> AnswerAsync(Country country, string question, IReadOnlyList<SearchHit> articles, CancellationToken cancellationToken = default);
}

public class AnswerException(string message, Exception? innerException = null) : Exception(message, innerException);
