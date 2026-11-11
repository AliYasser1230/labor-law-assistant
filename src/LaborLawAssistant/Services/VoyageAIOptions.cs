namespace LaborLawAssistant.Services;

public class VoyageAIOptions
{
    public const string SectionName = "VoyageAI";

    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "voyage-4";
}
