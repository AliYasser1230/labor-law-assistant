using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

public record DocumentDetailsViewModel(
    int Id,
    string Title,
    Country Country,
    int PageCount,
    int EmbeddedCount,
    PagedResult<ArticleItemViewModel> Articles);

public record ArticleItemViewModel(string ArticleNumber, int PageNumber, string? Heading, string Text)
{
    public static ArticleItemViewModel Create(string articleNumber, int pageNumber, string text)
    {
        var lineEnd = text.IndexOf('\n');
        if (lineEnd > 0)
        {
            var firstLine = text[..lineEnd].Trim();
            var looksLikeTitle = firstLine.Length is > 0 and <= 100
                                 && char.IsUpper(firstLine[0])
                                 && !firstLine.EndsWith('.') && !firstLine.EndsWith(':') && !firstLine.EndsWith(',');
            if (looksLikeTitle)
            {
                return new ArticleItemViewModel(articleNumber, pageNumber, firstLine, text[(lineEnd + 1)..].Trim());
            }
        }

        return new ArticleItemViewModel(articleNumber, pageNumber, null, text);
    }
}
