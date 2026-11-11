using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Validation;

/// <summary>
/// Model validation rule: the uploaded file must be a real PDF.
/// Server: the name must end with ".pdf" AND the file must start with "%PDF-"
/// (every PDF starts with these bytes, so renaming photo.jpg to photo.pdf does not pass).
/// Browser: only the ".pdf" name is checked (see wwwroot/js/file-validation.js).
/// </summary>
public class PdfFileAttribute : ValidationAttribute, IClientModelValidator
{
    private const string Message = "Only PDF files are allowed.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file && !IsPdf(file))
        {
            var localizer = validationContext.GetService<IStringLocalizer<SharedResource>>();
            return new ValidationResult(localizer?[Message] ?? Message);
        }

        return ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        var localizer = context.ActionContext.HttpContext.RequestServices.GetService<IStringLocalizer<SharedResource>>();
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-pdffile"] = localizer?[Message] ?? Message;
    }

    private static bool IsPdf(IFormFile file)
    {
        if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Read the first 5 bytes and compare them with "%PDF-".
        using var stream = file.OpenReadStream();
        var header = new byte[5];
        var bytesRead = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        return bytesRead == header.Length && Encoding.ASCII.GetString(header) == "%PDF-";
    }
}
