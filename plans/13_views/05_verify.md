---
status: done
---

# Phase 05 - Verify

## Overview

The docs and the phone.
Context: [`00_start.md`](00_start.md).

## Goals

1. The spec: Screens (Settings, landed in phase 01), Controls (the camera button), Scope (the cameras, the map icons, the settings; a choice of cameras leaves Next), a Locked decisions row for settings in `PlayerPrefs`.
2. The system pages: `camera-controller.md` (the views), `ui-system.md` (the Settings screen, the camera button), `world.md` (the maps' icons), `game-manager.md` (`GameSettings`), `ship-controller.md` (the rigs on the type, both player ships), `building_assets.md` or `combat.md` where they change.
3. The phone: each camera in a fight, both ships, the maps, the icon; the diagnostics report against the cannons run.

## Plan

- Write the docs; spec check, links and citations clean.
- A dev APK; the user's run and read.

## Done when

- Docs in the branch; gates green; the user's read and report.

## What the implementation found

- The docs needed no correction from the phone read beyond the sinking view (phase 06) and the helm's milder sea in Next.
- The phone read: the icon, the three cameras, both ships and both maps work. The helm is hard to watch in both motions in today's waves (kept, Q9 a). Sinking in the helm view broke the view, fixed in [`06_sinking_view.md`](06_sinking_view.md).
- Diagnostics at `bdb7efc`: GPU p50 10.3 ms, p95 13.3 (the cannons run 11.8, 12.8); at `cd43e2e`, after two new games, p50 11.8, p95 13.3, memory flat at about 154 MB.
- The report shows an fps maximum of 56606 from a frame measured at 0.0 ms, a display quirk left for the user to schedule.
