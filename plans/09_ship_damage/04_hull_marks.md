---
status: planned
---

# Phase 04 - Marks on the hull

## Overview

Each hit shows where it landed: splintered planks, a dark hole for a big one.
Context: [`00_start.md`](00_start.md) Q2, depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. A mark at each hit, on the hull's surface, sized by the hit.
2. One material and a fixed pool per ship; no allocations after start.

## Plan

- The texture: an atlas of a few splintered patches and holes, alpha cut, made by `Tools/asset_builders/textures.py` like the other textures, `M_HullMark` URP Lit with alpha clip.
- `HullMarks` (Gameplay) on the hull child: a pool of quads made at start; on `ShipDamage.Damaged` (and on restore) it finds the hull's surface at the hit (a ray from outside toward the centreline against the hull's mesh collider or the builder's half-beam), lays a quad there along the normal, a little off the surface, and picks a patch by the hit's size. The oldest is reused when the pool is full.
- Holes below the deck edge read dark; nothing is cut into the mesh.
- Tests (EditMode): a hit places a quad on the hull's side facing out, a bigger hit a bigger patch, the pool reuses the oldest.
- `docs/systems/ship-controller.md`: the marks.

## Out of scope

- Water pouring in, smoke, splinters flying (effects for the cannons' plan).

## Done when

- The tests pass; the gates pass; a capture of a rammed sloop and brig; the draws in view counted with marks.
