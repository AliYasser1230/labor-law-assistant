namespace LaborLawAssistant.Models;

/// <summary>
/// Countries whose labor law the app knows.
/// Saved in the database as text ("SaudiArabia"), see AppDbContext.
/// Egypt and UAE will be added later.
/// </summary>
public enum Country
{
    SaudiArabia,
}
