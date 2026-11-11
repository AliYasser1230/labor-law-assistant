using System.Text;
using System.Text.RegularExpressions;

namespace LaborLawAssistant.Services;

/// <summary>
/// Cuts law text into articles by finding the heading lines ("Article 1", "Article (12):", "Article 77 bis").
/// Everything after a heading, until the next heading, is the text of that article.
/// Text before the first article (title, introduction), page numbers, and page headers/footers are skipped.
/// </summary>
public partial class ArticleSplitter : IArticleSplitter
{
    // A heading line: "Article" + number, then optional "bis", then the end of the line or ":" "." "-" and more text.
    // The ":"/"." rule stops a sentence that wraps onto a new line ("Article 5 of this Law...") from looking like a heading.
    [GeneratedRegex(@"^Article\s*\(?\s*(?<number>\d+)\s*\)?(?:\s+(?<suffix>bis|repeated))?\s*(?:[:.\-–]\s*(?<rest>.*))?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex ArticleHeading();

    // A line that starts a list item: "1.", "2)", "a.", "(b)", "(iv)", "-", "•".
    [GeneratedRegex(@"^(?:\d{1,2}[.)]|[a-zA-Z][.)]|\((?:\d{1,2}|[a-zA-Z]{1,4})\)|[-•–])\s")]
    private static partial Regex ListItemStart();

    // Lines that are only a page number: "12", "- 12 -", "Page 12", "12 / 60", "Page 12 of 60".
    [GeneratedRegex(@"^(?:-\s*)?(?:page\s*)?\d+(?:\s*(?:/|of)\s*\d+)?(?:\s*-)?$", RegexOptions.IgnoreCase)]
    private static partial Regex PageNumberLine();

    public IReadOnlyList<ParsedArticle> Split(IReadOnlyList<PdfPageText> pages)
    {
        var articles = new List<ParsedArticle>();
        var headersAndFooters = FindHeadersAndFooters(pages);

        string? currentNumber = null;   // the article we are reading now (null = before the first article)
        var currentPage = 0;
        var currentText = new StringBuilder();
        var previousLine = "";
        var linesInArticle = 0;
        var lastNumber = 0;

        foreach (var page in pages)
        {
            foreach (var rawLine in page.Text.Split('\n'))
            {
                var line = rawLine.Trim();
                var heading = ArticleHeading().Match(line);
                var isHeading = heading.Success && IsNextArticle(heading, lastNumber);

                // Skip empty lines, page numbers, and headers/footers (but never an article heading).
                if (!isHeading && (line.Length == 0
                                   || PageNumberLine().IsMatch(line)
                                   || headersAndFooters.Contains(WithoutPageNumber(line))))
                {
                    continue;
                }

                if (isHeading)
                {
                    // A new article starts: save the previous one first.
                    AddArticle(articles, currentNumber, currentText, currentPage);

                    var number = int.Parse(heading.Groups["number"].Value);
                    var suffix = heading.Groups["suffix"].Value;
                    currentNumber = suffix.Length > 0 ? $"{number} {suffix.ToLowerInvariant()}" : number.ToString();
                    currentPage = page.PageNumber;
                    currentText.Clear();
                    previousLine = "";
                    linesInArticle = 0;
                    AppendLine(currentText, heading.Groups["rest"].Value, ref previousLine, ref linesInArticle); // text on the heading line, if any
                    lastNumber = number;
                }
                else if (currentNumber is not null)
                {
                    AppendLine(currentText, line, ref previousLine, ref linesInArticle);
                }
            }
        }

        AddArticle(articles, currentNumber, currentText, currentPage); // the last article
        return articles;
    }

    // PDF lines end where the paper ends, not where the sentence ends. So we join a line to the previous one
    // with a space, and start a new line only when:
    // the previous line ended a sentence (. : ;), or was short (a title like "Annual Leave"),
    // or the new line starts a list item ("1.", "a.", "(b)"),
    // or the previous line was the article's first line and the new line starts with a capital letter
    // (a long title like "Paying the Workers' Entitlements at the End of the Contract Term").
    private static void AppendLine(StringBuilder text, string line, ref string previousLine, ref int linesInArticle)
    {
        if (line.Length == 0)
        {
            return;
        }

        if (text.Length > 0)
        {
            var startsNewLine = previousLine.EndsWith('.') || previousLine.EndsWith(':') || previousLine.EndsWith(';')
                                || previousLine.Length < 60
                                || ListItemStart().IsMatch(line)
                                || (linesInArticle == 1 && char.IsUpper(line[0]));
            text.Append(startsNewLine ? '\n' : ' ');
        }

        text.Append(line);
        previousLine = line;
        linesInArticle++;
    }

    // A page number at the start or end of a line: "02 This is NOT..." or "...translation 3".
    [GeneratedRegex(@"^\d{1,4}\s+|\s+\d{1,4}$")]
    private static partial Regex PageNumberAtEdge();

    // "02 This is NOT an official translation" and "This is NOT an official translation" count as the same line.
    private static string WithoutPageNumber(string line) => PageNumberAtEdge().Replace(line, "").Trim();

    // A line printed on at least half of the pages is a page header or footer
    // (e.g. "This is NOT an official translation" or the law's name), not law text.
    // Only for PDFs with 4+ pages; short lines (under 8 characters) and article headings are never removed.
    private static HashSet<string> FindHeadersAndFooters(IReadOnlyList<PdfPageText> pages)
    {
        if (pages.Count < 4)
        {
            return [];
        }

        return pages
            .SelectMany(page => page.Text.Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length >= 8 && !ArticleHeading().IsMatch(line))
                .Select(WithoutPageNumber)
                .Distinct()) // count each line once per page
            .GroupBy(line => line)
            .Where(group => group.Count() >= pages.Count / 2)
            .Select(group => group.Key)
            .ToHashSet();
    }

    // Article numbers go up: 1, 2, 3... (numbers can be skipped when an article was removed from the law).
    // The same number again is only allowed with "bis" (an article added later, e.g. "77 bis").
    private static bool IsNextArticle(Match heading, int lastNumber)
    {
        var number = int.Parse(heading.Groups["number"].Value);
        var hasSuffix = heading.Groups["suffix"].Success;
        return number > lastNumber || (number == lastNumber && hasSuffix);
    }

    private static void AddArticle(List<ParsedArticle> articles, string? number, StringBuilder text, int page)
    {
        var cleanText = text.ToString().Trim();
        if (number is not null && cleanText.Length > 0)
        {
            articles.Add(new ParsedArticle(number, cleanText, page));
        }
    }
}
