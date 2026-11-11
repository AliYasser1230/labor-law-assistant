using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

public class QuestionLog
{
    public int Id { get; set; }

    public Country Country { get; set; }

    [MaxLength(1000)]
    public required string Question { get; set; }

    public required string Answer { get; set; }

    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public decimal CostUsd { get; set; }

    public DateTime AskedAt { get; set; }
}
