# Plugin System

## Overview

The ElBruno.MarkItDotNet plugin system allows you to package custom converters as reusable satellite packages that are automatically discovered and loaded via dependency injection. This is how the Excel, PowerPoint, and AI packages extend the core library.

## How It Works

1. **Converters** implement `IMarkdownConverter` to handle one or more file formats
2. **Plugins** implement `IConverterPlugin` to bundle related converters
3. **Service Extension** provides a `AddMyPlugin()` DI method to register the plugin
4. **Automatic Discovery** — the `ConverterRegistry` discovers all registered `IConverterPlugin` instances and loads their converters

No manual registration of individual converters needed — just call one `AddMyPlugin()` method, and all converters are available.

## Creating a Custom Plugin Package

### Step 1: Create a New NuGet Package

```bash
dotnet new classlib -n MyCompany.MyFormatPlugin
cd MyCompany.MyFormatPlugin
```

### Step 2: Reference the Core Package

```bash
dotnet add package ElBruno.MarkItDotNet
```

### Step 3: Implement Converters

Create one or more converters that implement `IMarkdownConverter`:

```csharp
using ElBruno.MarkItDotNet;
using System.Text;

namespace MyCompany.MyFormatPlugin;

/// <summary>
/// Converter for .myformat files to Markdown.
/// </summary>
public class MyFormatConverter : IMarkdownConverter
{
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".myformat", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension)
    {
        // Read from stream
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        // Convert to Markdown
        var markdown = ParseAndConvert(content);
        return markdown;
    }

    private static string ParseAndConvert(string content)
    {
        // Your conversion logic here
        return $"# Converted from MyFormat\n\n{content}";
    }
}
```

### Step 4: Implement the Plugin

Create a class that implements `IConverterPlugin`:

```csharp
using ElBruno.MarkItDotNet;

namespace MyCompany.MyFormatPlugin;

/// <summary>
/// Plugin that provides the MyFormat converter.
/// </summary>
public class MyFormatPlugin : IConverterPlugin
{
    /// <inheritdoc />
    public string Name => "MyFormat";

    /// <inheritdoc />
    public IEnumerable<IMarkdownConverter> GetConverters() =>
    [
        new MyFormatConverter(),
        // Add more converters if needed
    ];
}
```

### Step 5: Create the DI Extension

Add a static class with an extension method:

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace MyCompany.MyFormatPlugin;

/// <summary>
/// Extension methods for registering MyFormatPlugin with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MyFormat converter plugin to the service collection.
    /// </summary>
    public static IServiceCollection AddMyFormatPlugin(
        this IServiceCollection services)
    {
        services.AddSingleton<IConverterPlugin>(new MyFormatPlugin());
        return services;
    }
}
```

### Step 6: Package and Publish

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/MyCompany.MyFormatPlugin.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
```

## Using a Plugin

Once your plugin is published, users can install and use it like this:

```bash
dotnet add package MyCompany.MyFormatPlugin
```

Then register it in their DI container:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ElBruno.MarkItDotNet;
using MyCompany.MyFormatPlugin;

var services = new ServiceCollection();

// Register core
services.AddMarkItDotNet();

// Register your plugin
services.AddMyFormatPlugin();

var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();

// Your converter is now available
var result = await markdownService.ConvertAsync("document.myformat");
Console.WriteLine(result.Markdown);
```

## Advanced: Plugin with Options

For plugins that need configuration:

### 1. Create an Options Class

```csharp
namespace MyCompany.MyFormatPlugin;

public class MyFormatOptions
{
    public string? CustomProperty { get; set; }
    public int MaxSize { get; set; } = 10_000_000;
}
```

### 2. Update the Plugin

```csharp
public class MyFormatPlugin : IConverterPlugin
{
    private readonly MyFormatOptions _options;

    public MyFormatPlugin(MyFormatOptions? options = null)
    {
        _options = options ?? new MyFormatOptions();
    }

    public string Name => "MyFormat";

    public IEnumerable<IMarkdownConverter> GetConverters() =>
    [
        new MyFormatConverter(_options),
    ];
}
```

### 3. Update the Extension Method

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyFormatPlugin(
        this IServiceCollection services,
        Action<MyFormatOptions>? configure = null)
    {
        var options = new MyFormatOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IConverterPlugin>(new MyFormatPlugin(options));
        return services;
    }
}
```

### 4. Use with Configuration

```csharp
services.AddMyFormatPlugin(options =>
{
    options.CustomProperty = "value";
    options.MaxSize = 50_000_000;
});
```

## Built-in Plugins

### ElBruno.MarkItDotNet.Excel

Converts Excel spreadsheets (.xlsx) to Markdown tables.

```csharp
services.AddMarkItDotNetExcel();
```

### ElBruno.MarkItDotNet.PowerPoint

Converts PowerPoint slides (.pptx) to Markdown.

```csharp
services.AddMarkItDotNetPowerPoint();
```

### ElBruno.MarkItDotNet.AI

Provides AI-powered converters (OCR, image captioning, audio transcription).

Requires `IChatClient` registration:

```csharp
services.AddOpenAIChatClient("sk-...", "gpt-4-vision");
services.AddMarkItDotNetAI();
```

## Best Practices

1. **Keep converters focused** — each plugin should handle a cohesive set of related formats
2. **Use meaningful names** — plugin names should clearly indicate what they do
3. **Document dependencies** — clearly state any external dependencies (NuGet packages, APIs, etc.)
4. **Support streaming** — for large files, consider implementing `IStreamingMarkdownConverter`
5. **Handle errors gracefully** — use meaningful error messages in `ConversionResult`
6. **Add tests** — include unit tests for your converters
7. **Version appropriately** — follow semantic versioning and clearly document breaking changes

## Plugin Discovery and Registration

The `ConverterRegistry` automatically discovers plugins through:

1. **Explicit registration** — plugins registered as `IConverterPlugin` in DI are loaded on first use
2. **Auto-loading** — when `MarkdownService` queries the registry, it loads all available plugins

This happens without any manual discovery or reflection — plugins are simply instances registered in the DI container.
