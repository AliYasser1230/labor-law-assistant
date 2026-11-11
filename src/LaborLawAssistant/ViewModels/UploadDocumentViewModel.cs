using System.ComponentModel.DataAnnotations;
using LaborLawAssistant.Models;
using LaborLawAssistant.Validation;

namespace LaborLawAssistant.ViewModels;

/// <summary>
/// The data of the upload form (Views/Documents/Upload.cshtml).
/// Model binding fills these properties from the form fields with the same names.
/// The attributes are the model validation rules; the result is in ModelState.IsValid.
/// Display names and error messages are English keys, translated to Arabic in SharedResource.ar.resx.
/// </summary>
public class UploadDocumentViewModel
{
    public const int MaxFileSizeMb = 20;

    [Display(Name = "Country")]
    [Required(ErrorMessage = "Please choose a country.")]
    public Country Country { get; set; }

    [Display(Name = "Law title")]
    [Required(ErrorMessage = "Please enter the law title.")]
    [StringLength(200, ErrorMessage = "The title can have at most {1} characters.")]
    public string Title { get; set; } = "";

    [Display(Name = "PDF file")]
    [Required(ErrorMessage = "Please choose a PDF file.")]
    [PdfFile]
    [MaxFileSize(MaxFileSizeMb * 1024 * 1024)]
    public IFormFile? File { get; set; }
}
