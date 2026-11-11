using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LaborLawAssistant.Models;

/// <summary>
/// Countries whose labor law the app knows.
/// Saved in the database as text ("SaudiArabia"), see AppDbContext.
/// [Display] gives the name shown in drop-downs and tables (translated to Arabic via SharedResource).
/// Add a new country here (and its Arabic name in SharedResource.ar.resx); no database change is needed.
/// </summary>
public enum Country
{
    [Display(Name = "Saudi Arabia")]
    SaudiArabia,

    [Display(Name = "United Arab Emirates")]
    UnitedArabEmirates,
}

public static class CountryExtensions
{
    /// <summary>The [Display] name of a country, e.g. "United Arab Emirates" (translate it with Localizer).</summary>
    public static string GetDisplayName(this Country country) =>
        typeof(Country).GetField(country.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name ?? country.ToString();
}
