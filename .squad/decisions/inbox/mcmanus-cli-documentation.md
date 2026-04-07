# Decision: CLI Documentation & AI Skills Structure

**Date:** 2026-04-07  
**Decided by:** McManus (DevRel)  
**Status:** Approved

## Summary

Created comprehensive documentation and AI skill files for the MarkItDotNet CLI tool to ensure discoverability and usability across terminals, AI agents, and MCP-compatible tools.

## Decision Points

### 1. Documentation Location & Structure

**Decision:** All documentation lives under `/docs/` with hierarchical organization.

- **`docs/cli.md`** — Primary CLI reference (commands, options, exit codes, examples)
- **`docs/skills/markitdown-cli.md`** — AI/Copilot skill (library API + common patterns)
- **`docs/skills/markitdown-mcp.md`** — MCP integration (tool definitions, setup guides)
- **`.squad/skills/markitdown-cli/SKILL.md`** — Squad knowledge (build/test/run patterns)

**Rationale:** Mirrors the project's architecture (library core → satellite packages → tooling). Allows developers to find info by use case (CLI users → docs/cli.md; AI agents → docs/skills/).

### 2. Examples-First Documentation

**Decision:** Lead with real-world examples in every section.

- Basic conversions (1 file, batch, URL)
- Pipeline integration (piping, JSON output for scripting)
- Performance tuning (--streaming, --parallel)
- AI/RAG use cases (documented in skills files)

**Rationale:** If people can't see it in action, they can't use it. Examples are more searchable than prose.

### 3. Exit Codes as First-Class Documentation

**Decision:** Exit codes table (0, 1, 2, 3) appears in CLI docs and Squad skill.

**Rationale:** Critical for CI/CD integration and error handling in scripts. Must be discoverable and consistent.

### 4. MCP Tool Definitions are JSON Schemas

**Decision:** MCP tools defined with full JSON schema examples in `docs/skills/markitdown-mcp.md`.

**Rationale:** Enables developers to copy-paste into their MCP server implementations. Input/output examples are required for clear integration.

### 5. CLI Feeds NuGet Packages Table

**Decision:** Added `ElBruno.MarkItDotNet.Cli` row to README NuGet packages table.

**Rationale:** Users check that table first to understand what packages exist. CLI is a deliverable like any NuGet package.

## Implications

- **Developers building AI agents** will reference `docs/skills/markitdown-cli.md` for library patterns
- **DevOps engineers** will reference `docs/cli.md` for batch processing and exit codes
- **AI/ML engineers** will reference `docs/skills/markitdown-mcp.md` for MCP integration
- **Squad agents (future)** will reference `.squad/skills/markitdown-cli/SKILL.md` for build/test commands
- **README** now includes CLI section + tool in packages table (discoverability)

## Next Steps

1. Implement CLI tool at `src/ElBruno.MarkItDotNet.Cli/` (follow Squad skill pattern)
2. Create test fixtures in `src/tests/ElBruno.MarkItDotNet.Cli.Tests/test-files/`
3. Validate all exit codes and options match documentation
4. Create sample MCP server (Node.js) in `docs/samples/markitdown-mcp-server.js`
5. Test CLI with real documents (PDF, DOCX, XLSX, etc.)
