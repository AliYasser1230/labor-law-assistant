using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

public record HistoryItemViewModel(
    Country Country,
    string Question,
    string Answer,
    bool WasAnswered,
    DateTime AskedAt)
{
    public string Preview => Answer.Length <= 220 ? Answer : Answer[..220].TrimEnd() + "…";
}

public record HistoryPageViewModel(
    int TotalQuestions,
    int AnsweredQuestions,
    PagedResult<HistoryItemViewModel> Items)
{
    public int NotFoundQuestions => TotalQuestions - AnsweredQuestions;
}
