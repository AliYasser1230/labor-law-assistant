namespace LaborLawAssistant.Services;

public class SearchOptions
{
    public const string SectionName = "Search";

    public int TopK { get; set; } = 5;

    public double MinScore { get; set; } = 0.4;
}
