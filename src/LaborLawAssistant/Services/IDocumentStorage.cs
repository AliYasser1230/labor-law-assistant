namespace LaborLawAssistant.Services;

public interface IDocumentStorage
{
    Task<string> SaveAsync(Stream pdf, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
}
