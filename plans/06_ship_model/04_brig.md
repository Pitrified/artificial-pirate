---
status: planned
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
