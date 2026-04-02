# Lambert — History

## Project Context
- **Project:** ElBruno.MarkItDotNet — .NET library converting file formats to Markdown
- **User:** Bruno Capuano
- **Stack:** .NET 8/10, C#, xUnit, NuGet, GitHub Actions
- **Reference repo:** ElBruno.LocalLLMs — patterns to replicate:
  - Directory.Build.props with shared package metadata
  - .slnx XML-based solution format
  - global.json with sdk 8.0.0, rollForward latestMajor
  - CI workflow: build + test on PR (net8.0 only)
  - Publish workflow: triggered on release, OIDC NuGet auth, version from tag
  - nuget_logo.png in /images/ directory
  - README.md packed into NuGet package

## Learnings

### Phase 1 — Project Scaffolding (completed)
- **Pattern match:** All infrastructure files (Directory.Build.props, global.json, .editorconfig, .slnx) follow ElBruno.LocalLLMs exactly
- **Key paths:**
  - `global.json` — SDK 8.0.0 with rollForward latestMajor
  - `Directory.Build.props` — shared MSBuild props (nullable, analyzers, NuGet metadata)
  - `ElBruno.MarkItDotNet.slnx` — XML solution with /src/, /src/tests/, /src/samples/ folders
  - `src/ElBruno.MarkItDotNet/ElBruno.MarkItDotNet.csproj` — core library (net8.0;net10.0)
  - `src/tests/ElBruno.MarkItDotNet.Tests/` — xUnit + FluentAssertions test project
  - `images/nuget_logo.png` — placeholder NuGet icon (user will replace)
- **NuGet packaging:** README.md + nuget_logo.png packed via ItemGroup with relative `..\..\` paths from src project
- **CI note:** build/test with `-p:TargetFrameworks=net8.0` to avoid net10.0 SDK requirement on CI
- **Test project uses** `TargetFrameworks` (plural) `net8.0;net10.0` per task spec, unlike reference which used single `TargetFramework`
- **FluentAssertions v8.3.0** chosen over NSubstitute (reference used NSubstitute but task specified FluentAssertions)

### Phase 7 — CI/CD & NuGet Publishing Workflows (completed)
- **ci.yml** (`.github/workflows/ci.yml`): Triggers on push/PR to main. Builds and tests with `net8.0` only via `-p:TargetFrameworks=net8.0`. Uploads trx test results as artifact. Exact pattern match with ElBruno.LocalLLMs.
- **publish.yml** (`.github/workflows/publish.yml`): Triggers on release published or manual workflow_dispatch. Uses `release` environment. OIDC auth via `NuGet/login@v1` with `secrets.NUGET_USER`. Version determined from release tag > input > csproj fallback. Packs only the core library project. Pushes with `--skip-duplicate`.
- **Key adaptation:** All paths changed from `ElBruno.LocalLLMs` to `ElBruno.MarkItDotNet` (solution file, csproj paths, test project path)
- **OIDC pattern:** `id-token: write` permission required at job level, not workflow level. Login step outputs `NUGET_API_KEY` for push step.
- **No net10.0 in CI:** Both workflows use `-p:TargetFrameworks=net8.0` to avoid net10.0 SDK requirement on GitHub runners
