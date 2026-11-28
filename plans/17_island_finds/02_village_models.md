---
status: planned
---

# Phase 2 - Village and pier models

## Overview

The models, built in Blender like the ships, before they are placed.
After phase 1, which fixes the strip they stand on (about 100 x 60 m).
Context: [`00_start.md`](00_start.md), depends on [`01_regions.md`](01_regions.md).

## Goals

1. A village builder in `Tools/asset_builders/builders/`: 8 to 12 houses of about 5 x 8 m, a storehouse and a tavern of 10 to 16 m, laid out loosely on about 80 x 60 m with tracks of 3 to 5 m between them; walls, roofs, doors and windows as shapes, the period's timber and plaster with thatch or shingle roofs.
   Villages differ by layout (Q12); the count and footprint as parameters, for a hamlet or a town later.
2. A pier builder: about 50 x 8 m of planking on piles, standing from above the waterline down to the seabed, with bollards along both faces.
3. Few material slots (named slots as `docs/building_assets.md` says), so a village is a few draws; triangles reported by `build.py`, a few thousand for the village and the pier together as a starting target.

## Plan

- References: a few images of 17th and early 18th century Caribbean and Newfoundland settlements and timber piers in `references/`; main proportions only, not measurement.
- Shape pass in the probe, then the Unity capture for the palette, as `docs/building_assets.md` describes.
- One FBX per builder, the village as one or a few objects; reuse the existing palette and textures where they fit.
- The layouts follow `00_start.md` Q12.

## Out of scope

- Placing them in the world, colliders, the map (phase 3).
- A battery, moored ships, people.

## Done when

- Probe renders and a Unity capture of the village and the pier shown to the user, beside a brig for scale.
- Triangles and slots reported.
- Gates green.
