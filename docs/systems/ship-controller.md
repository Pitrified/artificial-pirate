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

- Component on the `PF_Ship` prefab; each kind of ship is a variant of it (`PF_Ship_Sloop`).
- Lives only inside the Game scene.

---

## Ship types (`SO_ShipType`)

A ship's kind is an `SO_ShipType` in `Assets/Settings/` (`SO_Ship_Sloop`), set on its prefab variant's `ShipController`, `WaveRider`, `WakeSource` and `HullContact`.
It holds:
- **the hull**: half length and half beam at the waterline, draft and freeboard. They are the model's numbers (`Tools/asset_builders/builders/<type>.py`), and `BuiltModelTests` checks them against the imported model. The variant's box collider is sized from them.
- **the handling**: the fields below;
- **the camera's offset**, handed to the camera by `GameSceneWiring`.

A new type is a model from the ship builder (`docs/building_assets.md`), an `SO_ShipType`, and a variant of `PF_Ship` carrying both.
A variant whose paint differs overrides the shared slots on its renderer, as `PF_Ship_Brig` does with `M_BrigTopsides` and `M_BrigTrim`.
`ShipController` is the player's: the Game scene has exactly one, which `GameSceneWiring` wires and `GameManager` saves. A ship nobody steers has its instance's `ShipController` removed, as the Game scene's `AnchoredBrig` does; it rides the waves where it lies, and keeps its `HullContact`, so it can be rammed.

## Tunable data (serialized → `SO_ShipType`)

| Field            | Default | Notes                                       |
| ---------------- | ------- | ------------------------------------------- |
| `MaxSpeed`       | 8 m/s   |                                             |
| `Acceleration`   | 4 m/s²  | Approach `MaxSpeed` from current speed      |
| `ReverseFactor`  | 0.3     | Multiplier on `MaxSpeed` for negative throttle |
| `YawRateDeg`     | 60 °/s  | At full steering input; the sloop has 30    |
| `SteeringResponseLerp` | 6 | Per-second smoothing of steering input    |

---

## Movement model

- `currentSpeed` lerps toward `MaxSpeed * throttle` at `Acceleration`.
- `transform.position += transform.forward * currentSpeed * Time.deltaTime`.
- `transform.Rotate(Vector3.up, YawRateDeg * smoothedSteering * Time.deltaTime)`.
- Y is locked at the sea-surface height (constant in v0.2).

No forces, no `MovePosition`. The root keeps yaw only, so `forward` and `Rotate` stay on the plane.
After each step the ship's `HullContact` resolves its hull against other ships' (below), and the speed becomes the part of the returned velocity along `forward`.

## Ship against ship (`HullContact`)

Kinematic bodies never collide with each other in PhysX, so ships meet through a query instead.

- Every ship's root carries a `HullContact`: its type, its hull's `BoxCollider` and the `Ship` layer (6), which holds the hulls and nothing else.
- **Mass** is the displacement from the type: length x beam x draft x a block coefficient of 0.45 x the water's density. The sloop is about 136 t and the brig about 505 t.
- **`Collide(velocity)`**, called by `ShipController.Tick` after it moves:
  - `Physics.OverlapBoxNonAlloc` around its own hull against the `Ship` layer;
  - for each other hull, `Physics.ComputePenetration` from its hull's pose as just moved, flattened to the water plane, and the root moves out by it.
- **A ram** shares the closing speed along the contact normal by mass, as two hulls that stay together. The rammer keeps `v * m1 / (m1 + m2)` along the normal, and the other ship takes the same as a **drift**. A hull with no `HullContact` cannot be moved and takes it all.
  - Held at full throttle, the sloop goes on shoving the brig slowly.
  - Moving away, or sliding along a side, costs no speed.
- **The drift** moves the root each frame and fades at `_driftDamping` (0.5 per second), so a drift of `v` glides about `2v` metres.
- The push is along the water only: a pushed ship drifts, it does not turn. A drifting ship does not check for hulls itself.
- Each ram is a **blow** to both hulls (`Struck`: the point, the closing speed, and each hull's share, the other's share of the two masses); running aground (`RunAground`) is a blow at the leading end with a share of 1.
- `HullContactTests` (EditMode): a ram shared by mass with both hulls ending apart, a heavier ship pushed less, a hull that cannot move stopping the rammer, backing away and sliding at full speed, a crate on another layer ignored, a drift fading to rest, and a held throttle shoving the other ship without passing through it.

## Damage (`ShipDamage`)

- Every ship's root carries a `ShipDamage` wired to its `HullContact`: the hull's health, from 1 (whole) to 0.
- **A blow** closing at `v`, of which the hull takes `share`, costs `share * (v^2 - h^2) / s^2` of health, nothing below `h`. The type sets `h` (`HarmlessSpeed`, 1.5 m/s) and `s` (`SinkingSpeed`, 14 m/s: a blow against a shore at that speed ruins the hull).
  - A full-speed grounding (8 m/s) takes about 31%.
  - The sloop ramming the brig at 8 m/s takes about 25% and the brig about 7%.
  - Pushing on against a shore or a hull rebuilds only a frame's acceleration of closing speed, below `h`, so a blow lands once.
- **`Hit(point, amount)`** takes health directly: what a weapon will call. `Damaged` reports each hit that did harm.
- **What damage does**, by the type's numbers:
  - `ShipController`'s top speed is `MaxSpeed` times `SpeedFactor`, from 1 down to `DamagedSpeedFloor` (0.6) when ruined;
  - `WaveRider` settles the hull by `Settle`, up to `DamagedSinkShare` (0.5) of the freeboard;
  - and lists it toward the side the damage landed on, `ListDeg`: `DamagedListDeg` (8 degrees) times the starboard damage less the port damage.
- **Sinking.** At 0 health the ship no longer answers the helm (`ShipController` zeroes its input) and goes under over `SinkSeconds` (8 s): `WaveRider` lowers the hull by `SinkDepth`, slowly at first (30 m at the end), and tips it toward the end last hit, up to `SinkPitchDeg` (25 degrees). Then `Sunk` fires and the ship's object is turned off, so its hull no longer collides. For the player's ship that is game over (`docs/systems/game-manager.md`); the brig is gone until the scene loads again.
- `ShipDamageTests` (EditMode): a ram shared by mass, a nudge doing no harm, one blow for a grounding with the throttle held, health stopping at 0, the speed, sink and list from the damage, a nearly ruined hull sailing near the floor speed, a ruined one ignoring the helm, going under by the end hit and sinking once. `GameOverTests` (PlayMode): the player's ship sinking to the game over screen and back to the menu with no save. `WaveRiderTests`: a damaged hull settled lower and listing toward the side hit.

---

## Riding the waves (`WaveRider`)

The ship stays kinematic and rides the water without physics (`plans/05_from_guide`, Q1).

- `PF_Ship`'s root carries `ShipController`, `WaveRider`, a `WakeSource` (its wake, `docs/systems/water.md`), a `HullContact`, a `ShipDamage` and a kinematic `Rigidbody`; a `Hull` child, on the `Ship` layer, carries the box collider and, on a variant, the model.
- Each `LateUpdate`, `WaveRider` samples the water (`WaterSurface.HeightAt`, the same waves the shader draws) at the bow, stern, port and starboard, the type's half length and half beam from the centre, and eases the hull's height, pitch and roll toward them at `_response` per second; `Start` places it at once. `_waterlineOffset` raises or sinks the hull's centre against the mean surface.
- The camera follows the root, so it does not bob; the save reads the root, so saves are unchanged.
- The kinematic `Rigidbody` makes the hull a moving collider: floating props are pushed out of it rather than passing through. The depth bake passes through it, as through anything on a Rigidbody.
- The water is wired on the scene's instance (`_water` on `WaveRider`, `_ripples` on `WakeSource`); the water's `WaterRipples` follows the ship.
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

- The ship stops where its leading end would be over ground shallower than its draft (`HullContact.RunAground`, the world's `HeightAt`): the hull takes the blow, the step is undone and the speed drops to 0. No slide along the shore.
- No buoyancy forces: the wave response is the hull's visual ride (`WaveRider`).
- No allocations per frame.
- No camera, audio, or UI calls.

---

## Failure handling

- NaN/Infinity in input → clamp to 0 and log warning.
- Position drifts beyond ±10 km from origin → re-center logged, no auto-action in v0.2.
