# Project Context

- **Owner:** Bruno Capuano
- **Project:** ElBruno.MarkItDotNet — .NET library converting 15+ file formats to Markdown. Building a CLI tool (`markitdown`) and AI skills.
- **Stack:** C#, .NET 8/10, System.CommandLine, NuGet, xUnit, FluentAssertions
- **Created:** 2026-04-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- **System.CommandLine beta4 handler pattern:** `SetHandler` with `InvocationContext` is needed for exit code control; the typed `SetHandler` overloads don't support `Task<int>` returns. Use `ctx.ExitCode` and `ctx.ParseResult.GetValueForOption()`.
- **DI needs `Microsoft.Extensions.DependencyInjection` (not just Abstractions):** The core library only pulls in Abstractions; the CLI must explicitly reference the full DI package for `BuildServiceProvider()`.
- **`Path` is a static class in .NET:** Cannot be used as a generic type parameter or extended. Use `Path.GetRelativePath()` directly.
- **Converter discovery for `formats` command:** `ConverterRegistry.GetAll()` returns `IReadOnlyList<IMarkdownConverter>`, and `CanHandle(extension)` checks support per converter.
