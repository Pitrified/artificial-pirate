# 📄 `docs/systems/world.md`

## Overview

The sea grows as the ship sails: islands are generated from a seed, cell by cell around the ship.
Plain, layered and deterministic, so later content (settlements, loot, wrecks) can be another layer on the same island records.
The research and the choices are in `plans/08_world_map/00_start.md`.

---

## Layers

| Layer | Where | What it is |
| ----- | ----- | ---------- |
| Seed | `WorldGenerator.Seed` | An int; the same seed gives the same world |
| Cells | `CellCoord`, `WorldGenerator.CellAt` | Squares of `CellSize` (400 m), cell (0, 0) centred on the origin |
| Islands | `IslandSite`, `WorldGenerator.TryGetIsland` | At most one a cell, kept wholly inside it (a jittered grid); its cell is its id, and it has its own seed |
| Height | `WorldGenerator.HeightAt` | The ground at any point, in metres; the waterline is 0 and the open sea's floor `Seabed` |

## Rules

- **Every draw comes from a hash** of the seed and the cell's coordinates (`WorldHash`), one stream per kind of draw. A cell can be asked for in any order and always comes out the same; a new kind of draw takes a new stream and moves none of the others.
- **An island never leaves its cell.** Its radius is capped so its reach (the radius with the coastline's wobble, plus the seabed slope, `SO_WorldSettings.Reach`) fits, and its centre is jittered only within the slack left. So `HeightAt` asks one cell, every cell's edge is the open seabed, and nothing is stitched across cells.
- **The shape** (`WorldGenerator.IslandHeight`): a blob whose coastline wanders with noise; the land is a cone from the coastline to the centre, varied by hills and raised to `Redistribution`, which flattens the shores and sharpens the peak; under the waterline a smooth slope reaches the seabed over `ShelfWidth`.
- **The start island** sits in the origin's cell in every world, at the origin, with a fixed seed: the scene's deck props stand on it, and the floating props, the sloop and the brig lie in its water.
- **One height function.** Everything that needs the ground samples `HeightAt`.
- Plain C# over `SO_WorldSettings`: no scene objects and no allocations, so it runs anywhere and is tested in EditMode.

## Streaming (`WorldStreamer`)

On the Game scene's `World` object, following the player's ship.

- The cells within `_loadRadius` (2: a 5 x 5 square, 2 km) of the ship's cell are loaded; when the ship enters another cell, the cells left out are unloaded.
- Islands are built one a frame, ring by ring outward, so the nearest come first.
- An island is a pooled object: a mesh from `IslandMeshBuilder`, `M_Sand` and `M_Grass`, and a mesh collider (the floating props land on it).
- `Begin(seed)` starts a world, dropping what was built. `Awake` begins the scene's own `_seed` until the game hands one.
- `IslandMeshBuilder`: a 48 x 48 grid over the island's reach in its local space, quads wholly deeper than half the seabed left out, sand below 0.8 m and grass above by each quad's mean height, UVs in world metres; it reuses its buffers.
- Cost: a loaded square holds 11 to 17 islands, 23 k to 36 k triangles (seeds 1 to 5), at most about 2.4 k an island; two draws an island.
- A linear fog in the sky's ground colour, from 400 to 900 m, hides islands being built at the edge of the square.

## The water's depth (`WorldDepthMap`)

On the `World` object, with the water, the streamer and the ship.

- A 256 x 256 map over 512 m around the ship: `clamp01((seaLevel - HeightAt) / 8 m)`, the encoding the water shader reads for its shore colour, foam and calm.
- A new world (a new `WorldStreamer.World`) is drawn at once. When the ship has sailed 128 m from the map's centre, a map around it is filled 16 rows a frame into the hidden one of two textures, and swapped in whole: 16 frames, 4 096 samples a frame.
- The centre moves in whole texels (2 m), so a texel always covers the same ground.
- It replaces the editor bake and `T_WaterDepth.png`, which needed the ground's colliders in the scene.

## The ground under the ship

`GameSceneWiring` hands the world to the player's `HullContact` (`UseGround`). A step that would put the hull's leading end (the bow ahead, the stern astern) over ground shallower than the draft is undone and the ship stops; it can turn or back away. See `docs/systems/ship-controller.md`.

## Settings (`SO_WorldSettings`)

| Field | Default | |
| ----- | ------- | - |
| `CellSize` | 400 m | |
| `IslandChance` | 0.5 | The share of cells with an island |
| `Radius` | 40 to 140 m | At the waterline, before the wobble; capped to fit the cell |
| `Peak` | 4 to 40 m | |
| `CoastWobble` | 0.25 | A share of the radius |
| `Hills` | 0.5 | 0 a smooth cone, 1 strongly varied |
| `Redistribution` | 1.6 | |
| `ShelfWidth` | 25 m | |
| `Seabed` | -20 m | |
| `StartRadius`, `StartPeak` | 26 m, 8 m | |

## Tests

`WorldDepthMapTests` (EditMode): a new world drawn at once, shallow over the start island and deep in the open; sailing far fills a new map over frames before showing it; another seed drawn at once; the water grid following in whole quads.
`WorldStreamerTests` (EditMode): a mesh follows the height function; the streamer builds the islands around its target one a frame, nearest first, drops those left behind, and starts again on another seed.
`HullContactTests`: aground only where the leading end is over shallow ground; a ship sailed at the start island stops short of it.
`WorldGeneratorTests` (EditMode): the same seed gives the same world in any order and another seed another; islands keep inside their cells, within the ranges; every cell's edge is the seabed; the share of cells with an island; heights between the seabed and the highest peak; the start island at the origin with the props where they belong, for several seeds.
