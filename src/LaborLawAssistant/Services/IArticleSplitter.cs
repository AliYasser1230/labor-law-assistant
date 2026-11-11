namespace LaborLawAssistant.Services;

/// <summary>One article found in the PDF text: its number ("109"), its text, and the page where it starts.</summary>
public record ParsedArticle(string ArticleNumber, string Text, int PageNumber);

/// <summary>
/// Cuts the text of a law into articles.
/// </summary>
public interface IArticleSplitter
{
    IReadOnlyList<ParsedArticle> Split(IReadOnlyList<PdfPageText> pages);
}
