using LaborLawAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Data;

/// <summary>
/// The connection between our C# classes and the SQLite database (EF Core).
/// Each DbSet below becomes one table.
/// Registered for dependency injection in Program.cs, so controllers and services can ask for it.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LawDocument> LawDocuments => Set<LawDocument>();
    public DbSet<LawArticle> LawArticles => Set<LawArticle>();
    public DbSet<QuestionLog> QuestionLogs => Set<QuestionLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Save the Country enum as text ("SaudiArabia") instead of a number (0),
        // so it is easy to read in DBeaver.
        configurationBuilder.Properties<Country>().HaveConversion<string>();
    }
}
