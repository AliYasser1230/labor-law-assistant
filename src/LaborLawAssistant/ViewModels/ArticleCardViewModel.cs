namespace LaborLawAssistant.ViewModels;

public record ArticleCardViewModel(
    ArticleItemViewModel Article,
    string? LawTitle = null,
    int? SourceNumber = null,
    bool IsCited = false);
