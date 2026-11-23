---
status: in progress
---

# Phase 06 - Verify

## Overview

The docs and the device: what the game now is, written down, and the phone's numbers in a fight.
Context: [`00_start.md`](00_start.md).

## Goals

1. The spec: Scope (combat, ships under sail in a circle, respawn moved to In the game; sound and chain shot into Next), the HUD line (the ship bars), the Systems index (`combat.md`).
2. `docs/systems/combat.md`: guns, balls, sides, effects, the helm, respawn, bars, shake; `ship-controller.md` and `building_assets.md` updated where they change.
3. The phone: a dev APK, a fight, the diagnostics report against the baseline before the plan; it decides whether the aimed barrels (phase 05, 30 more objects) stay.

## Plan

- Write the docs; `spec_check.py` passes with `combat.md`; links resolve; `plans.py check --citations` clean.
- A baseline report on `main` before the merge of this branch is the user's run (the build before this plan, `a0e18d4`), then the same on this branch in a fight.

## Out of scope

- Tuning beyond what the run shows as broken.

## Done when

- Docs merged in the branch; gates green; the user's read of a fight on the phone and the report's gpu and cpu p95 against the baseline.

## What the implementation found
