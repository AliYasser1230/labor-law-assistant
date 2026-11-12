namespace LaborLawAssistant.Services;

public record PdfPageText(int PageNumber, string Text);

public interface IPdfTextExtractor
{
    IReadOnlyList<PdfPageText> ExtractPages(Stream pdf);
}
