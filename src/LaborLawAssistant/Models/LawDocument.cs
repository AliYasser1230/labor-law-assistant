using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

/// <summary>
/// One uploaded law PDF (for example: the Saudi Labor Law).
/// Table: LawDocuments
/// </summary>
public class LawDocument
{
    // Primary key. EF Core sees the name "Id" and makes it auto-increment.
    public int Id { get; set; }

    public Country Country { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    // The original name of the uploaded file, e.g. "saudi-labor-law.pdf".
    [MaxLength(255)]
    public required string FileName { get; set; }

    public int PageCount { get; set; }

    // Always saved in UTC (world time), converted to local time only for display.
    public DateTime UploadedAt { get; set; }

    // Navigation property: all articles cut from this PDF (one document → many articles).
    public List<LawArticle> Articles { get; set; } = [];
}
