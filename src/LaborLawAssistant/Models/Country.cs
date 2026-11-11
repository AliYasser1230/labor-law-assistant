using System.ComponentModel.DataAnnotations;

namespace LaborLawAssistant.Models;

/// <summary>
/// Countries whose labor law the app knows.
/// Saved in the database as text ("SaudiArabia"), see AppDbContext.
/// [Display] gives the name shown in drop-downs and tables (translated to Arabic via SharedResource).
/// Egypt and UAE will be added later.
/// </summary>
public enum Country
{
    [Display(Name = "Saudi Arabia")]
    SaudiArabia,
}
