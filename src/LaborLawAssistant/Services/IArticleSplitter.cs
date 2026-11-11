namespace LaborLawAssistant.Services;

public record ParsedArticle(string ArticleNumber, string Text, int PageNumber);

public interface IArticleSplitter
{
    IReadOnlyList<ParsedArticle> Split(IReadOnlyList<PdfPageText> pages);
}
