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
| 02 | Island tiles       | -    | pending |
| 03 | Water that follows | -    | pending |
| 04 | Seed and save      | -    | pending |
| 05 | The minimap        | -    | pending |
| 06 | The full map       | -    | pending |
| 07 | Verify             | -    | pending |

## Log

- 2026-11-19: folder created; research and the design in `00_start.md`; waiting on the open questions.
- 2026-11-19: the user took every recommendation ("All ok"); execution starts.
- 2026-11-19: phase 01 done: the generator, its tests, `docs/systems/world.md`.
