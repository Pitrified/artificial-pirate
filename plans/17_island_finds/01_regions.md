---
status: planned
---

# Phase 1 - Regions and the settled island

## Overview

The world's shape before anything stands on it: clusters and open water, and one island per settled archipelago large enough for a village.
First, because the village's size depends on the island it gets, and the scale is judged on the map before any model exists.
Context: [`00_start.md`](00_start.md).

## Goals

1. Regions: a coarser grid over the cells, each region's kind (open sea, archipelago, settled archipelago) drawn from a new `WorldHash` stream of the seed and the region's coordinates, so a cell's island stays local to the seed and streaming is unchanged.
2. The region sets its cells' island chance and size ranges; open sea has few or no islands.
3. A settled region has exactly one settled island, in a cell drawn from the region: radius about 120 to 180 m, round enough to hold it, with a low flat strip of about 100 x 60 m at 1 to 3 m on its lee side (from `WindFromDeg`), its shore there straight enough for a 50 m pier.
   Its draws are a new stream; the island record (`IslandSite`) says it is settled and where the strip and the pier's rectangle are, for phase 3.
4. The gaps measured over seeds, against the targets: 150 to 300 m of open water inside an archipelago, 0.5 to 1 km between, a settled island every 600 to 1300 m of sailing.
5. The start clearing (340 m) and the start island unchanged.
6. The fixed settled island near the start: in every world, just outside the start clearing, its strip on the lee side, clear of the anchored ships and the circling brig.
   It is world generation, so it is here; phase 3 puts the village on it.

## Plan

- Read `WorldGenerator`, `WorldHash`, `IslandSite` and `SO_WorldSettings` first; the region kinds' settings go in `SO_WorldSettings` (no magic numbers).
- Keep the 400 m cells.
  A round island's reach from its centre is about 1.9 radii plus 33 m (`WorldGenerator.Site`: the coast wobble 0.3, the warp 0.6, the shelf), so at 400 m a settled island of up to about 190 m fits the "no further than the next cell" rule; a 300 m cell would cap it at about 140 m.
  With a chance near 1 and today's radii, an archipelago's shore-to-shore gaps come to roughly 150 to 250 m (an estimate), inside the target; a smaller cell only if the measurement says otherwise, and then the settled island shrinks with it and `WorldStreamer._loadRadius` grows.
- The settled island is round with little warp and stretch, so its strip stays flat and its shore straight; the strip is a flattening term in `IslandHeight` for that site.
- `WorldGeneratorTests.IslandChance_IsTheShareOfCellsWithAnIsland` and the radius-under-`Radius.y` assertion change by design: rewritten per region kind, not loosened.
- A scratch measurement in EditMode over seeds 1 to 5: islands per region kind, nearest shore-to-shore gaps, open stretches, settled islands per km2. Numbers in the log.
- Map captures of a few seeds (the full map with everything explored, or a scratch render of `HeightAt`) for the user's read of the shape.
- Tests (EditMode): a region's kind is the same in any order of asking; a settled region holds exactly one settled island; the strip is flat within its tolerance and on the lee side; the start clearing still holds.
- `docs/systems/world.md` updated with the regions and settings (the phase's own system page; the spec waits for phase 4).

## Out of scope

- Anything built on the island (phases 2 and 3).
- Changes to the island mesh resolution, unless the probe shows the 56 x 56 grid faceting on a 180 m island; then it is a step here.

## Done when

- The measurements in the log, within the targets or with the reason they are not.
- Map captures shown to the user.
- A loaded square's islands, triangles and draws measured as in `world.md`'s cost line, with a full archipelago loaded: up to 25 islands at 2 draws each is 50 of the 80 draws before any ship or village.
- Gates green (`Tools/gates.sh all`).
