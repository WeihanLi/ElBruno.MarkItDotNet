# ElBruno.MarkItDotNet

[![NuGet](https://img.shields.io/nuget/v/ElBruno.MarkItDotNet.svg)](https://www.nuget.org/packages/ElBruno.MarkItDotNet)
[![Build](https://github.com/elbruno/ElBruno.MarkItDotNet/actions/workflows/ci.yml/badge.svg)](https://github.com/elbruno/ElBruno.MarkItDotNet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**.NET library that converts file formats to Markdown** for AI pipelines, documentation workflows, and developer tools. Inspired by Python [markitdown](https://github.com/microsoft/markitdown).

## Description

ElBruno.MarkItDotNet provides a unified interface to convert multiple file formats (plain text, JSON, HTML, Word documents, PDFs, and images) into clean, structured Markdown. Designed for AI content pipelines, documentation systems, and any scenario where you need consistent Markdown output from mixed file sources.

## Installation

```bash
dotnet add package ElBruno.MarkItDotNet
```

## Supported Formats

| Format | Extensions | Converter | Dependencies |
|--------|-----------|-----------|---|
| Plain Text | `.txt`, `.md`, `.log` | `PlainTextConverter` | None |
| JSON | `.json` | `JsonConverter` | None |
| HTML | `.html`, `.htm` | `HtmlConverter` | `ReverseMarkdown` |
| Word (DOCX) | `.docx` | `DocxConverter` | `DocumentFormat.OpenXml` |
| PDF | `.pdf` | `PdfConverter` | `UglyToad.PdfPig` |
| Images | `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.svg` | `ImageConverter` | None |

## Target Frameworks

- .NET 8.0 (LTS)
- .NET 10.0

## Quick Start

The simplest way to get started is with the `MarkdownConverter` façade:

```csharp
using ElBruno.MarkItDotNet;

// Convert a file to Markdown
var converter = new MarkdownConverter();
var markdown = converter.ConvertToMarkdown("document.txt");
Console.WriteLine(markdown);

// Or convert from a stream
using var stream = File.OpenRead("document.pdf");
var result = await converter.ConvertAsync(stream, ".pdf");
Console.WriteLine(result.Markdown);
```

The `MarkdownConverter` class pre-registers all built-in converters and provides synchronous and asynchronous conversion methods.

## Dependency Injection

For more advanced scenarios (e.g., ASP.NET Core applications), use the DI extension method to register MarkItDotNet services:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ElBruno.MarkItDotNet;

var services = new ServiceCollection();

// Register MarkItDotNet with built-in converters
services.AddMarkItDotNet();

var provider = services.BuildServiceProvider();
var markdownService = provider.GetRequiredService<MarkdownService>();

// Convert files through the service
var result = await markdownService.ConvertAsync("document.html");
if (result.Success)
{
    Console.WriteLine(result.Markdown);
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

## API Reference

### MarkdownService

The main service for converting files to Markdown. Use this in DI scenarios or when you need advanced control over converters.

```csharp
public class MarkdownService
{
    public MarkdownService(ConverterRegistry registry);
    
    // Convert a file at the given path
    public Task<ConversionResult> ConvertAsync(string filePath);
    
    // Convert from a stream with explicit file extension
    public Task<ConversionResult> ConvertAsync(Stream stream, string fileExtension);
}
```

### ConversionResult

Represents the outcome of a file conversion. Always check `Success` before accessing `Markdown`.

```csharp
public class ConversionResult
{
    public string Markdown { get; }          // Converted content (empty if failed)
    public string SourceFormat { get; }      // Source format (e.g., ".pdf")
    public bool Success { get; }             // Whether conversion succeeded
    public string? ErrorMessage { get; }     // Error details if Success is false
}
```

### IMarkdownConverter

Contract for implementing custom converters.

```csharp
public interface IMarkdownConverter
{
    // Check if this converter handles the given file extension
    bool CanHandle(string fileExtension);
    
    // Perform the conversion (extension includes the leading dot)
    Task<string> ConvertAsync(Stream fileStream, string fileExtension);
}
```

### ConverterRegistry

Manages and resolves converters by file extension.

```csharp
public class ConverterRegistry
{
    public void Register(IMarkdownConverter converter);
    public IMarkdownConverter? Resolve(string extension);
    public IReadOnlyList<IMarkdownConverter> GetAll();
}
```

## Custom Converters

You can implement custom converters for unsupported file formats. Here's an example custom converter for `.csv` files:

```csharp
using ElBruno.MarkItDotNet;
using System.Text;

public class CsvConverter : IMarkdownConverter
{
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension)
    {
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        var csv = await reader.ReadToEndAsync();
        
        var lines = csv.Split('\n');
        if (lines.Length == 0) return string.Empty;
        
        var sb = new StringBuilder();
        
        // Header row
        var headers = lines[0].Split(',');
        sb.Append("| ");
        sb.Append(string.Join(" | ", headers));
        sb.AppendLine(" |");
        sb.Append("|");
        sb.Append(string.Concat(headers.Select(_ => " --- |")));
        sb.AppendLine();
        
        // Data rows
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = lines[i].Split(',');
            sb.Append("| ");
            sb.Append(string.Join(" | ", cells));
            sb.AppendLine(" |");
        }
        
        return sb.ToString();
    }
}
```

Then register your custom converter:

```csharp
var registry = new ConverterRegistry();
registry.Register(new CsvConverter());
// ... register other converters ...
var service = new MarkdownService(registry);
```

Or with DI:

```csharp
services.AddMarkItDotNet();
var registry = provider.GetRequiredService<ConverterRegistry>();
registry.Register(new CsvConverter());
```

## Samples

| Sample | Description |
|--------|-------------|
| [BasicConversion](src/samples/BasicConversion) | Console app demonstrating text, JSON, and HTML conversion with DI |

## Documentation

- [Architecture](docs/architecture.md) — design decisions, plugin system, converter pipeline, and internal structure
- [Plugins Guide](docs/plugins.md) — how to create custom plugin packages
- [Building & Testing](docs/building-and-testing.md) — how to build from source and run tests
- [Image Generation Prompts](docs/image-generation-prompts.md) — AI prompts for branding assets

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 👋 About the Author

**Made with ❤️ by [Bruno Capuano (ElBruno)](https://github.com/elbruno)**

- 📝 **Blog**: [elbruno.com](https://elbruno.com)
- 📺 **YouTube**: [youtube.com/elbruno](https://youtube.com/elbruno)
- 🔗 **LinkedIn**: [linkedin.com/in/elbruno](https://linkedin.com/in/elbruno)
- 𝕏 **Twitter**: [twitter.com/elbruno](https://twitter.com/elbruno)
- 🎙️ **Podcast**: [notienenombre.com](https://notienenombre.com)
