namespace LaborLawAssistant.Services;

/// <summary>
/// Saves PDFs in the App_Data/LawPdfs folder of the project.
/// App_Data is outside wwwroot, so the files can't be downloaded directly by URL,
/// and it is in .gitignore, so uploaded PDFs are not pushed to GitHub.
/// </summary>
public class DocumentStorage(IWebHostEnvironment environment) : IDocumentStorage
{
    private string Folder => Path.Combine(environment.ContentRootPath, "App_Data", "LawPdfs");

    public async Task<string> SaveAsync(IFormFile file)
    {
        Directory.CreateDirectory(Folder);

        // Never use the user's file name on disk: it could contain "../" or strange characters.
        // A random name (GUID) is always safe and never overwrites another file.
        var storedFileName = $"{Guid.NewGuid()}.pdf";

        await using var stream = File.Create(GetFullPath(storedFileName));
        await file.CopyToAsync(stream);

        return storedFileName;
    }

    public string GetFullPath(string storedFileName) => Path.Combine(Folder, storedFileName);
}
