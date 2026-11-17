---
status: done
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

- 2026-11-17. `02_mcp_smoke`, `03_performance`, `04_release`, each a `00_start.md` with a move note linking its source phase and a `tracking.md` with the move in the log; status `draft`. No phase files yet: each folder's first step is a human session that decides them.
- All 24 unticked items of the source's phases 9 to 13 are in a folder (5, 5, 4, 8, 2); phase 11's "store the keystore" is merged into the generate step.
  Phase 9's server choice and pin were done; its `.vscode/mcp.json` and Copilot Chat items became Claude Code's `claude mcp add` and `/mcp`.
- The performance folder needed no device question: the spec and the budget name a Pixel 7.
- `plans.py check` passes on 4 folders; link check 0 broken; the anchors used (`#9-release-checklist`, `#4-app-creation-one-time`, `#5-required-pre-launch-forms-even-for-internal-track`, `#12-definition-of-demo-ready`) match their headings.
