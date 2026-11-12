using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace LaborLawAssistant.Services;

public class PdfTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PdfPageText> ExtractPages(Stream pdf)
    {
        using var document = PdfDocument.Open(pdf);

        var pages = new List<PdfPageText>();
        foreach (var page in document.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);

            text = text.Normalize(NormalizationForm.FormKC);

            pages.Add(new PdfPageText(page.Number, text));
        }

        return pages;
    }
}
