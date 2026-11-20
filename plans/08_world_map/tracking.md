# The world grows - implementation tracking

Islands generated from a seed around the ship as it sails, a minimap and a full map. Brainstorm, research and questions in [`00_start.md`](00_start.md).

## Key decisions

- Layers: seed, cells of 400 m, one island at most per cell, centred anywhere in it and reaching no further than the next cells, one height function everything samples.
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
| 05 | The minimap        | [`05_minimap.md`](05_minimap.md) | done |
| 06 | The full map       | [`06_full_map.md`](06_full_map.md) | done |
| 07 | Verify             | [`07_verify.md`](07_verify.md) | in progress |
| 08 | Wonkier islands    | [`08_wonkier_islands.md`](08_wonkier_islands.md) | done |
| 09 | The HUD only while sailing | [`09_hud_while_sailing.md`](09_hud_while_sailing.md) | done |
| 10 | Waves out to the fog | [`10_far_water.md`](10_far_water.md) | done |
| 11 | Pan and zoom the map | [`11_map_pan_zoom.md`](11_map_pan_zoom.md) | done |
| 12 | Patches on the far water | [`12_far_water_patches.md`](12_far_water_patches.md) | done |

## Log

- 2026-11-19: folder created; research and the design in `00_start.md`; waiting on the open questions.
- 2026-11-19: the user took every recommendation ("All ok"); execution starts.
- 2026-11-19: phase 01 done: the generator, its tests, `docs/systems/world.md`.
- 2026-11-19: phase 02 done: islands stream around the ship, the ship stops at the shore, distance fog. The far islands' slopes show past the water grid: phase 03 grows the water.
- 2026-11-19: phase 03 done: the water follows the ship out to 2.3 km, the depth map is made at runtime, the bake and the hand-made island are gone. Open: a sun streak on the calm far water.
- 2026-11-19: phase 04 done: a random seed on a new game, the seed and explored cells saved (version 3) and restored.
- 2026-11-19: phase 05 done: shared map tiles, the HUD minimap, `CAPTURE_HUD`.
- 2026-11-19: phase 06 done: the full map over the paused game, opened from the minimap; `CAPTURE_MAP`.
- 2026-11-19: phase 07: a fresh clone passes the gates, the dev APK is built; waiting on the phone.
- 2026-11-19: the user found the first look too griddy and round: phase 08 moves islands anywhere in their cells, lets them spill into the next, and warps, stretches and lobes their shapes. A new APK.
- 2026-11-20: two phone reports: 60 fps held, Continue restores the world. The user's read: islands, minimap, map and fog are ok; the HUD covers the paused screens' buttons (phase 09); the far water calms too abruptly, and the map should pan on a drag.
- 2026-11-20: phase 09 done: the HUD shows only while playing. A new APK.
- 2026-11-20: the user took option A for the far water (phase 10), and asked for drag, pinch zoom, recentre and reset zoom on the map (phase 11).
- 2026-11-20: phase 10 done: the waves' light goes out to the fog; the phone check waits for phase 11's APK.
- 2026-11-20: phase 11 done: the map drags and pinches, with Ship and Reset zoom. A new APK with phases 10 and 11 for the phone.
- 2026-11-20: the phone run of phases 09 to 11: 60 fps, the map and the HUD fine; the far water still "a bit linear": phase 12 adds broad patches far out.
- 2026-11-20: phase 12 done: broad patches on the far water. A new APK for the phone.
