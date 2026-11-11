using LaborLawAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LawDocument> LawDocuments => Set<LawDocument>();
    public DbSet<LawArticle> LawArticles => Set<LawArticle>();
    public DbSet<QuestionLog> QuestionLogs => Set<QuestionLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Country>().HaveConversion<string>();
    }
}
