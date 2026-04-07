# Project Context

- **Owner:** Bruno Capuano
- **Project:** ElBruno.MarkItDotNet — .NET library converting 15+ file formats to Markdown. Building a CLI tool (`markitdown`) and AI skills.
- **Stack:** C#, .NET 8/10, System.CommandLine, NuGet, xUnit, FluentAssertions
- **Created:** 2026-04-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- **CLI test project created**: `src/tests/ElBruno.MarkItDotNet.Cli.Tests/` with 28 passing tests across 5 test classes (FormatsCommand, ConvertCommand, BatchCommand, UrlCommand, OutputFormatter).
- **CLI tests use process-based integration testing**: Tests invoke `dotnet run --project ...` via `CliRunner` helper, capturing stdout/stderr/exit codes. This tests the full CLI pipeline including argument parsing.
- **CLI exit codes**: 0=success, 2=file not found, 3=unsupported format, 1=general error.
- **UrlConverter handles invalid URLs gracefully**: The UrlConverter returns success (exit 0) for non-fetchable URLs like "not-a-valid-url" rather than failing.
- **System.CommandLine beta4 requires InvocationContext lambdas**: `SetHandler` with method groups doesn't support `CancellationToken` binding; use `async (InvocationContext ctx) => { ... }` pattern instead.
