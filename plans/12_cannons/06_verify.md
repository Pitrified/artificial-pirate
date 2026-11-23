---
status: done
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

- **Q11, the user's answer: the camera further out.** The sloop's chase offset from (0, 12, -42) to (0, 18, -56), the brig's in proportion from (0, 16, -60) to (0, 24, -80); the look points unchanged. In a capture of the fight from the game's camera the brig and its smoke are now in the frame beside the sloop. Gates green (EditMode 255, PlayMode 27).
- The brig's harm stands (the user: a ship twice the size should hurt; exploring takes care).

- **The phone, 2026-11-23** (Pixel 7 Pro, `f95ae39`, 144 s, two fights won, both brigs sunk): 60 fps held (frame p50 and p95 16.8 ms). GPU p50 11.8 ms, p95 12.8, mean 11.7 over the session; the last 60 s, fighting the sailing brig, p95 12.3, mean 11.6. Against `0d14399` (p50 11.8, p95 12.3, mean 11.2): the median unchanged, the mean and the session's p95 about 0.5 ms up, inside the 16.7 ms frame. GC p95 0.1 kB a frame, no problems logged. The aimed barrels stay.
- Six frames over 33 ms: the load (350 and 183 ms), the pause at the end (133 ms), and four single 33 ms frames at 10.7, 23.9, 61.2 and 99.5 s; the first is just before the first volley's hits, the others line up with nothing logged. Not followed: one each in a minute.
- The anchored brig, sunk at 44 s, did not come back: the player fought the second brig within 300 m of its anchorage, as the respawn intends.
