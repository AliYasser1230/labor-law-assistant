namespace LaborLawAssistant.Services;

public class LocalDocumentStorage(IWebHostEnvironment environment) : IDocumentStorage
{
    private string Folder => Path.Combine(environment.ContentRootPath, "App_Data", "LawPdfs");

    public async Task<string> SaveAsync(Stream pdf, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Folder);

        var storedFileName = $"{Guid.NewGuid()}.pdf";

        await using var file = File.Create(Path.Combine(Folder, storedFileName));
        await pdf.CopyToAsync(file, cancellationToken);

        return storedFileName;
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        File.Delete(Path.Combine(Folder, storedFileName));
        return Task.CompletedTask;
    }
}
