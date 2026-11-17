---
status: done
---

# Phase 01 - Copy the baseline

## Overview

The source tree at `8b521a7`, the rows the audit marks `pirate` or `both`, nothing else.
Names are not touched in this phase, so its diff against the source is only the file set; phase 02 renames.

## Plan

- `git archive` of the source commit, then delete what the audit marks `guide`: `plans/`, `scratch/`, `docs/tracking.md`, `docs/ai-development-playbook.md`, `docs/mcp-setup.md`, `docs/dev-environment-setup.md`, `docs/copilot-instructions-template.md`.
- Check the result against the audit with the source's `scratch/audit/coverage.py` logic: every remaining path is `pirate` or `both`, every `pirate` or `both` path is present.
- Keep the source's `Tools/plans.py` as this repo's vendored copy.

## Done when

- The file set matches the audit.
- The project compiles in batch mode with 0 errors and 0 warnings, from a fresh `Library/`.

## What the implementation found

- 2026-11-17. `git archive` of `8b521a7` minus `plans/`, `scratch/` and the five guide-only docs: 262 files, the same set as the audit's `pirate` and `both` rows (checked by a script applying the audit's longest-match rule to the source tree), and byte-identical to the source.
- First batch-mode open from an empty `Library/`: exit 0, with the known one-time Shader Graph `GUID` CS0246 errors from the package cache while it imports.
  Second compile: exit 0, no `error CS` or `warning CS`. Unity wrote nothing outside the ignored `Library/`, `Logs/` and `UserSettings/`.
