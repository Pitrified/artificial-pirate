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

`WorldGeneratorTests` (EditMode): the same seed gives the same world in any order and another seed another; islands keep inside their cells, within the ranges; every cell's edge is the seabed; the share of cells with an island; heights between the seabed and the highest peak; the start island at the origin with the props where they belong, for several seeds.
