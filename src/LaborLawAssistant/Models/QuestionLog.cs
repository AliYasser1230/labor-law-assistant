using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

/// <summary>
/// One question a user asked, with Claude's answer and the cost. Shown on the History page.
/// Table: QuestionLogs
/// </summary>
public class QuestionLog
{
    public int Id { get; set; }

    public Country Country { get; set; }

    [MaxLength(1000)]
    public required string Question { get; set; }

    public required string Answer { get; set; }

    // Tokens used by the Claude request (from the API response), and the price in US dollars.
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public decimal CostUsd { get; set; }

    // Always saved in UTC.
    public DateTime AskedAt { get; set; }
}
