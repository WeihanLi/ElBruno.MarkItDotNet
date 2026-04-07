# Decision: CLI Exit Code Contract

**Author:** Fenster (CLI Developer)
**Date:** 2025-07-17

## Context

The `markitdown` CLI tool needs consistent, documented exit codes so scripts and CI pipelines can act on results.

## Decision

Exit codes follow this contract:
- **0** — Success
- **1** — Conversion error (runtime failure, unsupported content)
- **2** — File/directory not found
- **3** — Unsupported format (no converter registered for the extension)

This is enforced in all command handlers (`ConvertCommand`, `BatchCommand`, `UrlCommand`).

## Rationale

Pipe-friendly tools need a reliable exit code API. Distinct codes let callers differentiate between "bad input" and "tool failure" without parsing stderr.
