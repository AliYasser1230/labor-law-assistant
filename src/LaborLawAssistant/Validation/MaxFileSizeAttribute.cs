using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Validation;

public class MaxFileSizeAttribute(long maxBytes) : ValidationAttribute, IClientModelValidator
{
    private const string Message = "The file must be smaller than {0} MB.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file && file.Length > maxBytes)
        {
            var localizer = validationContext.GetService<IStringLocalizer<SharedResource>>();
            return new ValidationResult(GetMessage(localizer));
        }

        return ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        var localizer = context.ActionContext.HttpContext.RequestServices.GetService<IStringLocalizer<SharedResource>>();
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-maxfilesize"] = GetMessage(localizer);
        context.Attributes["data-val-maxfilesize-max"] = maxBytes.ToString();
    }

    private string GetMessage(IStringLocalizer? localizer)
    {
        var megabytes = maxBytes / 1024 / 1024;
        return localizer?[Message, megabytes] ?? string.Format(Message, megabytes);
    }
}
