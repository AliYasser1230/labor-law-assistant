using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

public class LawArticle
{
    public int Id { get; set; }

    public int LawDocumentId { get; set; }

    public LawDocument? LawDocument { get; set; }

    [MaxLength(20)]
    public required string ArticleNumber { get; set; }

    public required string Text { get; set; }

    public int PageNumber { get; set; }

    public float[]? Embedding { get; set; }
}
