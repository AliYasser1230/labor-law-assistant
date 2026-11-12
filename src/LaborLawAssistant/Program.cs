using System.Text.Encodings.Web;
using System.Text.Unicode;
using Anthropic;
using Google.Api.Gax;
using Google.Cloud.Storage.V1;
using LaborLawAssistant;
using LaborLawAssistant.Data;
using LaborLawAssistant.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] cultures = ["en", "ar"];
    options.SetDefaultCulture("en")
        .AddSupportedCultures(cultures)
        .AddSupportedUICultures(cultures);
});

var database = builder.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
if (database.UsePostgreSql)
{
    builder.Services.AddDbContext<AppDbContext, PostgresAppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddDataProtection()
    .SetApplicationName("LaborLawAssistant")
    .PersistKeysToDbContext<AppDbContext>();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
var storage = builder.Configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();
if (storage.UseGoogleCloudStorage)
{
    builder.Services.AddSingleton(_ => new StorageClientBuilder { EmulatorDetection = EmulatorDetection.EmulatorOrProduction }.Build());
    builder.Services.AddScoped<IDocumentStorage, GoogleCloudDocumentStorage>();
}
else
{
    builder.Services.AddScoped<IDocumentStorage, LocalDocumentStorage>();
}

builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddScoped<IArticleSplitter, ArticleSplitter>();
builder.Services.AddScoped<ILawDocumentImporter, LawDocumentImporter>();

builder.Services.Configure<VoyageAIOptions>(builder.Configuration.GetSection(VoyageAIOptions.SectionName));
builder.Services.AddHttpClient<IEmbeddingService, VoyageEmbeddingService>(client =>
{
    client.BaseAddress = new Uri("https://api.voyageai.com/v1/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.Configure<SearchOptions>(builder.Configuration.GetSection(SearchOptions.SectionName));
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection(AnthropicOptions.SectionName));
builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AnthropicOptions>>().Value;
    return new AnthropicClient { ApiKey = settings.ApiKey };
});
builder.Services.AddScoped<IAnswerService, ClaudeAnswerService>();

builder.Services.AddHealthChecks();

builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource)));

var app = builder.Build();

if (database.MigrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
