namespace LaborLawAssistant.Services;

public class DocumentStorage(IWebHostEnvironment environment) : IDocumentStorage
{
    private string Folder => Path.Combine(environment.ContentRootPath, "App_Data", "LawPdfs");

    public async Task<string> SaveAsync(IFormFile file)
    {
        Directory.CreateDirectory(Folder);

        var storedFileName = $"{Guid.NewGuid()}.pdf";

        await using var stream = File.Create(GetFullPath(storedFileName));
        await file.CopyToAsync(stream);

        return storedFileName;
    }

    public string GetFullPath(string storedFileName) => Path.Combine(Folder, storedFileName);

    public void Delete(string storedFileName) => File.Delete(GetFullPath(storedFileName));
}
