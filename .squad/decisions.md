# Squad Decisions

## Active Decisions

### Phase 1 Scaffolding Patterns

**Author:** Lambert  
**Date:** 2025-07-18  
**Status:** Implemented  

Project scaffolding follows ElBruno.LocalLLMs patterns exactly:
- `.slnx` XML solution format (not `.sln`)
- `Directory.Build.props` for shared MSBuild/NuGet metadata
- `global.json` SDK 8.0.0 with `rollForward: latestMajor`
- Multi-target `net8.0;net10.0` for both library and test projects
- CI builds use `-p:TargetFrameworks=net8.0` to avoid SDK requirements
- NuGet pack includes README.md and nuget_logo.png from repo root

**Test Stack:** xUnit 2.9.0 + FluentAssertions 8.3.0, coverlet.collector for coverage

**Notes:** Icon at `images/nuget_logo.png` is a placeholder. `/src/samples/` folder exists in .slnx but is empty, ready for future samples.

### Phase 2 Architecture — Extensible Converter Pipeline

**Author:** Dallas (Backend Dev)  
**Date:** 2026-07-22  
**Status:** Implemented

#### Context
Phase 1 had a monolithic `MarkdownConverter` with a switch statement. Phase 2 needed an extensible architecture per the PRD.

#### Decision
- **Interface-first:** `IMarkdownConverter` with `CanHandle` + `ConvertAsync(Stream, string)`. Extension method for file-path convenience overload.
- **Registry pattern:** `ConverterRegistry` holds a `List<IMarkdownConverter>`, resolves first match via `CanHandle`. No dictionary keying — allows converters to handle multiple extensions.
- **Result object:** `ConversionResult` with static factories instead of throwing exceptions from MarkdownService. Façade (`MarkdownConverter`) still throws for backward compat.
- **DI wiring:** `AddMarkItDotNet()` with `Action<MarkItDotNetOptions>` configures everything as singletons.
- **Backward compat:** Kept `MarkdownConverter` as a thin sync façade over `MarkdownService`.

#### Consequences
- Adding a new format converter = implement `IMarkdownConverter` + register it in `ServiceCollectionExtensions` and `MarkdownConverter` constructor.
- Stream-based API means converters never touch the filesystem directly.
- `ConversionResult` makes error handling explicit for consumers.

### Phase 7 CI/CD Workflow Patterns

**Author:** Lambert (DevOps)  
**Date:** 2026-04-02  
**Status:** Implemented

#### Context
Needed CI and publish workflows for ElBruno.MarkItDotNet NuGet library.

#### Decision
Matched ElBruno.LocalLLMs workflow patterns exactly, with only path adaptations:
- CI builds net8.0 only (avoids net10.0 SDK requirement on runners)
- Publish uses OIDC authentication via `NuGet/login@v1` (no long-lived API keys)
- Version sourced from release tag first, then workflow_dispatch input, then csproj fallback
- Only the core library (`src/ElBruno.MarkItDotNet/ElBruno.MarkItDotNet.csproj`) is packed and pushed

#### Requirements
- GitHub environment `release` must be configured in repo settings
- Secret `NUGET_USER` must be set (NuGet account for OIDC)
- NuGet trusted publisher must be configured for OIDC to work

### Copilot Directive: Source Organization & Publishing Rules

**Author:** Bruno Capuano (via Copilot)  
**Date:** 2026-04-02T17:50:00Z  
**Status:** Captured

#### Directive
All code, tests, and samples must live under the `src/` folder. Copilot instructions updated to reflect this rule and include NuGet publishing requirements.

#### Impact
- Enforces consistent project layout across team
- Simplifies CI/CD (no root-level projects to build)
- Aligns with ElBruno.LocalLLMs conventions

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
