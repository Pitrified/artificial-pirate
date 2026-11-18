# 📄 `docs/systems/ship-controller.md`

## Overview

Player ship using a **kinematic** model - the Transform is moved directly,
no Rigidbody forces.

---

## Responsibilities

- Read movement input from InputManager
- Apply forward thrust along ship-forward
- Apply yaw rotation from steering
- Expose current ship state for the camera and save system

---

## Ownership & Lifetime

- Component on the `PF_Ship` prefab.
- Lives only inside the Game scene.

---

## Tunable data (serialized → `SO_ShipTuning`)

| Field            | Default | Notes                                       |
| ---------------- | ------- | ------------------------------------------- |
| `MaxSpeed`       | 8 m/s   |                                             |
| `Acceleration`   | 4 m/s²  | Approach `MaxSpeed` from current speed      |
| `ReverseFactor`  | 0.3     | Multiplier on `MaxSpeed` for negative throttle |
| `YawRateDeg`     | 60 °/s  | At full steering input                      |
| `SteeringResponseLerp` | 6 | Per-second smoothing of steering input    |

---

## Movement model

- `currentSpeed` lerps toward `MaxSpeed * throttle` at `Acceleration`.
- `transform.position += transform.forward * currentSpeed * Time.deltaTime`.
- `transform.Rotate(Vector3.up, YawRateDeg * smoothedSteering * Time.deltaTime)`.
- Y is locked at the sea-surface height (constant in v0.2).

No forces, no `MovePosition`, no physics queries. The root keeps yaw only, so `forward` and `Rotate` stay on the plane.

---

## Riding the waves (`WaveRider`)

The ship stays kinematic and rides the water without physics (`plans/05_from_guide`, Q1).

- `PF_Ship`'s root carries `ShipController`, `WaveRider` and a kinematic `Rigidbody`; a `Hull` child carries the mesh, the box collider and a `RippleSource`.
- Each `LateUpdate`, `WaveRider` samples the water (`WaterSurface.HeightAt`, the same waves the shader draws) at the bow, stern, port and starboard, `_halfLength` and `_halfBeam` from the centre, and eases the hull's height, pitch and roll toward them at `_response` per second; `Start` places it at once. `_waterlineOffset` raises or sinks the hull's centre against the mean surface.
- The camera follows the root, so it does not bob; the save reads the root, so saves are unchanged.
- The kinematic `Rigidbody` makes the hull a moving collider: floating props are pushed out of it rather than passing through. The depth bake passes through it, as through anything on a Rigidbody.
- The water is wired on the scene's instance (`_water` on `WaveRider`, `_water` and `_ripples` on the hull's `RippleSource`).
- `WaveRiderTests` (EditMode): level at the sea on flat water, nose up where the water rises toward the bow, roll with crests along the hull, eased after the first step.

---

## Public interface

```
void SetInput(float throttle, float steering)   // [-1, 1] each
void Teleport(Vector3 position, float yawDeg)   // used by SaveSystem.Load
ShipState GetState()                            // position + yaw
```

---

## Interactions

### Uses
- `InputManager.GetMovementInput()` (inside `Update`)

### Used by
- CameraController (reads transform)
- WaveRider (reads the root's position and heading)
- SaveSystem (reads `GetState`, calls `Teleport` on load)

---

## Constraints

- No collision handling with the island in v0.2: the ship can sail onto it.
- No buoyancy forces: the wave response is the hull's visual ride (`WaveRider`).
- No allocations per frame.
- No camera, audio, or UI calls.

---

## Failure handling

- NaN/Infinity in input → clamp to 0 and log warning.
- Position drifts beyond ±10 km from origin → re-center logged, no auto-action in v0.2.
