namespace LaborLawAssistant.Services;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);

    Task<float[]> EmbedQueryAsync(string question, CancellationToken cancellationToken = default);
}

public class EmbeddingException(string message, Exception? innerException = null) : Exception(message, innerException);
