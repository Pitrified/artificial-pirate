---
status: in progress
---

# Phase 07 - Verify

## Plan

- A fresh clone of the branch passes the gates.
- The dev APK goes to the phone.
- HUMAN on the phone:
  - start a new game, sail away from the start island for a few minutes, past a few islands, into one's shore;
  - open the map from the minimap, close it, and Continue a saved game;
  - paste the diagnostics report here.
  - Check that 60 fps holds with islands in view, and give your read of the islands, the minimap, the map, the fog and the far water (a sun streak shows on the calm water past about 110 m).

## Done when

- The phone report: 60 fps held, no per-frame allocation beyond a new island's map tile, no problems.
- The user's read written here; what they want changed becomes a phase or a later plan.

## What the implementation found
- 2026-11-19. **Fresh clone** of `feat/08_world_map` at `add62c0`, in `~/repos/artificial-pirate-verify` (deleted after):
  - LFS: 17 files, real content, the models identical to the working tree's;
  - `Tools/setup.sh` exit 0;
  - gates from an empty `Library/`: compile 0 errors, 0 warnings; EditMode 182/182; PlayMode 21/21; the clone's tree unchanged;
  - `plans.py check` passes; 125 relative links in the markdown, none broken.
- **The dev APK**: `Tools/build-android.sh dev` at `add62c0`, Build SUCCEEDED, 74,128,459 bytes (85 kB more than the ship model's); the build left git unchanged.
- **Not measured here**: triangles and draws with islands in view. `SceneCapture -captureBudget` counts the scene in the editor, where no island is built. By count, 23 k to 36 k island triangles are loaded around the ship, and each island is two draws.
