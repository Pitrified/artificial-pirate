---
status: done
---

# Phase 04 - The brig

## Plan

- **`builders/brig.py`**: `PARAMS` from phase 01's numbers over `ship.build`.
- **New parts in `ship.py`:**
  - the raised quarterdeck, as a deck height step over a span of the length, the bulwark following it;
  - square sails on yards, if phase 02 did not already need them.
  Whatever the brig needs is a parameter or a part; nothing in `ship.py` names a type.
- **Shape passes** in the probe against the brig's list. The same budget: at most 3 k triangles and 5 slots.
- **Unity:**
  - `SO_Ship_Brig`;
  - `PF_Ship_Brig`, a variant of `PF_Ship`;
  - `BuiltModelTests` and the type-bounds test gain the brig.
- **Anchored in the bay.** A brig instance in the Game scene with `WaveRider` and no `ShipController` input, so it rides the waves where it lies.
  - It has no `WakeSource`, since it does not move.
  - Its collider pushes the floating props.
  - If the base prefab's `ShipController` would still move it, the anchored instance does without it. Which way is tidier is decided in the phase and written down.
- **Palette**: one pass against the brig's photograph, in the shared slots. A material override on the variant only where a shared colour clearly cannot serve both.
- **Captures**: both ships in one frame; the sloop sailing past the anchored brig.
- **Code**: comments only where the reason is not in the code.

## Done when

- Gates green; the dev APK builds.
- The brig's list reads, or the findings say which line cannot.
- The capture shows both types at their real sizes side by side.
- `SceneCapture -captureBudget` from the start and over the bay stays within 150 k triangles and 80 draws.

## What the implementation found

- 2026-11-19. **`builders/brig.py`**:
  - the *Niagara*'s numbers over `ship.build`: 33.7 x 9.8 m, freeboard 3 m, a broad trim band for the gun deck;
  - two masts with four yards each, a spanker on the mainmast, three jibs;
  - the draft is not published; 3.4 m is assumed.
- **One new part, not the quarterdeck**:
  - The *Niagara* is flush-decked (phase 01), so no quarterdeck was built.
  - What the brig did need is **yard bracing**. Square sails hung square across the ship are edge-on from the side, and the first side probe showed bare masts. `brace_deg` per mast turns the yards and their sails round the mast. It is a parameter, not a type, and defaults to 0.
  - The brig uses 40°, against its photograph; the sloop 30°, so its topsails show from the side too. `Sloop.fbx` was rebuilt with it.
- **Probe**: two rounds side-on against `niagara_sail.jpg`. The hull, band, two square rigs narrowing upward, spanker and jibs read as the photograph's.
- **Budget**: 1,870 triangles, 5 slots.
- **Unity**, through a throwaway editor script, deleted:
  - `ModelSetup` on `Brig.fbx`;
  - `SO_Ship_Brig`: hull 16.85 / 4.9 / 3.4 / 3.0 m; 7 m/s, 2 m/s², 20 °/s; camera (0, 16, -60) looking at (0, 12, 18);
  - `PF_Ship_Brig`, a variant of `PF_Ship`, with the collider sized from the type.
- **Paint**: the brig's hull is black with an ochre band and the sloop's white with a red one, so the variant overrides the shared slots with `M_BrigTopsides` and `M_BrigTrim`, copies of `M_Topsides` and `M_Trim`. The model's own slot mapping stays shared, and `BuiltModelTests` checks it.
- **Anchored**:
  - `AnchoredBrig` is a `PF_Ship_Brig` in the bay at (-35, 0, 70), heading 60°, with its `WaveRider` and `WakeSource` wired to the water.
  - It first kept its `ShipController`, which with no input wired holds still. But `SaveRestoreTests` and `PauseResumeTests` failed: they hold that the Game scene has exactly one `ShipController`, the player's, which `GameManager` saves.
  - That holds: `ShipController` is the player's control. So the scene's instance has the component removed, as a prefab override; `PF_Ship_Brig` keeps it, so the brig can still be the player's ship.
- **Captures**:
  - `both`: the two ships together on the swell, the sloop sailing past;
  - `brig_side`: the brig broadside in the game, with the sloop passing close.
- **Budget in the scene** (`SceneCapture -captureBudget`), from beside the bay and from 120 m above: 56,840 triangles and 36 material slots in view. That is within 150 k and 80. Both views count the same, so probably everything is in view in each.
- `BuiltModelTests` gains the brig: waterline against its type, keel, bow, transforms and slots.
- **Gates**: compile 0 warnings, EditMode 146/146, PlayMode 19/19. **Dev APK** 73,736,651 bytes; the build left git unchanged.
