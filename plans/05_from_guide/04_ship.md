---
status: done
---

# Phase 04 - The ship on the water

## Plan

- The ship rides the waves, kinematically: each frame, the `WaveField` height at bow, stern, port and starboard gives a target height, pitch and roll, smoothed; planar movement unchanged.
- A `RippleSource` on the hull; a kinematic Rigidbody and collider so the props bounce off it.
- Tests: on flat water the ship stays level at sea level; on a wave it rises and pitches with it; movement tests unchanged.
- `docs/systems/ship-controller.md` updated.

## Done when

- Gates green; a capture sequence of the ship sailing through the waves past the props.

## What the implementation found

- 2026-11-19. **`WaveRider`** (`Game.Gameplay`) on the ship's root: the water at bow, stern, port and starboard sets the `Hull` child's height, pitch and roll, eased at 4 per second, placed at once in `Start`. The root stays on the plane with yaw only: tilting it would bend `ShipController`'s `forward` and its `Rotate` axis, so `ShipController` and its tests are unchanged, and the camera (which follows the root) does not bob.
- `PF_Ship` re-rigged by a throwaway editor script: mesh, renderer and box collider moved from the root to a `Hull` child (with the root's old scale, 1 x 0.5 x 2), a `RippleSource` on the hull, a kinematic `Rigidbody` and `WaveRider` (half length 1, half beam 0.5) on the root; in the scene, the rider and the ripple source wired to the water. The scene's ship start now at y 0, the height `ShipController` holds it at (it was 0.5, so the first frame dropped it).
- **A splash at the start**: the first sequence showed a big ring around the ship in its first frames. `RippleSource` took its first position in `OnEnable`, before the hull was put on the water, and read the move as a fast entry. It now takes its first position in its first `Update`; with the hull placed in `Start` and the start height fixed, the ring is gone. The same flaw is in the guide's `RippleSource`: the fix goes back with the bake's.
- Depth map rebaked: with the hull's collider on the kinematic Rigidbody, the map is now byte for byte the guide's.
- `WaveRiderTests` (EditMode, 4): level at the sea on flat water; nose up where the water rises toward the bow, at the slope's angle, no roll with crests across; roll with crests along; eased after the first step.
- Sequence capture side on (6 s, 5 fps, eye (7, 2, 57)): the hull sits on the water and pitches and rolls with it; its tracked centroid moves 29 px vertically over the run. The ship stands still in the capture (the sequence test gives no input), so sailing past the props is left for the phone, in phase 05.
- `docs/systems/ship-controller.md`: "Riding the waves", the constraints updated.
- Gates: compile 0 warnings, EditMode 136/136, PlayMode 16/16; dev APK 73,684,166 bytes, git unchanged by the build.
