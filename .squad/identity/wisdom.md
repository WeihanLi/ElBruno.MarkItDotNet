---
last_updated: 2026-04-02T17:35:27.513Z
---

# Team Wisdom

Reusable patterns and heuristics learned through work. NOT transcripts — each entry is a distilled, actionable insight.

## Patterns

<!-- Append entries below. Format: **Pattern:** description. **Context:** when it applies. -->
**Pattern:** Use static factory methods on result types (`Succeeded`/`Failure`) instead of throwing exceptions from service classes. Reserve exceptions for the backward-compatible façade layer only. **Context:** Any service that returns an outcome to the caller — keeps control flow explicit and composable.
