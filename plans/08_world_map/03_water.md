---
status: done
---

# Phase 03 - Water that follows

The water reaches the far islands and follows the ship; the seabed depth comes from the world at runtime; the bake and the hand-made island go.

## What the implementation found
- 2026-11-19. **The grid** (`builders/water_grid.py`, `WaterGrid.fbx`):
  - a fine core of 256 m at 2 m quads;
  - coarse flat quads of 64 m out to 2.3 km;
  - 35,328 triangles, from 33,800.
- **The waves calm far out.** The drawn waves calm to nothing from 100 to 124 m from the grid's centre (`_WaterFarCalm`, pushed by `WaterSurface`). The core's edge is flat, so the coarse quads meet it without cracks. The CPU waves do not calm there, as with the shallows, so floaters should stay near the ship.
- **The grid follows** the ship in whole 2 m quads (`WaterSurface._follow`).
- **`WorldDepthMap`**:
  - 256 x 256 over 512 m around the ship, in the shader's encoding;
  - a new world is drawn at once;
  - after 128 m of sailing, a new map is filled 16 rows a frame into the hidden texture and swapped in whole.
- **The map's edge reads deep.** The shader clamps past the edge, and a pale band in the first capture looked like an island's shallows smeared outward. The fix and its test are in, but the band stayed, so that was not it.
- **The band** is most likely the sun's reflection. Past about 110 m the water is now calm and mirrors the sun as a straight streak toward it, where the waves used to break it up. It reads like a glitter path, but it starts abruptly. Left as is for now; softer far glint or a wider wave core would be the fixes if it bothers.
- **Removed**:
  - `WaterDepthBake` and `T_WaterDepth.png`;
  - `Island.fbx` and `builders/island.py`.
  
  No GUID references were left.
- **The scene**, through a throwaway editor script (deleted): the water follows the sloop and has no depth map set; a `WorldDepthMap` on the `World` object.
- **Captures**:
  - from above the sloop: the far islands sit in water, with no skirts, and the shore foam rings the generated start island;
  - from 600 m straight down: fog over nearly everything, and a far island's beach ring in water.
- **Docs**: `water.md` (parts, rules, adding water, cost), `world.md`, and `functional-specs.md` (the Game scene's contents).
- **Gates**: compile 0 warnings, EditMode 174/174 (5 new in `WorldDepthMapTests`), PlayMode 19/19.
