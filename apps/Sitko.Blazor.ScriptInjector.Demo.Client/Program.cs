using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Events;
using Sitko.Blazor.ScriptInjector;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScriptInjector();
Log.Logger = new LoggerConfiguration()
    .WriteTo.BrowserConsole(LogEventLevel.Debug, formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
await builder.Build().RunAsync();
