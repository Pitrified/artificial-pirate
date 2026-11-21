---
status: planned
---

# Phase 02 - Wood

## Overview

Every part the user named is now the flat brown `ShipWood` with no UVs. One change lifts all of them: wood textures, and UVs on the builder's poles and boxes.
Context: [`00_start.md`](00_start.md) "Shared: wood", depends on [`01_frame_times.md`](01_frame_times.md) for the measure.

## Goals

1. Two wood slots: `Spar` (masts, yards, booms, gaffs, the bowsprit, tiller and wheel) with grain running along, and `ShipWood` (bulwark, rail, stanchions, hatches, capstan) as boards with seams.
2. Each a grey texture multiplied by the colour, and a normal map, from `textures.py`.
3. UVs in metres on poles and boxes; smooth sides on round poles, more sides on the masts.

## Plan

- `textures.py`: `wood_grain` (long grain, tiling, for spars) and `wood_boards` (boards about 0.2 m wide with seams and butt joints, from `_planks`), each with a normal map from `normal_map`; registered in `TEXTURES`.
- `mesh.py`:
  - `pole`: u along the axis in metres, v around (the circumference in metres), a seam column of duplicated vertices so it wraps; optional smooth sides, caps flat.
  - `box` / `axis_box`: four vertices a face, UVs from the face's two longest extents in metres; triangles unchanged.
  - Callers that pass a palette `uv` keep it.
- `rig.py`: masts, spars and bowsprit in `Spar`, smooth; `MAST_SIDES` 8 to 12 or 16 (the stump follows it).
- `fittings.py`: helm and tiller in `Spar`; the rest stays `ShipWood`.
- Materials `M_Spar` (new) and `M_ShipWood` (textured), URP Lit, normal map on; the FBX import maps the new slot; the prefabs pick it up.
- Rebuild `Sloop.fbx` and `Brig.fbx`; `BuiltModelTests` updated for the new slot and counts.
- Captures from the five poses in `00_start.md`, before and after, on one sheet.

## Out of scope

- New shapes (phases 03 to 05).
- The hull's own textures.

## Done when

- Gates green; the captures show grain on the masts and boards on the bulwark and hatches.
- Counts logged (triangles, slots with both ships in view).
- The dev APK's GPU time against the baseline, if the user runs it here; otherwise in phase 06.

## Pitfalls

- A box's shared corners carry one UV; without the split per face the texture smears.
- The mast's 8 to 16 sides change the stump and `ShipMasts` nothing, but `BuiltModelTests` may count vertices or objects.
- The normal maps need importing as normal maps (the importer's texture type), set by an editor script.
