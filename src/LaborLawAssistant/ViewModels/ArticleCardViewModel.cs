namespace LaborLawAssistant.ViewModels;

public record ArticleCardViewModel(
    ArticleItemViewModel Article,
    string? LawTitle = null,
    double? Score = null,
    int? SourceNumber = null,
    bool IsCited = false);
