using LaborLawAssistant.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Data;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<LawDocument> LawDocuments => Set<LawDocument>();
    public DbSet<LawArticle> LawArticles => Set<LawArticle>();
    public DbSet<QuestionLog> QuestionLogs => Set<QuestionLog>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Country>().HaveConversion<string>();
    }
}
