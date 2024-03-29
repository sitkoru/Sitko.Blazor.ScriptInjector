using System.Globalization;
using Serilog;
using Sitko.Blazor.ScriptInjector;
using Sitko.Blazor.ScriptInjector.Demo.Client.Pages;
using Sitko.Blazor.ScriptInjector.Demo.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddScriptInjector();

Log.Logger = new LoggerConfiguration()
    .WriteTo.BrowserConsole(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();
builder.Logging.ClearProviders().AddSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Home).Assembly);

app.Run();
