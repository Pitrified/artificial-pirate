---
status: done
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

- 2026-11-19. **`SO_ShipType`**:
  - `SO_ShipTuning` became `SO_ShipType` through `git mv`, so the script and its asset (now `SO_Ship_Sloop`) keep their GUIDs.
  - It holds the hull (half length, half beam, draft, freeboard), the handling, and the camera's offset and look point.
  - No waterline offset: every ship model has its origin on the waterline, so `WaveRider`'s own `_waterlineOffset` stays at 0.
  - The asset's YAML still carries the old `m_EditorClassIdentifier`. It is editor-only, Unity resolves the script by GUID, and YAML is not edited by hand.
- **Readers**:
  - `ShipController` reads the type (`Type` is public for the wiring).
  - `WaveRider` and `WakeSource` read the half length and beam from it. `WakeSource`'s dent radius is now `_dentRadiusHalfBeams` (1.2), so its dent and foam scale with the hull.
  - `CameraController.SetTarget(target, offset, lookAt)`: `SO_CameraTuning` lost its offset, and `GameSceneWiring` hands the camera the ship type's offset and look point.
  - **The look point is new to the plan.** Looking at the waterline from far enough back for a 20 m ship cut the sails off the top of the frame.
- **Prefabs and scene**, through a throwaway editor script, deleted:
  - `PF_Ship` is the base: its `Hull` keeps the box collider and lost the cube and its scale.
  - `PF_Ship_Sloop` is a variant with the `Sloop` model under `Hull`, the collider 6.2 x 4.9 x 20.3 m from the type, and the type on its three components.
  - The scene's ship was swapped with `ReplacePrefabAssetOfPrefabInstance`, which keeps its wiring to the water, the ripples and the camera; it keeps the name `Ship`.
- **Sloop type**:
  - hull 10.15 / 3.1 / 2.4 / 2.5 m;
  - handling as before, except a yaw rate of 30 °/s instead of 60, so a 20 m hull no longer turns in less than its own length;
  - camera at (0, 12, -42), looking at (0, 9, 12).
  These are first values, for the phone to tune.
- **Bow direction.** `BuiltModelTests.Ship_BowFacesPlusZ` failed on the first model: Blender's +Y comes out as Unity's -Z. `ship.py` now turns the finished ship half round so the bow faces Blender's -Y; the probe's starboard view is `--view 270 5`. `docs/building_assets.md` says so.
- **The hull's end caps faced inward**, and back-face culling hid the transom in Unity: the first chase capture saw the deck through an open stern. Their windings are fixed. The Blender probe renders both faces, so only a Unity view shows this.
- **Waves.** `SO_WaterSettings`' four waves went from 12 / 7.5 / 4.5 / 3.4 m to 40 / 24 / 13 / 7 m, with steepness 0.12 / 0.1 / 0.08 / 0.06. The longest is about 0.75 m high. The depth map is unchanged, since it does not depend on the waves.
- **Captures** (`~/.cache/claude-verify/ship/`):
  - `chase_before`: old camera and waves, the sloop small at the bottom of the frame, sails cut off;
  - `chase_after`: full throttle with a turn; the whole rig in view and the hull on a visible swell;
  - `side`: the sloop at 30% throttle from the side, pitching and rolling with the swell; the hull, band and sails read as on the reference.
- **The wake** scales with the beam: the foam trail is now about 6 m wide and solid white. Its strength is still the phone session's to tune (`_foamRate`, `_WakeFoam`).
- **Palette**: the preview colours from `sloop.py` (white topsides, red trim, black bottom, brown wood, off-white sails) were set with no measured pass, as the user asked for less detail; they read as on the reference.
- **Not changed**: the ripple square stays 48 m, about 2.4 ship lengths, and the ship still sails onto the island.
- **Tests**:
  - `BuiltModelTests` has props (size, pivot at the base) and ships (waterline extents against the type, keel at the draft, bow toward +Z); both get transforms and materials.
  - `ShipControllerTests`, `WaveRiderTests`, `WaterRippleTests` and `CameraControllerTests` moved to the type and the new `SetTarget`.
- **Docs**: `ship-controller.md` (ship types), `camera-controller.md` (offset and look point from the type), `water.md` (the wake's radii in half beams).
- **Gates**: compile 0 warnings, EditMode 141/141, PlayMode 19/19.
- **Dev APK**: 73,711,286 bytes; the build left git unchanged.
