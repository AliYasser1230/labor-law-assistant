using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

public class LawDocument
{
    public int Id { get; set; }

    public Country Country { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(255)]
    public required string FileName { get; set; }

    [MaxLength(100)]
    public required string StoredFileName { get; set; }

    public int PageCount { get; set; }

    public DateTime UploadedAt { get; set; }

    public List<LawArticle> Articles { get; set; } = [];
}
