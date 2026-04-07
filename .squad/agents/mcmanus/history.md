# Project Context

- **Owner:** Bruno Capuano
- **Project:** ElBruno.MarkItDotNet — .NET library converting 15+ file formats to Markdown. Building a CLI tool (`markitdown`) and AI skills.
- **Stack:** C#, .NET 8/10, System.CommandLine, NuGet, xUnit, FluentAssertions
- **Created:** 2026-04-07

## Learnings

- CLI tool uses System.CommandLine for argument parsing; supports single file, batch, URL, and formats listing commands
- All CLI options map to MarkdownService methods: --streaming for large files, --format for markdown/json output, --parallel for batch performance
- Exit codes: 0 (success), 1 (conversion error), 2 (file not found), 3 (unsupported format) — critical for CI/CD integration
- Documentation lives in `/docs/` with `cli.md` as main reference; skills in `/docs/skills/` for AI training
- MCP integration enables headless conversion for AI agents; tool definitions map CLI commands to struct I/O schemas
- Squad skill encapsulates build/test/run commands for CLI; patterns include streaming, batch parallel, plugin auto-discovery
