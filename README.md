# Sitko.Blazor.ScriptInjector

![Nuget](https://img.shields.io/nuget/dt/Sitko.Blazor.ScriptInjector) ![Nuget](https://img.shields.io/nuget/v/Sitko.Blazor.ScriptInjector)

Library for script and css injection to Blazor pages

# Installation

```
dotnet add package Sitko.Blazor.ScriptInjector
```

Register in DI and configure in `Startup.cs`

```c#
services.AddScriptInjector(Configuration);
```

# Usage

Inject `IScriptInjector`

```c#
@inject IScriptInjector _scriptInjector
```

## Inject inline script

```c#
await _scriptInjector.InjectAsync(ScriptInjectRequest.Inline("inline", "console.log('Inline script is executed');"));
```

## Inject script from url

```c#
await _scriptInjector.InjectAsync(ScriptInjectRequest.FromUrl("url", "/script.js"));
```

## Script from resource

Embed script as resource in your `.csproj`

```xml
<ItemGroup>
    <EmbeddedResource Include="assembly.js" LogicalName="assembly.js" />
</ItemGroup>
```

Then inject

```c#
await _scriptInjector.InjectAsync(ScriptInjectRequest.FromResource("resource", GetType().Assembly, "assembly.js"));
```

## Inject inline css

```c#
await _scriptInjector.InjectAsync(CssInjectRequest.Inline("inlinecss", "body{background: blue;}"));
```

## Inject css from url

```c#
await _scriptInjector.InjectAsync(CssInjectRequest.FromUrl("urlcss", "/style.css"));
```

## Css from resource

Embed script as resource in your `.csproj`

```xml
<ItemGroup>
    <EmbeddedResource Include="assembly.css" LogicalName="assembly.css" />
</ItemGroup>
```

Then inject

```c#
await _scriptInjector.InjectAsync(CssInjectRequest.FromResource("resourcecss", GetType().Assembly, "assembly.css"));
```

## Run code after script or css is loaded

Pass callback to `InjectAsync`. For example - chain script load:

```c#
private Task LoadInlineScriptAsync()
{
    return _scriptInjector.InjectAsync(ScriptInjectRequest.Inline("inline", "console.log('Inline script is executed');"), LoadUrlScriptAsync);
}

private Task LoadUrlScriptAsync(CancellationToken cancellationToken)
{
    return _scriptInjector.InjectAsync(ScriptInjectRequest.FromUrl("url", "/script.js"), LoadResourceScriptAsync, cancellationToken);
}

private Task LoadResourceScriptAsync(CancellationToken cancellationToken)
{
    return _scriptInjector.InjectAsync(ScriptInjectRequest.FromResource("resource", GetType().Assembly, "assembly.js"), _ =>
    {
        // all scripts are loaded
        return Task.CompletedTask;
    }, cancellationToken);
}
});
```

## Multiple scripts and styles at once

```c#
await _scriptInjector.InjectAsync(new[]
{
    ScriptInjectRequest.Inline("inline", "console.log('Inline script is executed');")
    ScriptInjectRequest.FromUrl("url", "/script.js"),
    ScriptInjectRequest.FromResource("resource", GetType().Assembly, "assembly.js"),
    CssInjectRequest.Inline("inlinecss", "body{background: blue;}"),
    CssInjectRequest.FromResource("resourcecss", GetType().Assembly, "assembly.css"),
    CssInjectRequest.FromUrl("urlcss", "/style.css")
}, _ =>
{
    // all scripts are loaded
    return Task.CompletedTask;
});
```

## Scripts deduplication

All script requests with same id will be executed only once per scope

```c#
await _scriptInjector.InjectAsync(new[]
{
    _scriptInjector.Inline("inline", "console.log('Inline script is executed');"),
    _scriptInjector.Inline("inline", "console.log('Inline script 2 is executed');"),
    _scriptInjector.Inline("inline", "console.log('Inline script 3 is executed');"),
});
```

In browser console will be only
```
Inline script is executed
```

# Demo
1. Clone repo
2. Go to `apps/Sitko.Blazor.ScriptInjector.Demo`
3. Run `dotnet run`
4. Open [https://localhost:5001/](https://localhost:5001/) in browser
