using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sitko.Blazor.ScriptInjector;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScriptInjector();
await builder.Build().RunAsync();
