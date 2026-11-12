using LaborLawAssistant.Data;
using LaborLawAssistant.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Controllers;

public class HistoryController(AppDbContext db) : Controller
{
    private const int QuestionsPerPage = 10;

    public async Task<IActionResult> Index(int page = 1)
    {
        var totalQuestions = await db.QuestionLogs.CountAsync();
        var answeredQuestions = await db.QuestionLogs.CountAsync(log => log.InputTokens > 0);

        var totalPages = Math.Max(1, (int)Math.Ceiling(totalQuestions / (double)QuestionsPerPage));
        page = Math.Clamp(page, 1, totalPages);

        var items = await db.QuestionLogs
            .OrderByDescending(log => log.Id)
            .Skip((page - 1) * QuestionsPerPage)
            .Take(QuestionsPerPage)
            .Select(log => new HistoryItemViewModel(
                log.Country,
                log.Question,
                log.Answer,
                log.InputTokens > 0,
                log.AskedAt))
            .ToListAsync();

        return View(new HistoryPageViewModel(
            totalQuestions,
            answeredQuestions,
            new PagedResult<HistoryItemViewModel>(items, page, QuestionsPerPage, totalQuestions)));
    }
}
