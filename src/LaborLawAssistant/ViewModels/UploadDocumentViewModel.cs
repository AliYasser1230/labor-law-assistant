using System.ComponentModel.DataAnnotations;
using LaborLawAssistant.Models;
using LaborLawAssistant.Validation;

namespace LaborLawAssistant.ViewModels;

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
