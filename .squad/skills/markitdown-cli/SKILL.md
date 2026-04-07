# markitdown-cli

**Confidence:** low
**Domain:** CLI tooling, file conversion
**Created:** 2026-04-07

## Pattern

The markitdown CLI wraps MarkdownService for terminal use. Built with System.CommandLine for robust argument parsing and streaming support.

### Tool Command

```bash
markitdown <file|batch|url|formats> [options]
```

### Installation

```bash
# Global tool
dotnet tool install -g ElBruno.MarkItDotNet.Cli

# Local tool
dotnet tool install ElBruno.MarkItDotNet.Cli
```

### Build

```bash
dotnet build src/ElBruno.MarkItDotNet.Cli -p:TargetFrameworks=net8.0;net10.0 --nologo
```

### Test

```bash
dotnet test src/tests/ElBruno.MarkItDotNet.Cli.Tests -p:TargetFrameworks=net8.0;net10.0 --nologo
```

### Run Locally

```bash
dotnet run --project src/ElBruno.MarkItDotNet.Cli --framework net8.0 -- <args>
```

## Key Commands

### Single File Conversion

```bash
# Convert to stdout
markitdown report.pdf

# Save to file
markitdown report.pdf -o report.md

# Stream large files
markitdown huge.pdf --streaming -o huge.md

# JSON output with metadata
markitdown data.csv --format json | jq .metadata
```

### Batch Conversion

```bash
# All files in directory
markitdown batch ./documents -o ./output

# Recursive with subdirectories
markitdown batch ./docs -o ./output -r

# Filter by pattern
markitdown batch ./mixed -o ./converted -r --pattern "*.pdf"

# Multiple extensions
markitdown batch ./mixed -o ./converted -r --pattern "*.{docx,pdf}"

# Control parallelism (1-4 slower, multi-core faster)
markitdown batch ./docs -o ./output -r --parallel 2
```

### URL Conversion

```bash
# Print to terminal
markitdown url https://example.com

# Save to file
markitdown url https://example.com -o page.md

# JSON with metadata
markitdown url https://example.com --format json
```

### List Formats

```bash
# Show all converters
markitdown formats

# Find specific format
markitdown formats | grep pdf
```

## Exit Codes

| Code | Status | Meaning |
|------|--------|---------|
| `0` | ✅ | Success — file(s) converted |
| `1` | ❌ | Conversion Error — file corrupted or format error |
| `2` | ❌ | File Not Found — input doesn't exist |
| `3` | ❌ | Unsupported Format — no converter registered |

## Options Summary

### Global Options

- `-q, --quiet` — Suppress progress/debug output
- `-v, --verbose` — Show detailed conversion logs
- `--version` — Show version
- `--help` — Show help

### File Conversion Options

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| `-o, --output` | path | stdout | Output file path |
| `--format` | markdown\|json | markdown | Output format |
| `--streaming` | bool | false | Stream large files |

### Batch Conversion Options

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| `-o, --output` | path | **required** | Output directory |
| `-r, --recursive` | bool | false | Include subdirectories |
| `--pattern` | glob | `*.*` | File glob filter |
| `--parallel` | int | CPU cores | Parallelism level |
| `--format` | markdown\|json | markdown | Output format |

### URL Conversion Options

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| `-o, --output` | path | stdout | Output file path |
| `--format` | markdown\|json | markdown | Output format |

## Architecture

```
MarkItDotNet.Cli/
├── Program.cs                # Entry point, command registration
├── Commands/
│   ├── ConvertCommand.cs     # markitdown <file>
│   ├── BatchCommand.cs       # markitdown batch <dir>
│   ├── UrlCommand.cs         # markitdown url <url>
│   └── FormatsCommand.cs     # markitdown formats
├── Services/
│   ├── CliService.cs         # Wrapper around MarkdownService
│   └── OutputFormatter.cs    # Markdown/JSON output formatting
└── Options/
    ├── GlobalOptions.cs      # Shared options (-q, -v)
    └── CommandOptions.cs     # Command-specific options
```

## Integration Points

- **MarkdownService** — Core conversion logic (from ElBruno.MarkItDotNet)
- **System.CommandLine** — Argument parsing, help generation
- **DI Container** — Dependency injection for plugin system
- **Converter Plugins** — Excel, PowerPoint, AI, Whisper auto-discovery

## Testing Strategy

- Unit tests for command parsing and option validation
- Integration tests for actual file conversions
- E2E tests for batch operations and streaming
- Fixture files in `test-files/` with various formats

## Common Issues

| Issue | Solution |
|-------|----------|
| File not found | Check absolute path or use `$(pwd)/file.pdf` |
| Unsupported format | Run `markitdown formats` to see supported extensions |
| Out of memory on batch | Use `--parallel 1` or `--streaming` |
| Slow on large directory | Increase `--parallel` on multi-core machines |

## Performance Benchmarks

| Operation | Time | Notes |
|-----------|------|-------|
| Convert 1 MB PDF | ~50ms | Page-based extraction |
| Batch 100 files (1 MB each) | ~5s | 8 parallel threads |
| Convert URL | ~800ms | Fetch + render + extraction |
| CSV → table (1000 rows) | ~10ms | Simple parse |

## Dependencies

| Package | Purpose |
|---------|---------|
| `ElBruno.MarkItDotNet` | Core conversion service |
| `System.CommandLine` | CLI argument parsing |
| `Microsoft.Extensions.DependencyInjection` | Plugin registration |
| `Spectre.Console` | Progress bars, colored output |

## Future Enhancements

- [ ] Progress bar for batch operations
- [ ] Filtering by file size
- [ ] Skip existing output files
- [ ] API server mode (HTTP interface)
- [ ] Watch mode (auto-convert on file change)
- [ ] Output to databases (MongoDB, PostgreSQL)
- [ ] Compression (gzip output)
- [ ] Webhook notifications for batch completion
