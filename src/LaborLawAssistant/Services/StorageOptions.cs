namespace LaborLawAssistant.Services;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public const string GoogleCloudStorageProvider = "GoogleCloudStorage";

    public string Provider { get; set; } = "Local";

    public string BucketName { get; set; } = "";

    public bool UseGoogleCloudStorage => Provider.Equals(GoogleCloudStorageProvider, StringComparison.OrdinalIgnoreCase);
}
