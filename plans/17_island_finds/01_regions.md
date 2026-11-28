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
   Its draws are a new stream; the island record (`IslandSite`) says it is settled and where the strip and the pier's root are, for phase 3.
4. The gaps measured over seeds, against the targets: 150 to 300 m of open water inside an archipelago, 0.5 to 1 km between, a settled island every 600 to 1300 m of sailing.
5. The start clearing (340 m) and the start island unchanged.

## Plan

- Read `WorldGenerator`, `WorldHash`, `IslandSite` and `SO_WorldSettings` first; the region kinds' settings go in `SO_WorldSettings` (no magic numbers).
- Start from the current 400 m cells; if an archipelago cannot reach 150 to 300 m gaps with a chance near 1, try a smaller cell, and then `WorldStreamer._loadRadius` grows to keep about 2 km loaded and the fog hiding the build edge.
- The settled island's radius may exceed what "no further than the next cells" allows at 400 m (bounds plus the shelf); check it against the rule, and shrink or enlarge the cell rather than break the rule.
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
- A loaded square's islands and triangles measured as in `world.md`'s cost line.
- Gates green (`Tools/gates.sh all`).
