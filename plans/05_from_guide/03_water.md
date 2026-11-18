---
status: done
---

# Phase 03 - Water

## Plan

- Copy `Gameplay/Water/*`, `Water.shader`, `WaterRippleSim.shader`, the normal texture, `SO_WaterSettings`, `Editor/WaterDepthBake.cs`, `docs/systems/water.md`, the water tests.
- In the Game scene: the guide's island in place of `PF_Island`, the `Water` object in place of `Sea`, the depth map baked against the island; `SG_Water.shader` and its material removed.
- A few floating barrels and crates with `Buoyancy` and `RippleSource`.

## Done when

- Gates green; a capture sequence of the water and the props; dev APK.

## What the implementation found

- 2026-11-18. From the guide at `0716d6d`: `Gameplay/Water/*`, `Water.shader` and `WaterRippleSim.shader` (named `artificial-pirate/...`), `M_Water` (the guide's, another GUID; this repo's, on `SG_Water`, removed with the shader), `M_WaterRippleSim`, `M_Sky`, the normal and depth textures, the water grid model, `SO_WaterSettings`, `WaterDepthBake`, `docs/systems/water.md`, and the tests `WaveFieldTests`, `BuoyancyTests`, `FloatingPropsTests`, `WaterRippleTests`, `SequenceCaptureTests`, `AllocationTests`. No project setting differs but the product names, so none changed.
- **The scene, carried as prefabs**: the guide placed its water and props from a script it did not keep. A throwaway editor script in the guide listed its Game scene and saved the island, the water, the beach props and the five floating barrels and crates as prefabs; another here removed `Sea` and `PF_Island`, placed them unpacked (the barrels and crates stay instances of `PF_Barrel` and `PF_Crate`), wired each `Buoyancy` and `RippleSource` to the water, set the sky, and moved the ship's start into the bay, (0, 0.5, 55) facing the island, since the guide's layout centres on the island at the origin. Both scripts and the prefabs deleted after; the guide's tree is clean. The listing here then matched the guide's, blobs, avatar and launcher left out (Q2).
- `PF_Island`, `M_Island` and `SG_Water` removed (nothing else used them). `CannonballLauncher` stays as a script for `WaterRippleTests`, not in the scene.
- **The depth bake took floating bodies for seabed**: rebaked here, the map differed from the guide's by 25 texels around the floating props and the ship's start. Its rays stopped at any collider. `WaterDepthBake` now passes through colliders on a Rigidbody (`GroundBelow`); rebaked, only the ship's 10 texels differ, and they go in phase 04 when the ship gets its kinematic Rigidbody. The guide's committed map was baked before its props were placed, but its tool has the same flaw: the fix goes back to the guide.
- Sequence capture (4 s, 6 fps, from (0, 7, 72) to (0, 0, 38)): the island with its beach and shore foam, the floating barrels and crates, the ship's box above the water, sun glint; motion 5.3 mean change per frame, the sky's cells 0.
- Docs: `water.md` (the launcher's row), the spec's water row, scene table and systems table, the docs index, `project-structure.md`'s prefabs, `headless-unity.md`'s sequence gate.
- Gates: compile 0 warnings, EditMode 132/132, PlayMode 16/16; dev APK 73,680,002 bytes, git unchanged by the build.
