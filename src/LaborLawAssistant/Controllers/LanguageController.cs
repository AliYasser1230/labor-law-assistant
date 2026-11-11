using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaborLawAssistant.Controllers;

public class LanguageController(IOptions<RequestLocalizationOptions> localizationOptions) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string? culture, string? returnUrl)
    {
        var isSupported = localizationOptions.Value.SupportedUICultures?
            .Any(supported => supported.Name == culture) == true;

        if (isSupported)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture!)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
        }

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
