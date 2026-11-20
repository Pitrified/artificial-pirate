---
status: done
---

# Phase 04 - Marks on the hull

## Overview

Each hit shows where it landed: splintered planks, a dark hole for a big one.
Context: [`00_start.md`](00_start.md) Q2, depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. A mark at each hit, on the hull's surface, sized by the hit.
2. One material and a fixed pool per ship; no allocations after start.

## Plan

- The texture: an atlas of two splintered patches and two holes, alpha cut, made by `Tools/asset_builders/textures.py` (`hull_marks`) like the other textures; `M_HullMark` URP Lit with alpha clip, made with its texture's import settings by a throwaway editor script.
- Where a mark lands: above the water the builder's topsides are vertical, at `half_beam(t)` along the length (`builders/ship/hull.py`). `SO_ShipType` gains that profile as a curve of the half-beam along the length (sampled from the builder at its 21 stations, set on the sloop and brig types by the throwaway script), and a mark sits on it, facing out along the side's normal, a little off the surface, kept between the waterline and the rail.
- `HullMarks` (Gameplay) on a `Marks` child of the `Hull`: one mesh of `ShipDamage.KeptHits` quads (one draw per ship, arrays made at start), rebuilt from `ShipDamage`'s kept hits on `Damaged` and on a new `Restored` event; a bigger hit a bigger patch, a hole above a size. The oldest goes when the hits do.
- Holes read dark; nothing is cut into the mesh.
- Tests (EditMode): a hit places a quad on the hull's side facing out, a bigger hit a bigger patch, the marks follow the kept hits (oldest dropped), a restore draws the saved ones.
- `docs/systems/ship-controller.md`: the marks; `docs/building_assets.md` or the builder's notes: the profile on the type comes from `half_beam`.

## Out of scope

- Water pouring in, smoke, splinters flying (effects for the cannons' plan).

## Done when

- The tests pass; the gates pass; a capture of a rammed sloop and brig; the draws in view counted with marks.

## What the implementation found

- The first marks (0.8 to 2.4 m) read as a smudge from the chase camera; 1.2 to 3.5 m reads as a hole. The prefab had saved the first sizes, so they were set again on it.
- The renderer is off while there are no hits: a whole ship costs no draw, a damaged one one more (the draws in view rise by at most two, one a ship).
- The ram's mark lands at the rammer's bow, against the other hull, so it is mostly hidden while the ships touch.
- Capture: the sloop after ramming the brig, a 15% hole amidships on starboard at the waterline.
