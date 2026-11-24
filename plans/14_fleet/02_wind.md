---
status: planned
---

# Phase 02 - The wind

## Overview

The sails are baked for one wind the game does not have. A wind per world, and each mast's sailing state baked for bands of the wind's angle off the bow, one shown by the heading (Q1 a, Q2 a). Before the new types, so they are built with it.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. One wind per world, its bearing from the world's seed, steady; no save change.
2. Each mast's sailing state baked for each band, with its running rigging and the clearance check at 0 for each.
3. The flag follows the wind.
4. In the game each ship shows, per mast, the band its heading puts the wind in, with hysteresis at the edges; a broken mast takes all its bands.

## Plan

- **Builder**: `sails.WIND_FROM_DEG` gives way to a list of bands (five to seven, both tacks, from close-hauled to running; the count chosen by the triangles in memory and the look). Each band is `<Name>Mast<i>Sailing<b>`; `Anchored` and `Stump` unchanged. `clearance` per band, and the boom brought in per band until no rope crosses a gaff sail, as `build` does now for the one trim. The band's angles are printed by the report, for the type.
- **The flag**: its own object already, built with its origin at the masthead (today the model's origin), turned about it to stream downwind in the game, its waving material unchanged.
- **Game**: the wind's bearing kept by the world (from its seed, as the islands are); a component on `PF_Ship` showing each standing mast's band from the ship's heading against the wind, its band edges and hysteresis on a tuning asset; `MastParts` holds every band so a broken mast takes them all.
- **Scenes and prefabs** by a throwaway editor script: each variant's masts with their bands, the sails-set choice kept for anchored ships.
- **Tests**: the band for a heading against a wind both ways round, the hysteresis, a broken mast's bands gone, the wind from the seed the same each time; `BuiltModelTests` a band object per mast per band.
- **Capture**: the circling brig at points round its circle; the sloop from the chase while turning.

## Out of scope

- The wind on the speed (Q2: a note for the spec's Next).
- The smoke drifting with the wind.

## Done when

- Tests pass; gates green; clearance 0 for every band; the captures read; the user judges the jumps on the phone.

## What the implementation found
