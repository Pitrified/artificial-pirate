---
status: done
---

# Phase 02 - Island tiles

`WorldStreamer` and the runtime island meshes in the Game scene, the ship stopping at the shore, and the distance fog.

## What the implementation found
- 2026-11-19. **`IslandMeshBuilder`** builds a 48 x 48 grid over an island's reach, in its local space:
  - the quads wholly deeper than half the seabed are left out;
  - two submeshes, sand below 0.8 m and grass above;
  - its buffers are reused, so a build allocates nothing.
- **`WorldStreamer`**:
  - loads the 5 x 5 cells around the ship and builds one island a frame, ring by ring outward;
  - pools the island objects, each with a mesh collider;
  - `Begin(seed)` starts a world; `Awake` begins the scene's own seed until phase 04 hands it the game's.
- **The ground.** `HullContact.UseGround` and `Aground`: a step that would put the bow (or the stern, astern) over ground shallower than the draft is undone, and the speed drops to 0. `GameSceneWiring` hands the world to the player's hull.
- **The scene**, through two throwaway editor scripts (deleted):
  - `SO_WorldSettings.asset` is new;
  - the `Island` instance is gone, replaced by a `World` object with the streamer, wired to the sloop and to `GameSceneWiring`;
  - the deck props are lifted onto the start island's ground (3 to 4.5 m up its hill);
  - a linear fog from 400 to 900 m in the sky's ground colour.
  
  The first script left the streamer's settings reference empty in the saved scene, although it was set before saving. A second script set it from the asset on disk.
- **Capture** in Play mode, from above the sloop: the start island with its beach and hill, four generated islands on the horizon, the sloop and the brig in deep water.
  - Past the 260 m water grid, the far islands' underwater slopes stand in the open as pale skirts. The water has to reach them: phase 03 grows the grid with coarse flat rings out to about 2 km, and calms the drawn waves to nothing at the fine core's edge so the rings cannot crack against it.
- **Cost**:
  - 11 to 17 islands in the loaded square, 23 k to 36 k triangles for seeds 1 to 5, at most about 2.4 k an island;
  - with the scene's 65 k that is within 150 k even if all of it were in view;
  - two draws an island, so 12 islands in view take the scene to about 72 slots of 80. That is for the phone to measure; one land material would halve it.
- `Island.fbx`, its builder and the depth bake are still there; phase 03 replaces the bake and removes them.
- **Gates**: compile 0 warnings, EditMode 169/169 (4 new in `WorldStreamerTests`, 2 in `HullContactTests`), PlayMode 19/19.
