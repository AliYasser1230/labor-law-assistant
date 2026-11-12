using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Services;

public class GoogleCloudDocumentStorage(StorageClient client, IOptions<StorageOptions> options) : IDocumentStorage
{
    private const string Folder = "law-pdfs";

    public async Task<string> SaveAsync(Stream pdf, CancellationToken cancellationToken = default)
    {
        var objectName = $"{Folder}/{Guid.NewGuid()}.pdf";

        await client.UploadObjectAsync(options.Value.BucketName, objectName, "application/pdf", pdf, cancellationToken: cancellationToken);

        return objectName;
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default) =>
        client.DeleteObjectAsync(options.Value.BucketName, storedFileName, cancellationToken: cancellationToken);
}
