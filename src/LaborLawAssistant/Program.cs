using System.Text.Encodings.Web;
using System.Text.Unicode;
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

builder.Services.AddControllersWithViews();

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
