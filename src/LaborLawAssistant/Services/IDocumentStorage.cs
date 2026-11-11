namespace LaborLawAssistant.Services;

/// <summary>
/// Saves uploaded law PDFs on disk and finds them again.
/// An interface, so controllers depend on "what it does", not "how" (dependency injection, easy to test).
/// </summary>
public interface IDocumentStorage
{
    /// <summary>Saves the file and returns the random name it was saved under.</summary>
    Task<string> SaveAsync(IFormFile file);

    /// <summary>The full path on disk of a saved file (used in step 4 to read the PDF).</summary>
    string GetFullPath(string storedFileName);
}
