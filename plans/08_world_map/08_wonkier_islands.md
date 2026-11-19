---
status: done
---

# Phase 08 - Wonkier islands

Asked 2026-11-19 by the user, on the first look: "no it's too griddy and regular, can we try more movement within the cell, wonkyer shapes of the islands".

## What the implementation found
- 2026-11-19. **Placement.** An island's centre is now anywhere in its cell, and the island may spill into the next cells but no further. `HeightAt` asks the 3 x 3 cells around a point and takes the highest ground, so neighbouring islands can meet and merge. This is the "feature decided by its cell, asked by every cell it overlaps" pattern from the research.
- **Shapes**:
  - a warp (noise pushing the point about, features about 1.6 radii apart) bends islands into hooks and crescents;
  - a stretch up to 2 along a random direction;
  - up to two more lobes, out to 1.3 radii, as peninsulas;
  - a coastline wobble of 0.3.
  
  Each island's bounds cover all of it. A radius that would reach past the next cell is shrunk.
- **Tried on the way**, with the same seeds side by side: the first set (warp 0.3 at fine features) bent nothing, and stronger values at the same features only roughened the edges. The coarser warp is what changed the shapes.
- **The start island** stays round and unwarped (radius 24 m). No other island reaches within 160 m of the origin, so the props, the sloop and the brig keep open water.
- **Follow-ons**:
  - `WorldMapTiles` asks whether any island reaches a cell before using the open-sea tile;
  - the island mesh is a 56 x 56 grid over the island's bounds;
  - a cell's island is kept once worked out, since a point now asks nine cells;
  - the settings asset was reset to the new defaults, and the deck props put back on the start island's ground (throwaway editor script, deleted).
- **Cost**: 10 to 16 islands, 14 k to 23 k triangles in the loaded square (seeds 1 to 5), at most about 1.8 k an island: fewer than before, since the tighter bounds leave out more deep water.
- **Tests**: islands centred in their cells reach no further than the next; outside its bounds an island is the seabed; the start clearing is open water for four seeds. They replace the cell-edge test.
- **Gates**: compile 0 warnings, EditMode 186/186, PlayMode 21/21. A new dev APK: 74,159,539 bytes.
