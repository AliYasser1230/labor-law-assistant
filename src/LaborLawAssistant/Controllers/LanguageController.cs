using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Controllers;

/// <summary>
/// Switches the site language (English / Arabic).
/// The choice is saved in a cookie, and the localization middleware reads it on every request.
/// </summary>
// Dependency injection: ASP.NET Core passes the language options from Program.cs into this constructor.
public class LanguageController(IOptions<RequestLocalizationOptions> localizationOptions) : Controller
{
    // Model binding: "culture" and "returnUrl" come from the form fields in _Layout.cshtml.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string? culture, string? returnUrl)
    {
        // Only accept a language that the app supports ("en" or "ar").
        var isSupported = localizationOptions.Value.SupportedUICultures?
            .Any(supported => supported.Name == culture) == true;

        if (isSupported)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture!)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
        }

        // Go back to the same page, but only if it is a page on our own site (safe redirect).
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
