using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

/// <summary>
/// One row in the documents table (Views/Documents/Index.cshtml).
/// Only the fields the page shows, so we don't load every article's text just to count them.
/// </summary>
public record DocumentListItemViewModel(
    int Id,
    string Title,
    Country Country,
    string FileName,
    int PageCount,
    int ArticleCount,
    DateTime UploadedAt);
