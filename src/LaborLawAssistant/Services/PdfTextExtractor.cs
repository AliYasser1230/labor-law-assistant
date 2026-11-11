using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace LaborLawAssistant.Services;

/// <summary>
/// Reads PDF text with PdfPig (a free .NET library).
/// </summary>
public class PdfTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PdfPageText> ExtractPages(string filePath)
    {
        using var document = PdfDocument.Open(filePath);

        var pages = new List<PdfPageText>();
        foreach (var page in document.GetPages())
        {
            // ContentOrderTextExtractor keeps the reading order and the line breaks.
            // We need the line breaks to find the lines that start with "Article ...".
            var text = ContentOrderTextExtractor.GetText(page);

            // PDFs often use special joined letters, like "ﬁ" (one character) instead of "fi".
            // FormKC turns them back into normal letters, so search and Claude see normal words.
            text = text.Normalize(NormalizationForm.FormKC);

            pages.Add(new PdfPageText(page.Number, text));
        }

        return pages;
    }
}
