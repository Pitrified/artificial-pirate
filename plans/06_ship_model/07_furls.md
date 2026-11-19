---
status: done
---

# Phase 07 - Furled sails

Asked 2026-11-19 by the user: "work on the cylinders of the furled up sails. they read like a flat white. now in this feature branch".

## Plan

A furled sail is a bundle of canvas, not a tube:
- **Shape.** Rings along the spar whose radius swells toward the middle and is lumpy where the cloth bunches. The middle sags below the spar under its own weight, and the ends taper to the yardarms.
- **Gaskets.** Dark ties around the bundle every metre or so, in `Tar`.
- **Canvas mapping.** UVs along the length and round the girth, so the canvas texture's seams read as folds wrapping the bundle.

Benchmark the triangles; a screenshot at anchor; the gates and a commit.

## What the implementation found
- 2026-11-19. **First try**: lumps along the bundle, a swell to the middle, a sag, and ties as rings. The ties stood out of the lumps only here and there and read as scattered dark dashes, and the canvas still read white.
- **What holds**:
  - **The ties cinch the bundle.** Rings sit at each tie (every 1.6 m), pinched to 70% of the radius, and between the ties, bulging with three twisting folds round the girth. The ties are thin dark bands just proud of the pinch.
  - **Shape.** The bundle swells to the middle and sags 35% of its radius there. Each furl gets a fixed seed from its position, so no two bunch alike.
  - **Canvas.** The UVs run along the bundle and round it, so the canvas's seams wrap it.
  - **A `FurledCloth` slot**: the same canvas, tinted greyer (0.8, 0.77, 0.7) and less glossy, as weathered, shaded cloth.
- **Cost**: the anchored state goes from 706 to 2,994 triangles on the brig, and from 318 to 1,254 on the sloop. One more slot, shown only at anchor.
- Capture `f4_furl`, before and after, side by side.
- **Gates**: compile 0 warnings, EditMode 146/146, PlayMode 19/19. No new APK: the change is a few thousand triangles at anchor only.
