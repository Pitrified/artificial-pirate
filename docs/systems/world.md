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
| Islands | `IslandSite`, `WorldGenerator.TryGetIsland` | At most one a cell, centred anywhere in it and reaching no further than the next cells (a jittered grid); its cell is its id, and it has its own seed |
| Height | `WorldGenerator.HeightAt` | The ground at any point, in metres: the highest of the islands in the 3 x 3 cells around it; the waterline is 0 and the open sea's floor `Seabed` |

## Rules

- **Every draw comes from a hash** of the seed and the cell's coordinates (`WorldHash`), one stream per kind of draw. A cell can be asked for in any order and always comes out the same; a new kind of draw takes a new stream and moves none of the others.
- **An island reaches no further than the next cells.** Its centre is anywhere in its own cell, and its bounds (`IslandSite.Bounds`: every lobe's ellipse at its widest coastline, turned, plus the warp and the seabed slope) at most a cell from its centre; a radius that would reach further is shrunk to fit. So `HeightAt` asks the 3 x 3 cells around a point and takes the highest ground: islands may meet and merge, and the grid does not show.
- **The shape** (`WorldGenerator.IslandHeight`):
  - the point is first pushed about by noise (`Warp`, features about 1.6 radii apart), which bends the island and gives it hooks and bays;
  - it is measured against each lobe: the main one, and up to `Lobes` more set out to `LobeSpread` radii, each an ellipse stretched up to `Stretch` along the island's turned length, its edge wandering by `CoastWobble`;
  - inside the highest lobe the land is a cone to the lobe's centre (smaller lobes rise less), varied by hills and raised to `Redistribution`, which flattens the shores and sharpens the peaks; under the waterline a smooth slope reaches the seabed over `ShelfWidth`.
- **The start island** sits in the origin's cell in every world, at the origin, round and unwarped, with a fixed seed: the scene's deck props stand on it, and the floating props, the sloop and the brig lie in its water. No other island reaches within `StartClearing` (160 m) of the origin.
- **One height function.** Everything that needs the ground samples `HeightAt`.
- Plain C# over `SO_WorldSettings`, no scene objects, tested in EditMode. A cell's island is kept once worked out (a dictionary that grows by one entry a cell asked for), since a point asks nine cells.

## Streaming (`WorldStreamer`)

On the Game scene's `World` object, following the player's ship.

- The cells within `_loadRadius` (2: a 5 x 5 square, 2 km) of the ship's cell are loaded; when the ship enters another cell, the cells left out are unloaded.
- Islands are built one a frame, ring by ring outward, so the nearest come first.
- An island is a pooled object: a mesh from `IslandMeshBuilder`, `M_Sand` and `M_Grass`, and a mesh collider (the floating props land on it).
- `Begin(seed)` starts a world, dropping what was built and explored. `Awake` begins the scene's own `_seed`; `GameManager` begins the game's before the first frame.
- `IslandMeshBuilder`: a 56 x 56 grid over the island's bounds in its local space, quads wholly deeper than half the seabed left out, sand below 0.8 m and grass above by each quad's mean height, UVs in world metres; it reuses its buffers.
- Cost: a loaded square holds 10 to 16 islands, 14 k to 23 k triangles (seeds 1 to 5), at most about 1.8 k an island; two draws an island.
- A linear fog in the sky's ground colour, from 400 to 900 m, hides islands being built at the edge of the square.

## The water's depth (`WorldDepthMap`)

On the `World` object, with the water, the streamer and the ship.

- A 256 x 256 map over 512 m around the ship: `clamp01((seaLevel - HeightAt) / 8 m)`, the encoding the water shader reads for its shore colour, foam and calm.
- A new world (a new `WorldStreamer.World`) is drawn at once. When the ship has sailed 128 m from the map's centre, a map around it is filled 16 rows a frame into the hidden one of two textures, and swapped in whole: 16 frames, 4 096 samples a frame.
- The centre moves in whole texels (2 m), so a texel always covers the same ground.
- It replaces the editor bake and `T_WaterDepth.png`, which needed the ground's colliders in the scene.

## Seed, exploring and the save

- **Explored cells** (`ExploredCells`, on the streamer): the cells within `_seeRadius` (1: a 3 x 3 square) of the ship's cell, added when the ship enters a cell. They are kept by cell, as islands are, so later state (an island visited, looted) can be kept the same way. `Version` goes up when cells are added, so a map can tell when to redraw.
- **The seed.** `GameSceneWiring` hands the streamer to `GameManager.RegisterWorld` before it wires the ground. With a save, the saved seed begins and the saved explored cells are added back; without one (a new game), a random seed from 1 up begins.
- **The save** (`GameSaveData` version 3, `docs/functional-specs.md` §9) holds the seed and the explored cells; `GameManager` writes them with the ship.

## The maps

- **`WorldMapTiles`**: a cell drawn once as a tile of `SO_MapStyle.TileSize` (48) pixels, coloured by height (deep to shallow water, sand, grass, rock), and kept. Cells no island reaches share one open-sea tile; an island's tile is allocated the first time it is painted (9 KB).
- **`WorldMap`**, on the `World` object: the streamer's world in tiles; `Compose` draws any rectangle of cells into a pixel buffer, the cells not explored in `SO_MapStyle.Unexplored`, painting at most a budget of new tiles. Both maps compose from it, so a cell is painted once.
- **`Minimap`** (`Game.UI`, on the HUD, top right, 220 px): north up, the 3 x 3 cells around the ship in a 144 px texture, recomposed when the ship enters a cell or more is explored, one new tile a frame. The image shows a window two cells wide centred on the ship (`RawImage.uvRect`), and the ship's arrow (a texture made in code) turns with its heading. Tapping it raises `Clicked`.
- The HUD is uGUI, as the joystick and pause button are; `Game.UI` now references `Game.Gameplay` and uGUI.
- The HUD's canvas sorts above the UI Toolkit screens, so `GameSceneWiring` shows it only in `Playing`: on the map, pause and diagnostics screens it is off and cannot cover their buttons or take their taps.
- **The full map.** Tapping the minimap: `GameSceneWiring` calls `GameManager.OpenMap(worldMap, ship)`, which has `WorldMap.ComposeExplored` draw the smallest square of cells holding every explored one (at most 21 a side, 1 008 px, centred on the ship when they spread wider), places the ship on it, disables input and moves to `Paused`, as the pause screen does. `UISystem.ShowMap` shows it on the `Map` document (`UI/Map.uxml`, `MapPanel`): a square window on the picture, the ship's arrow placed and turned, and a Close button that raises `OnMapCloseRequested`, on which `GameManager` hides the map and resumes. `IUISystem` takes only a texture, the ship's place and the view's shares of the map's side, so `Game.UI`'s screens need nothing of the world.
- **Pan and zoom.** The window opens centred on the ship, `SO_MapStyle.ViewCells` (7) across, or the whole picture when less has been explored. One finger drags, two pinch about their midpoint, from the whole picture to `ClosestCells` (2) across; the view never leaves the picture. Ship centres on the ship, the zoom kept; Reset zoom goes back to the opening zoom, the centre kept. `MapView` holds the view and the gestures' math, with no Unity events; `MapPanel` feeds it the window's pointer events. At 48 px a cell, 2 cells across is soft.
- No pan or zoom yet; the map fits the explored area.

## The ground under the ship

`GameSceneWiring` hands the world to the player's `HullContact` (`UseGround`). A step that would put the hull's leading end (the bow ahead, the stern astern) over ground shallower than the draft is undone and the ship stops; it can turn or back away. See `docs/systems/ship-controller.md`.

## Settings (`SO_WorldSettings`)

| Field | Default | |
| ----- | ------- | - |
| `CellSize` | 400 m | |
| `IslandChance` | 0.5 | The share of cells with an island |
| `Radius` | 30 to 110 m | The main lobe at the waterline, before stretch and wobble; shrunk to reach no further than the next cell |
| `Peak` | 4 to 40 m | |
| `Stretch` | up to 2 | Length against width |
| `Lobes` | up to 2 more | |
| `LobeSpread` | 1.3 radii | How far a lobe's centre lies, at most |
| `LobeSize` | 0.35 to 0.75 | A share of the radius |
| `CoastWobble` | 0.3 | A share of the radius |
| `Warp` | 0.6 | A share of the radius |
| `Hills` | 0.5 | 0 a smooth cone, 1 strongly varied |
| `Redistribution` | 1.6 | |
| `ShelfWidth` | 25 m | |
| `Seabed` | -20 m | |
| `StartRadius`, `StartPeak` | 24 m, 8 m | |
| `StartClearing` | 160 m | Open water kept around the origin |

## Tests

`MapScreenTests` (PlayMode): tapping the minimap opens the map over the paused game; Close resumes.
`GameManagerTests` (EditMode): `OpenMap` from Playing shows the map with the ship placed and pauses, the close event resumes; from elsewhere it is ignored.
`WorldMapTests` (EditMode): the full map fits the explored cells, or caps around the ship; explored cells drawn by height and the rest dark; new island tiles painted within a budget, open sea free; the minimap drawing around the ship, centring its window, turning its arrow and raising `Clicked`.
`WorldDepthMapTests` (EditMode): a new world drawn at once, shallow over the start island and deep in the open; sailing far fills a new map over frames before showing it; another seed drawn at once; the water grid following in whole quads.
`SaveRestoreTests` (PlayMode): a new game picks a seed; the seed and the explored cells come back on Continue.
`WorldStreamerTests` (EditMode): explored cells reveal a square and count only new cells; the streamer marks the cells around its target; a mesh follows the height function; the streamer builds the islands around its target one a frame, nearest first, drops those left behind, and starts again on another seed.
`HullContactTests`: aground only where the leading end is over shallow ground; a ship sailed at the start island stops short of it.
`WorldGeneratorTests` (EditMode): the same seed gives the same world in any order and another seed another; islands centred in their cells reach no further than the next, within the ranges; outside its bounds an island is the seabed; the start clearing is open water for several seeds; the share of cells with an island; heights between the seabed and the highest peak; the start island at the origin with the props where they belong, for several seeds.
