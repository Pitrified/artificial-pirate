# The world grows - implementation tracking

Islands generated from a seed around the ship as it sails, a minimap and a full map. Brainstorm, research and questions in [`00_start.md`](00_start.md).

## Key decisions

- Layers: seed, cells of 400 m, one island at most per cell kept inside it (a jittered grid), one height function everything samples.
- Streaming in a 5 x 5 ring of cells around the ship, islands built at runtime.
- Water follows the ship, with a runtime depth map in place of the bake.
- Minimap on the uGUI HUD, the full map a UI Toolkit screen.
- Generated everywhere, a fixed start island at the origin; the ship stops at the shore; the maps show what has been explored; distance fog hides the pop-in.

## Phases

| #  | Phase              | Plan | Status  |
| -- | ------------------ | ---- | ------- |
| 01 | The generator      | [`01_generator.md`](01_generator.md) | done |
| 02 | Island tiles       | [`02_island_tiles.md`](02_island_tiles.md) | done |
| 03 | Water that follows | [`03_water.md`](03_water.md) | done |
| 04 | Seed and save      | [`04_seed_and_save.md`](04_seed_and_save.md) | done |
| 05 | The minimap        | -    | pending |
| 06 | The full map       | -    | pending |
| 07 | Verify             | -    | pending |

## Log

- 2026-11-19: folder created; research and the design in `00_start.md`; waiting on the open questions.
- 2026-11-19: the user took every recommendation ("All ok"); execution starts.
- 2026-11-19: phase 01 done: the generator, its tests, `docs/systems/world.md`.
- 2026-11-19: phase 02 done: islands stream around the ship, the ship stops at the shore, distance fog. The far islands' slopes show past the water grid: phase 03 grows the water.
- 2026-11-19: phase 03 done: the water follows the ship out to 2.3 km, the depth map is made at runtime, the bake and the hand-made island are gone. Open: a sun streak on the calm far water.
- 2026-11-19: phase 04 done: a random seed on a new game, the seed and explored cells saved (version 3) and restored.
