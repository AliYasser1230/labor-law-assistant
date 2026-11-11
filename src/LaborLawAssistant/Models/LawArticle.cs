using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

/// <summary>
/// One article of a law (for example: Article 109 about annual leave).
/// This is what RAG searches and what Claude cites.
/// Table: LawArticles
/// </summary>
public class LawArticle
{
    public int Id { get; set; }

    // Foreign key: which LawDocument this article belongs to.
    public int LawDocumentId { get; set; }

    // Navigation property: lets us write article.LawDocument.Title in C#.
    public LawDocument? LawDocument { get; set; }

    // Text, not int, because some laws have numbers like "77 bis".
    [MaxLength(20)]
    public required string ArticleNumber { get; set; }

    // The full text of the article.
    public required string Text { get; set; }

    // The PDF page where the article starts (shown as the source).
    public int PageNumber { get; set; }

    // The embedding from Voyage AI: 1024 numbers that describe the meaning of the text.
    // Empty (null) until step 5 creates it. EF Core saves the array as JSON text.
    public float[]? Embedding { get; set; }
}
