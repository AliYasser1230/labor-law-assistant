namespace LaborLawAssistant.Services;

public interface IDocumentStorage
{
    Task<string> SaveAsync(IFormFile file);

    string GetFullPath(string storedFileName);

    void Delete(string storedFileName);
}
