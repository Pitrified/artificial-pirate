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
- **A new dev APK** after phase 08 (wonkier islands): 74,159,539 bytes. The fresh-clone check was of `add62c0`, before it.
- 2026-11-20. **Phone report**, Pixel 7 Pro, the phase 08 APK (built from `54aeb73` with phase 08 uncommitted, 22:18Z; committed as `a3e98fe` three minutes later). A new game, seed 1024627825, 415 s:
  - 60 fps held: p05 59.7 fps, p50 16.8 ms; 26 frames over 33 ms (0.17%), all at the scene load or on coming back from the background;
  - memory flat at 150 to 154 MB, heap 10 to 14 MB; per frame p95 0.1 kB, 9 collections; one frame of 5.7 MB, most likely the scene load or the map's first compose;
  - the map was open from 103.7 s to 406.5 s, with the app in the background for part of it;
  - one warning: `OpenMap called from invalid state 'Paused'` at 405.2 s, a tap on the minimap's spot reaching the HUD through the open map. The game ignored it, but the map screen does not stop taps reaching the HUD under it;
  - not in this run: Continue of a saved game (the session ended on the pause screen), and how long was sailed among islands (about 100 s of play before the map opened).
- 2026-11-20. **Second phone report**, the same APK, Quit then Continue, 69 s:
  - restored seed 1024627825 with 12 cells explored, and the ship at (468.9, -148.4) yaw 195.3;
  - 60 fps held (p05 59.7); 9 slow frames, all at the scene load or the pause screen opening;
  - the 5.7 MB frame shows again with no map opened, so it is the scene load, not the map.
- **The user's read**, so far: on the map and diagnostics screens the HUD (joystick, minimap) covers their buttons and takes their taps. The HUD canvas sorts above the UI Toolkit panels; the fix, hiding the HUD while not sailing, is proposed as phase 09.
- **Not measured here**: triangles and draws with islands in view. `SceneCapture -captureBudget` counts the scene in the editor, where no island is built. By count, 14 k to 23 k island triangles are loaded around the ship after phase 08, and each island is two draws.
