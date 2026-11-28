---
status: planned
---

# Phase 3 - Placement and the map

## Overview

The village and pier in the world, something ships can hit, and a mark on the map once seen.
Context: [`00_start.md`](00_start.md), depends on [`01_regions.md`](01_regions.md) and [`02_village_models.md`](02_village_models.md).

## Goals

1. `WorldStreamer` builds a settled island's village on its strip and its pier out from the shore's root, with the island, pooled the same way.
2. Ships stop against the pier, by the means `00_start.md` Q11 settles; the village stands on the island, which the ships already cannot sail onto.
3. The village on the fixed settled island near the start (made in phase 1).
4. A village icon on the minimap and the full map once its cell is explored (`ExploredCells`), drawn from the island records, not from scene objects: the minimap finds ships on the ships' layer, so the villages are a second source of `MapMarker`s (heading 0).

## Plan

- Ships meet each other through `HullContact`'s overlap query on the ships' layer (kinematic bodies do not collide in PhysX) and the ground through `HeightAt`; the pier follows Q11.
- Tests (PlayMode): the fixed village built near the start; the pier stops a ship sailed into it; the icon on the minimap once its cell is explored, not before.
- Captures: the fixed village from the helm and the chase camera, a brig alongside the pier for scale.

## Out of scope

- Using the dock (in the spec's Next list).
- Moored ships at the pier.

## Done when

- Tests and gates green.
- The captures shown to the user.
- Draws and triangles with a village in view measured in the editor.
