using System.ComponentModel.DataAnnotations;
using LaborLawAssistant.Models;
using LaborLawAssistant.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LaborLawAssistant.ViewModels;

public class AskQuestionViewModel
{
    public const int MaxQuestionLength = 1000;

    [Display(Name = "Country")]
    [Required(ErrorMessage = "Please choose a country.")]
    public Country Country { get; set; }

    [Display(Name = "Your question")]
    [Required(ErrorMessage = "Please type your question.")]
    [StringLength(MaxQuestionLength, MinimumLength = 5, ErrorMessage = "The question must be between {2} and {1} characters.")]
    public string Question { get; set; } = "";

    [BindNever]
    public List<Country> AvailableCountries { get; set; } = [];

    [BindNever]
    public SearchResult? Result { get; set; }

    [BindNever]
    public LawAnswer? Answer { get; set; }
}
