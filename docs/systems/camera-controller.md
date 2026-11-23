# 📄 `docs/systems/camera-controller.md`

## Overview

Third-person chase camera. Follows the ship from a fixed offset with smoothing, and shakes when told.

---

## Responsibilities

- Follow target Transform
- Maintain an offset in target-local space, given with the target
- Smooth position and rotation
- Optional look-at on the target
- Shake: an offset that wanders and dies away, laid over the smoothed position and taken off before the next smoothing step, so it never feeds back into the follow

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
