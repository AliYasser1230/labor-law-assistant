using System.Text.Encodings.Web;
using System.Text.Unicode;
using LaborLawAssistant;
using LaborLawAssistant.Data;
using LaborLawAssistant.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (dependency injection).

// Write Arabic letters as normal text in the HTML (not as &#x...; codes).
// Dangerous characters like < > & " are still escaped.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Localization: Arabic translations live in Resources/SharedResource.ar.resx.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// The app supports two languages. English is the default.
// These options are used by the localization middleware and by LanguageController.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] cultures = ["en", "ar"];
    options.SetDefaultCulture("en")
        .AddSupportedCultures(cultures)
        .AddSupportedUICultures(cultures);
});

// Database: EF Core + SQLite. The connection string is in appsettings.json ("DefaultConnection").
// Any controller or service can now get an AppDbContext in its constructor.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Our own services. "Scoped" = one object per web request.
builder.Services.AddScoped<IDocumentStorage, DocumentStorage>();

builder.Services.AddControllersWithViews()
    // Translate [Display] names and validation messages using SharedResource.ar.resx.
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Picks the language for each request (from the language cookie).
// Must run before MVC, so controllers and views see the right language.
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
