using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

public record HistoryItemViewModel(
    Country Country,
    string Question,
    string Answer,
    int InputTokens,
    int OutputTokens,
    decimal CostUsd,
    DateTime AskedAt)
{
    public bool WasAnswered => InputTokens > 0;

    public string Preview => Answer.Length <= 220 ? Answer : Answer[..220].TrimEnd() + "…";
}

public record HistoryPageViewModel(
    int TotalQuestions,
    int AnsweredQuestions,
    decimal TotalCostUsd,
    PagedResult<HistoryItemViewModel> Items);
