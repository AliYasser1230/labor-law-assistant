using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LaborLawAssistant.Models;

public enum Country
{
    [Display(Name = "Saudi Arabia")]
    SaudiArabia,

    [Display(Name = "United Arab Emirates")]
    UnitedArabEmirates,
}

public static class CountryExtensions
{
    public static string GetDisplayName(this Country country) =>
        typeof(Country).GetField(country.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name ?? country.ToString();
}
