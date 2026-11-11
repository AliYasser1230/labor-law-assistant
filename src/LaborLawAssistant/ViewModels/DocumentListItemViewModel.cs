using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

public record DocumentListItemViewModel(
    int Id,
    string Title,
    Country Country,
    string FileName,
    int PageCount,
    int ArticleCount,
    int EmbeddedCount,
    DateTime UploadedAt);
