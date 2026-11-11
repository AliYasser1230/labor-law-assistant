using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Validation;

/// <summary>
/// Model validation rule: the uploaded file must not be bigger than maxBytes.
/// Use it like [MaxFileSize(20 * 1024 * 1024)] on an IFormFile property.
/// Checked on the server (IsValid) and in the browser (AddValidation + wwwroot/js/file-validation.js).
/// </summary>
public class MaxFileSizeAttribute(long maxBytes) : ValidationAttribute, IClientModelValidator
{
    private const string Message = "The file must be smaller than {0} MB.";

    // Server check: runs during model binding, before the controller action.
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file && file.Length > maxBytes)
        {
            var localizer = validationContext.GetService<IStringLocalizer<SharedResource>>();
            return new ValidationResult(GetMessage(localizer));
        }

        return ValidationResult.Success;
    }

    // Browser check: adds data-val-* attributes to the <input>; jQuery Validation reads them.
    public void AddValidation(ClientModelValidationContext context)
    {
        var localizer = context.ActionContext.HttpContext.RequestServices.GetService<IStringLocalizer<SharedResource>>();
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-maxfilesize"] = GetMessage(localizer);
        context.Attributes["data-val-maxfilesize-max"] = maxBytes.ToString();
    }

    // The message in the user's language (English or Arabic).
    private string GetMessage(IStringLocalizer? localizer)
    {
        var megabytes = maxBytes / 1024 / 1024;
        return localizer?[Message, megabytes] ?? string.Format(Message, megabytes);
    }
}
