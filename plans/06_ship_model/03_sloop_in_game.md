---
status: planned
---

# Phase 03 - The sloop in the game

## Plan

- **`SO_ShipType`** (`Game.Gameplay`), in place of `SO_ShipTuning`:
  - the hull's half length, half beam and waterline offset;
  - the movement tuning: `SO_ShipTuning`'s five fields, their defaults set for the sloop;
  - the camera offset.
  `SO_Ship_Sloop.asset` in `Settings/`. `SO_ShipTuning` and its asset are removed.
- **Readers:**
  - `ShipController`: takes the type instead of `_tuning`; `ShipControllerTests` move to it.
  - `WaveRider` and `WakeSource`: read the half length, half beam and waterline offset from the type, and their own size fields go.
    - Tests that build ships in code configure them with a type made in code.
    - `WakeSource`'s dent and foam radii become fractions of the beam, so one setting suits every type.
  - Camera: `SO_CameraTuning` keeps the follow settings, and the offset comes from the followed ship's type.
    `GameSceneWiring`, which already wires the ship and the camera, hands the camera the type's offset.
    The offset field in `SO_CameraTuning` goes.
- **Prefabs**, through a throwaway editor script run in batch mode and deleted before the commit:
  - `PF_Ship` becomes the base, the root's components unchanged and a `Hull` child for the model;
  - `PF_Ship_Sloop` is a variant carrying `Sloop.fbx` and `SO_Ship_Sloop`, and the hull's `BoxCollider` sized from the type;
  - the Game scene's ship becomes `PF_Ship_Sloop`, with the scene's wiring kept.
  - The model's import: `ModelSetup` maps the five slots to `Assets/Art/M_<slot>.mat`.
- **Waves.** `SO_WaterSettings` gets longer waves so a 12 m hull pitches and rolls, the shortest still 3 m or more on the 2 m grid.
  - Looked at in a sequence before and after.
  - The depth map is rebaked only if the shore foam changes.
- **Wake.** Looked at in a sailing sequence at the new size. Its strength is the phone's to tune, as before.
- **Tests:**
  - `BuiltModelTests`: a case for `Sloop.fbx`, with a pivot check for a waterline origin;
  - a test that each `SO_ShipType` asset's half length and half beam match its model's bounds within a few centimetres.
- **Palette.** One measured pass in a `SceneCapture` against the sloop's photograph (`docs/building_assets.md` §5.3), set with `MaterialTuning`.
- **Captures:**
  - side-on and chase sequences of the sloop sailing and turning;
  - `measure.py track` on the hull for its pitch and roll.
- **Docs**: `ship-controller.md` (the type, its readers), `water.md` (the wake's radii, the waves), `camera-controller.md`.
- **Code**: no shims left for the removed fields; comments only where the reason is not in the code.

## Done when

- Gates green: compile with 0 warnings, EditMode and PlayMode.
- The capture shows the sloop sailing on the waves with its wake, the camera framing it.
- The dev APK builds.

## What the implementation found
