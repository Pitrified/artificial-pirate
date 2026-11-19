---
status: done
---

# Phase 04 - Seed and save

A new game picks a seed; the save keeps it and the cells explored; Continue brings back the same world.

## What the implementation found
- 2026-11-19. **`ExploredCells`**, on the streamer: the 3 x 3 cells around the ship's cell, added when it enters a cell. `Version` goes up on new cells, for the maps to poll.
- **`GameManager.RegisterWorld`**, called by `GameSceneWiring` before it wires the ground:
  - with a save, it begins the saved seed and adds back the explored cells;
  - without one, it begins a random seed from 1 up.
  
  The scene's own seed (`Awake`) builds nothing in between, since `Start` runs before the first `Update`.
- **The save**: `GameSaveData` version 3, with `world.seed` and `world.explored`. An older save resets, as the specs say. §9 of `functional-specs.md` shows it.
- **Tests**:
  - `SaveRestoreTests.SaveReloadContinue_RestoresTheWorld` (PlayMode): a new seed, the ship moved two cells away, and the same seed and explored count after Continue;
  - EditMode: explored cells and the streamer marking them.
  
  One expectation of mine was wrong: two cells over, the squares share a column, so 15 cells, not 18.
- **Gates**: compile 0 warnings, EditMode 176/176, PlayMode 20/20.
