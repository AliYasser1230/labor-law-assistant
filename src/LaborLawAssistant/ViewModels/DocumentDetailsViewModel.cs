using LaborLawAssistant.Models;

namespace LaborLawAssistant.ViewModels;

/// <summary>
/// One law and one page of its articles (Views/Documents/Details.cshtml).
/// Lets the admin check that the PDF was cut into articles correctly.
/// </summary>
public record DocumentDetailsViewModel(
    string Title,
    Country Country,
    int PageCount,
    PagedResult<ArticleItemViewModel> Articles);

/// <summary>One article card. Heading = the article's short title, if the law has one ("Annual Leave").</summary>
public record ArticleItemViewModel(string ArticleNumber, int PageNumber, string? Heading, string Text)
{
    public static ArticleItemViewModel Create(string articleNumber, int pageNumber, string text)
    {
        // Many laws (like the UAE law) put a short title on the first line of each article.
        // A first line counts as a title if it is short, starts with a capital letter,
        // and does not end like a sentence.
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
