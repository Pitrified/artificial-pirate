# 📄 `docs/systems/camera-controller.md`

## Overview

The camera that follows the player's ship, in one of three views from the ship's type, and shakes when told.

- **Chase**: behind and above, smoothed.
- **Bird's eye**: high and a little astern, looking down toward the bow, smoothed.
- **Helm**: the helmsman's eyes over the bow, held fast; **steady** (the heading only, the horizon level) or **riding** (the hull's pitch and roll).

The view is the player's setting (`GameSettings.Camera`, `HelmRides`, `docs/systems/game-manager.md`); the HUD's CAM button cycles it. While the player's ship sinks, the scene's wiring shows the chase whatever the setting (`ShipDamage.SinkingStarted`): the hull goes under and tips while the ship's root stays at the surface, which no helm pose survives. A change of view eases from where the camera was to the new view over `SwitchSeconds` (0.5 s). Each type carries a rig (place and look point, in the ship's space) per view (`SO_ShipType.Rig`): the chase's is `CameraOffset` and `CameraLookAt`; the bird's eye about 45 m up and 22 m astern on the sloop, 60 and 30 on the brig; the helm's eye from the ship builder's report (`helm_eye`: behind the wheel or beside the tiller, 1.7 m over the deck).

---

## Responsibilities

- Follow target Transform
- Maintain an offset in target-local space, given with the target
- Smooth position and rotation
- Optional look-at on the target
- Shake: an offset that wanders and dies away, laid over the pose at the end; the follow keeps its own pose apart from the transform, so neither the shake nor a change of view feeds into the smoothing

---

## Ownership & Lifetime

- Component on the Game scene's `MainCamera` GameObject.
- Created/destroyed with the Game scene.

---

## Tunable data (serialized)

- `float PositionLerp` (per-second, e.g. `5`)
- `float RotationLerp` (per-second, e.g. `5`)
- `bool LookAtTarget` (default `true`)
- `float ShakeMost` (0.8 m), `ShakeDecay` (6 per second), `ShakeFrequency` (12 Hz)
- `float VolleyShake` (0.12 m: the followed ship's volley) and `HitShakePerHealth` (10 m per share of health it loses), used by `GameSceneWiring`

Overridable via a `SO_CameraTuning` ScriptableObject in `Assets/Settings/`.
Where it sits and what it looks at are the followed ship's: `SO_ShipType.CameraOffset` and `CameraLookAt`, handed over by `GameSceneWiring` with the target. A 20 m ship needs the camera far back, and a look point above the deck keeps its sails in view.

---

## Public interface

```
void SetTarget(Transform target, Vector3 offset, Vector3 lookAt)
void SetShip(Transform root, Transform hull, SO_ShipType type, CameraView view, bool helmRides)
void SetView(CameraView view, bool helmRides)   // eases into the new view
void SnapToTarget()              // teleport, used after scene load
void Shake(float strength)       // metres; a stronger shake replaces a weaker one
```

---

## Update rules

- Runs in `LateUpdate` (after ship moves).
- Interpolation uses `1 - Mathf.Exp(-k * dt)` for framerate-independent smoothing.
- Caches `transform` once.

---

## Interactions

### Uses
- Target `Transform` (typically ShipController's transform)

### Used by
- (Nothing - pure consumer)

---

## Constraints

- No input handling.
- No collision avoidance (v0.2).

---

## Failure handling

- Null target → camera holds its last position, logs warning once per scene load.
