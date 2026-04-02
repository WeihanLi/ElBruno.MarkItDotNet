# Building & Testing

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git

## Clone the Repository

```bash
git clone https://github.com/elbruno/ElBruno.MarkItDotNet.git
cd ElBruno.MarkItDotNet
```

## Build

```bash
dotnet build ElBruno.MarkItDotNet.slnx
```

To build for a specific framework (e.g., when .NET 10 SDK is not installed):

```bash
dotnet build -p:TargetFrameworks=net8.0
```

## Run Tests

```bash
dotnet test ElBruno.MarkItDotNet.slnx
```

Or targeting a specific framework:

```bash
dotnet test -p:TargetFrameworks=net8.0
```

The test suite uses [xUnit](https://xunit.net/) with [FluentAssertions](https://fluentassertions.com/) and currently includes 141 tests covering all converters.

## Run the Sample App

```bash
dotnet run --project src/samples/BasicConversion/BasicConversion.csproj
```

## Project Structure

```
ElBruno.MarkItDotNet/
├── src/
│   ├── ElBruno.MarkItDotNet/          # Main library (packable NuGet)
│   │   ├── Converters/                # Built-in converters
│   │   ├── IMarkdownConverter.cs      # Core interface
│   │   ├── MarkdownService.cs         # Main service
│   │   ├── ConversionResult.cs        # Result type
│   │   └── ConverterRegistry.cs       # Converter resolver
│   ├── tests/
│   │   └── ElBruno.MarkItDotNet.Tests/  # xUnit test project
│   └── samples/
│       └── BasicConversion/           # Demo console app
├── docs/                              # Documentation
├── images/                            # Branding assets
├── Directory.Build.props              # Shared MSBuild properties
├── global.json                        # SDK version
└── ElBruno.MarkItDotNet.slnx         # Solution file
```

## CI/CD

- **CI** (`ci.yml`) — builds and tests on every push/PR to `main`
- **Publish** (`publish.yml`) — packs and pushes to NuGet.org on GitHub release (OIDC auth)

## Creating a Release

1. Ensure all tests pass
2. Create a GitHub release with a tag like `v0.1.0`
3. The publish workflow automatically builds, packs, and pushes to NuGet.org
