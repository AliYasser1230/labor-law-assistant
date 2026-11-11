using LaborLawAssistant.Data;
using LaborLawAssistant.Models;
using LaborLawAssistant.Services;
using LaborLawAssistant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LaborLawAssistant.Controllers;

public class AskController(
    AppDbContext db,
    ISearchService searchService,
    IAnswerService answerService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AskController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new AskQuestionViewModel { AvailableCountries = await GetAvailableCountriesAsync() };
        if (model.AvailableCountries.Count > 0)
        {
            model.Country = model.AvailableCountries[0];
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AskQuestionViewModel model, CancellationToken cancellationToken)
    {
        model.AvailableCountries = await GetAvailableCountriesAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var question = model.Question.Trim();

        try
        {
            model.Result = await searchService.SearchAsync(model.Country, question, cancellationToken);
        }
        catch (EmbeddingException exception)
        {
            logger.LogWarning(exception, "Search failed");
            ModelState.AddModelError(string.Empty, localizer["The search is not available right now. Please try again in a minute."]);
            return View(model);
        }

        if (model.Result.Found)
        {
            try
            {
                model.Answer = await answerService.AnswerAsync(model.Country, question, model.Result.Hits, cancellationToken);
            }
            catch (AnswerException exception)
            {
                logger.LogWarning(exception, "Answer failed");
                ModelState.AddModelError(string.Empty, localizer["The answer is not available right now. The closest articles are shown below."]);
            }
        }

        await SaveToHistoryAsync(model, question, cancellationToken);
        return View(model);
    }

    private async Task SaveToHistoryAsync(AskQuestionViewModel model, string question, CancellationToken cancellationToken)
    {
        string answerText;
        if (model.Result is { Found: false })
        {
            answerText = "I couldn't find this in the law.";
        }
        else if (model.Answer is null)
        {
            return;
        }
        else if (model.Answer.Refused)
        {
            answerText = "(Claude declined to answer.)";
        }
        else
        {
            var cited = model.Answer.Parts.SelectMany(part => part.SourceNumbers).Distinct().Order().ToList();
            var sources = cited.Select(number =>
            {
                var hit = model.Result!.Hits[number - 1];
                return $"[{number}] {hit.LawTitle}, Article {hit.ArticleNumber}, page {hit.PageNumber}";
            });
            answerText = model.Answer.ToPlainText().Trim()
                         + (cited.Count > 0 ? "\n\nSources:\n" + string.Join("\n", sources) : "");
        }

        db.QuestionLogs.Add(new QuestionLog
        {
            Country = model.Country,
            Question = question,
            Answer = answerText,
            InputTokens = model.Answer?.InputTokens ?? 0,
            OutputTokens = model.Answer?.OutputTokens ?? 0,
            CostUsd = model.Answer?.CostUsd ?? 0,
            AskedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private Task<List<Country>> GetAvailableCountriesAsync() =>
        db.LawDocuments.Select(document => document.Country).Distinct().ToListAsync();
}
