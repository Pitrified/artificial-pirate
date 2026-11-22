---
status: planned
---

# Phase 03 - Keeping it current

## Overview

The entry points say what the spec is, every feature updates it (Q4, Q5), and a check in the gates fails on structural drift.
Depends on [`02_spec.md`](02_spec.md).

## Plan

- `.github/copilot-instructions.md`, `AGENTS.md`, `docs/README.md`: the spec as what the game is and its scope; the rule: a plan's start file says what it changes in the spec, the spec changes in the same branch, the last phase's "Done when" names it.
- `Tools/spec_check.py` (standard library): every `docs/systems/*.md` linked from the spec; every `ScreenId` member named in backticks in the spec's screens table; the `"version"` in `save-system.md`'s JSON example equal to `GameSaveData.CurrentVersion` (no other doc states the version). Prints what is missing; exit 1 on any.
- `Tools/gates.sh`: a `spec` gate, run first by `all` before Unity starts, its own PASS or FAIL line.
- `docs/headless-unity.md` §3 gates list: the check named.

## Done when

- The check passes on the tree; gates green with its line.
