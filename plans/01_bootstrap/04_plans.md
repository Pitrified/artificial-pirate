---
status: planned
---

# Phase 04 - Move the open roadmap

## Overview

The source's `docs/tracking.md` phases 9 to 13 are the game's open work, written as a checklist.
They become plan folders here, as [`00_start.md`](00_start.md) maps them.

## Plan

- `02_mcp_smoke`, `03_performance`, `04_release`, each with `00_start.md` and `tracking.md`, status `draft`.
- Each `00_start.md` opens with a move note naming its source phase and linking the source's `docs/tracking.md`.
- Checklist items brought up to date as they move: phase 9's server choice, pin and client config are done at the source commit (CoplayDev `unity-mcp`, Claude Code over HTTP), so only the smoke test is left; the `~/.config` path is the renamed one.
- The phase 8 item still open in the source, logcat clean on a device, goes into this folder's [`05_verify.md`](05_verify.md) device run instead.

## Done when

- `python3 Tools/plans.py check` passes.
- Every unticked item of the source's phases 9 to 13 is in one of the folders.

## What the implementation found
