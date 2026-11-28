---
status: planned
---

# Phase 3 - Placement and the map

## Overview

The village and pier in the world, something ships can hit, and a mark on the map once seen.
Context: [`00_start.md`](00_start.md), depends on [`01_regions.md`](01_regions.md) and [`02_village_models.md`](02_village_models.md).

## Goals

1. `WorldStreamer` builds a settled island's village on its strip and its pier out from the shore's root, with the island, pooled the same way.
2. The pier has a collider the ships stop against; the village stands on the island, which the ships already cannot sail onto.
3. A fixed village near the start: a settled island just outside the start clearing, in every world, placed so the anchored brig and giant and the ship sailing its circle keep clear of it.
4. A village icon on the minimap and the full map once its cell is explored (`ExploredCells`), drawn like the ships' icons from the island record, not from scene objects.

## Plan

- At the review: read how ships meet colliders today (`HullContact`, the ship collision code) and decide whether the pier is a collider the hull stops at or ground in `HeightAt`; the second keeps one height function but makes the pier a shallow.
- The fixed village's place is drawn from the seed's wind (its strip on the lee side) at a fixed distance from the origin; checked against the anchored ships' positions and the circling ship's path.
- Tests (PlayMode): the fixed village built near the start; the pier stops a ship sailed into it; the icon on the minimap once its cell is explored, not before.
- Captures: the fixed village from the helm and the chase camera, a brig alongside the pier for scale.

## Out of scope

- Using the dock (in the spec's Next list).
- Moored ships at the pier.

## Done when

- Tests and gates green.
- The captures shown to the user.
- Draws and triangles with a village in view measured in the editor.
