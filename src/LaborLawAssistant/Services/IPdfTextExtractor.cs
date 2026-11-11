namespace LaborLawAssistant.Services;

/// <summary>The text of one PDF page and its page number (1, 2, 3...).</summary>
public record PdfPageText(int PageNumber, string Text);

/// <summary>
/// Reads the text of a PDF, page by page.
/// </summary>
public interface IPdfTextExtractor
{
    IReadOnlyList<PdfPageText> ExtractPages(string filePath);
}
