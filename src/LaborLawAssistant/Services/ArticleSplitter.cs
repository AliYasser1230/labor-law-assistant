using System.Text;
using System.Text.RegularExpressions;

namespace LaborLawAssistant.Services;

public partial class ArticleSplitter : IArticleSplitter
{
    [GeneratedRegex(@"^Article\s*\(?\s*(?<number>\d+)\s*\)?(?:\s+(?<suffix>bis|repeated))?\s*(?:[:.\-–]\s*(?<rest>.*))?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex ArticleHeading();

    [GeneratedRegex(@"^(?:\d{1,2}[.)]|[a-zA-Z][.)]|\((?:\d{1,2}|[a-zA-Z]{1,4})\)|[-•–])\s")]
    private static partial Regex ListItemStart();

    [GeneratedRegex(@"^(?:-\s*)?(?:page\s*)?\d+(?:\s*(?:/|of)\s*\d+)?(?:\s*-)?$", RegexOptions.IgnoreCase)]
    private static partial Regex PageNumberLine();

    public IReadOnlyList<ParsedArticle> Split(IReadOnlyList<PdfPageText> pages)
    {
        var articles = new List<ParsedArticle>();
        var headersAndFooters = FindHeadersAndFooters(pages);

        string? currentNumber = null;
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

                if (!isHeading && (line.Length == 0
                                   || PageNumberLine().IsMatch(line)
                                   || headersAndFooters.Contains(WithoutPageNumber(line))))
                {
                    continue;
                }

                if (isHeading)
                {
                    AddArticle(articles, currentNumber, currentText, currentPage);

                    var number = int.Parse(heading.Groups["number"].Value);
                    var suffix = heading.Groups["suffix"].Value;
                    currentNumber = suffix.Length > 0 ? $"{number} {suffix.ToLowerInvariant()}" : number.ToString();
                    currentPage = page.PageNumber;
                    currentText.Clear();
                    previousLine = "";
                    linesInArticle = 0;
                    AppendLine(currentText, heading.Groups["rest"].Value, ref previousLine, ref linesInArticle);
                    lastNumber = number;
                }
                else if (currentNumber is not null)
                {
                    AppendLine(currentText, line, ref previousLine, ref linesInArticle);
                }
            }
        }

        AddArticle(articles, currentNumber, currentText, currentPage);
        return articles;
    }

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

    [GeneratedRegex(@"^\d{1,4}\s+|\s+\d{1,4}$")]
    private static partial Regex PageNumberAtEdge();

    private static string WithoutPageNumber(string line) => PageNumberAtEdge().Replace(line, "").Trim();

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
                .Distinct())
            .GroupBy(line => line)
            .Where(group => group.Count() >= pages.Count / 2)
            .Select(group => group.Key)
            .ToHashSet();
    }

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
