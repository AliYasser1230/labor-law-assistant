using System.Text.Encodings.Web;
using System.Text.Unicode;
using Anthropic;
using Microsoft.Extensions.Options;
using LaborLawAssistant;
using LaborLawAssistant.Data;
using LaborLawAssistant.Services;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDocumentStorage, DocumentStorage>();
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

builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource)));

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
