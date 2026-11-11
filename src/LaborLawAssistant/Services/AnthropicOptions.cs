namespace LaborLawAssistant.Services;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "claude-opus-5-5";

    public decimal InputPricePerMillionTokens { get; set; } = 4m;

    public decimal OutputPricePerMillionTokens { get; set; } = 20m;
}
