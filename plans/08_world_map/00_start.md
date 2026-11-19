---
status: in progress
priority: 0
description: |
  A sea that grows as the ship sails: islands generated from a seed, tile by tile around the
  ship, with some elevation; a minimap on the HUD and a full map when it is tapped. Scaffolding
  that objects, settlements and loot can hang on later.
---

# The world grows

Asked 2026-11-19 by the user: "Now we plan the map growing. So that when the ship moves more tiles appear. We procedurally generate the map. Nothing too fancy for now. We pick a seed, have a rule to spawn blobs that are island with some elevation. We keep it quite open to future work like objects, settlements, loot. Easy scaffolding for now. Search online for standard patterns. Plus a mini map overlay, and a map when clicked."

## Where it stands

- **One island**, `Island.fbx` from `builders/island.py`: a round radial height field. It is flat at 0 out to 26 m, has a beach to 40 m, and a seabed at -4 m out to 90 m. It has mesh colliders, and two slots, `Sand` and `Grass`.
- **The water** is one static grid at the origin, 260 m square (`WaterGrid.fbx`). Its shader is world space.
  - Shore colour, foam and the calming of the waves read `T_WaterDepth.png`. The editor bakes that by raycasting the island's colliders over the rect -90..90 m (`WaterDepthBake`), and `WaterSurface` hands its rect to the shader as a global.
  - The ripple square already follows the ship.
- **Near the island**: the anchored brig at (-35, 70) and floating barrels and crates.
- **The camera**'s far plane is 1000 m, with no fog. Past the water grid, the sky's ground colour stands in for the sea.
- **The save** holds the ship's position and yaw (`GameSaveData` version 2). A version mismatch resets the save (specs §9).
- **UI**:
  - The HUD in the Game scene is uGUI (the joystick and the pause button).
  - The screens (Loading, Menu, Pause, Diagnostics) are UI Toolkit documents routed by `UISystem`.
  - `docs/systems/ui-system.md` forbids mixing both in one screen.
- **The ship sails through the island** (`ship-controller.md` constraints).
- **Budget**: 150 k triangles and 80 draws; the scene is about 65 k triangles and 48 slots with everything in view.

## Research: the standard patterns

- **Seeded chunks.** Split the world into cells, and give each cell its own random stream, from a hash of the world seed and the cell's coordinates. A cell can then be dropped when the ship leaves and rebuilt the same when it comes back, and nothing depends on the order cells were visited. All randomness must come from that stream.
  - Sources: [Atomic Object](https://spin.atomicobject.com/infinite-procedurally-generated-world/), and Unity's [primer on repeatable random numbers](https://unity.com/blog/engine-platform/primer-on-repeatable-random-numbers), which also covers picking a hash that is both well distributed and cheap.
- **Things larger than a cell.** Generate in layers, where a layer reads the one below with a margin, as [LayerProcGen](https://runevision.github.io/LayerProcGen/) does. For placed features, a coarse cell decides the feature, and every chunk it overlaps asks that cell ([Godot forum](https://forum.godotengine.org/t/infinite-procedural-world-how-to-implement-a-high-performance-map-with-pan-zoom-interactive-pois/135066)). The simplest form keeps each feature inside its own cell, so no feature crosses a cell edge.
- **Where features go.** A **jittered grid**, one point per cell at a random spot inside it, is fast and easy. It clumps a little more than Poisson disc, which gives the best spacing but is slower and harder to make local ([Red Blob, jittered grid vs Poisson disc](https://redblobgames.com/x/1830-jittered-grid)).
- **Island shapes.**
  - Noise reshaped by a distance function: land is where `noise - distance` is above the sea ([Red Blob, polygon map generation](https://www.redblobgames.com/maps/polygon-map-generation/)).
  - Several shaping functions are compared in [Red Blob, island shaping functions](https://www.redblobgames.com/maps/terrain-from-noise/islands.html): "square bump" forces the island shape hardest, and "smooth" keeps most of the noise's variation.
  - A power on the elevation (redistribution) flattens the lowlands and sharpens the peaks.
- **Minimaps.**
  - Draw the map once, or once per piece, into a texture, rather than with a second camera every frame.
  - Keep what has been explored as a separate mask, and update it only where it changes.
  - Large textures set pixel by pixel are slow, so keep them small and update them rarely.
  - Sources: [itch.io devlog](https://itch.io/devlog/717814/minimap-and-fog-of-war.amp), [Unity discussions](https://discussions.unity.com/t/cheap-way-in-making-fog-of-war-215624/215624).

## The design

Built in layers, each reading only the one below, so later content (settlements, loot, wrecks) is another layer on the same island records.

1. **`WorldSeed`**: an int, picked on a new game and saved.
2. **Cells.** A cell is 400 m square, addressed by integer coordinates. A hash of (seed, cx, cz) seeds each cell's stream (`System.Random`, or a small hash of our own if that turns out not to be stable across runtimes).
3. **The island layer.** It uses a jittered grid: each cell holds at most one island, kept wholly inside the cell, so there is nothing to stitch across edges. An island is a record, `IslandSite`:
   - its cell, which is its stable id;
   - its centre, and a radius of 40 to 140 m;
   - its peak height, 4 to 40 m;
   - its own seed for its shape, and later for whatever is put on it.
   About half of the cells have an island; the share is a setting.
4. **The height function.**
   - `HeightAt(x, z)` asks only the cell the point is in.
   - The shape is a blob: a radial falloff with the coastline wobbled by low-frequency noise. A noise height is shaped by distance (Red Blob's "smooth") and raised to a power for flat shores and a peak.
   - Every other part samples this one function, which is plain C# with no Unity objects:
     - the island meshes;
     - the water's depth map;
     - the map's colours;
     - a check for land under the ship.
   - That makes it testable in EditMode.
5. **Streaming.** A `WorldStreamer` in the Game scene follows the ship.
   - The cells within 2 of the ship's cell (5 x 5, 2 km) are loaded, and a cell that falls out is unloaded.
   - Island objects are pooled, and at most one is built a frame.
   - Each island's mesh is a height grid over its bounds, built at runtime. It uses the `Sand` and `Grass` slots split by height (plus `Rock` above a height if it reads better), and a mesh collider.
6. **Water follows the ship.**
   - The water grid moves with the ship, snapped to whole grid cells so the vertices do not swim. The shader is world space already.
   - The depth map is made at runtime from the height function: 256 x 256 over 512 m around the ship, remade when the ship has moved 128 m from its centre.
   - The editor bake goes, and so does `T_WaterDepth.png`.
7. **The save** gets the seed and the explored cells (version 3: an old save resets, as the specs say).
8. **The minimap** sits in a HUD corner (uGUI, as the HUD is): north up, with the ship as an arrow that turns.
   - Its texture is drawn from the height function's colours for the 3 x 3 cells around the ship, and redrawn when the ship enters a new cell, not every frame.
   - Unexplored cells are dark.
9. **The full map** opens when the minimap is tapped. It is a UI Toolkit screen routed by `UISystem`, like Pause.
   - It pauses the game, shows every explored cell with the ship's mark, and closes with a button.
   - It is drawn from the explored cells when it opens.
   - No pan or zoom for now: it fits the explored area to the screen.

Kept out, but left room for:
- **Content.** Content is a later layer reading `IslandSite`: an island's own seed decides what is on it, and the explored set is keyed by cell, so later state such as looted or visited can be keyed the same way.
- **Map markers.** The map draws from a list of marks, so one kind (the ship) now, and more later.

## Phases

1. **The generator.** Seed, cells, islands and the height function, in plain C#.
   - Tests: the same seed gives the same world; a cell's result does not depend on which cells came first; every island stays inside its cell; the start cell rule (question 1).
   - A throwaway editor dump of a height map to a PNG, to look at a few seeds.
2. **Island tiles.** `WorldStreamer`, the runtime island meshes, pooling, the Game scene wired up.
   - Captures, and the triangle and slot count in view.
3. **Water that follows.** The grid follows the ship, the depth map is made at runtime, and the bake is removed. Captures of shore foam on a generated island.
4. **Seed and save.** A new game picks a seed, and the save holds it and the explored cells. Tests in `SaveRestoreTests`.
5. **The minimap.** It goes on the HUD, its texture is drawn per cell, and it marks explored cells.
6. **The full map.** The screen, opened from the minimap, pausing the game.
7. **Verify.** Gates, a dev APK for the phone, the budget with islands in view, and a fresh clone.

## Open questions

1. **The start.** The hand-made island at the origin has the brig in its bay and the floating props around it.
   - (a, recommended) Generate everything, with the origin's cell always holding a medium island where the bay keeps the brig and the props in water. `Island.fbx`, its builder and the bake go.
   - (b) Keep the hand-made island, and leave the origin's cell to it.
2. **The ship and the land.** With hills, sailing through an island looks wrong. Stop the ship at the shore, using the height function under its bow as if it were a hull that cannot move (`HullContact`'s rule)? That is small: recommended, as part of phase 2.
3. **What the maps show.** Only what has been explored, the cells the ship has been near (recommended, and what makes a map worth having)? Or the whole world around the ship?
4. **Pop-in.** Islands load 800 m to 1 km away, and the camera sees 1 km. A thin distance fog in the sky's colour, from about 400 to 900 m, would hide islands appearing and the water grid's edge. Recommended.
5. **Sizes.** Cells of 400 m, islands 40 to 140 m in radius with peaks of 4 to 40 m, and an island in about half the cells. The sloop is 20 m long. Change anything?

## Answers

The user, 2026-11-19: "All ok": every recommendation taken.
1. Everything generated; the origin's cell always holds a medium island whose bay keeps the brig and the props in water. `Island.fbx`, its builder and the bake go.
2. The ship stops at the shore, in phase 2.
3. The maps show only what has been explored.
4. A thin distance fog in the sky's colour, about 400 to 900 m.
5. The sizes as proposed.
