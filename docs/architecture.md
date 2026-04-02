# Architecture

## Overview

ElBruno.MarkItDotNet uses a **converter pipeline** pattern where each file format has a dedicated converter that implements a common interface. A registry resolves the correct converter by file extension, and a service orchestrates the conversion workflow.

## Core Components

```
┌─────────────────────────────────────────────────┐
│                 MarkdownService                  │
│         (main entry point / orchestrator)        │
├─────────────────────────────────────────────────┤
│               ConverterRegistry                  │
│       (resolves converter by extension)          │
├──────┬──────┬──────┬──────┬──────┬──────────────┤
│ Text │ JSON │ HTML │ DOCX │ PDF  │    Image     │
│      │      │      │      │      │              │
│ Plain│Format│Rever-│Open- │PdfPig│  Metadata    │
│ read │+high-│seMark│Xml   │      │  extraction  │
│      │light │down  │      │      │              │
└──────┴──────┴──────┴──────┴──────┴──────────────┘
        All implement IMarkdownConverter
```

### IMarkdownConverter

The core contract. Every converter implements two methods:

- `CanHandle(string fileExtension)` — returns `true` if the converter supports the given extension
- `ConvertAsync(Stream fileStream, string fileExtension)` — converts the stream content to Markdown

### ConverterRegistry

A simple resolver that holds all registered converters and finds the right one for a given extension. It iterates through registered converters and returns the first one where `CanHandle()` returns `true`.

### MarkdownService

The top-level service that wraps the registry and provides file I/O convenience methods. It handles:

- Opening files from disk paths
- Resolving the correct converter via the registry
- Wrapping results in `ConversionResult` (success/failure)

### MarkdownConverter (Façade)

A backward-compatible static-like entry point that pre-registers all built-in converters. Use this for quick scripts; use `MarkdownService` + DI for production apps.

### ConversionResult

A result type with `Success`, `Markdown`, `SourceFormat`, and `ErrorMessage`. Always check `Success` before reading `Markdown`.

## Converter Details

| Converter | Extensions | Strategy | NuGet Dependency |
|-----------|-----------|----------|-----------------|
| PlainTextConverter | .txt, .md, .log, .csv | Direct stream read | None |
| JsonConverter | .json | Pretty-print + fenced code block | None |
| HtmlConverter | .html, .htm | HTML → Markdown via ReverseMarkdown | [ReverseMarkdown](https://www.nuget.org/packages/ReverseMarkdown) |
| DocxConverter | .docx | Extract paragraphs via OpenXml SDK | [DocumentFormat.OpenXml](https://www.nuget.org/packages/DocumentFormat.OpenXml) |
| PdfConverter | .pdf | Extract text per page via PdfPig | [PdfPig](https://www.nuget.org/packages/PdfPig) |
| ImageConverter | .png, .jpg, .gif, .bmp, .webp | Metadata placeholder (no OCR) | None |

## Dependency Injection

`services.AddMarkItDotNet()` registers:

- `ConverterRegistry` as singleton (with all built-in converters)
- `MarkdownService` as transient
- `MarkItDotNetOptions` from configuration (optional)

## Extending with Custom Converters

1. Implement `IMarkdownConverter`
2. Register via `ConverterRegistry.Register()` or DI
3. The registry automatically resolves your converter for matching extensions

## Design Decisions

- **Stream-first API** — converters take `Stream`, not file paths, enabling in-memory and cloud scenarios
- **Extension-based routing** — simple, predictable, no content sniffing
- **No OCR built-in** — `ImageConverter` provides metadata only; OCR can be added via custom converter
- **Multi-target net8.0 + net10.0** — supports current LTS and latest .NET
