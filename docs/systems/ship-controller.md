# 📄 `docs/systems/ship-controller.md`

## Overview

A ship using a **kinematic** model, the player's or another's - the Transform is moved directly,
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
- **the guns**: the starboard muzzles, a ball's damage and the hull's strength against balls (`docs/systems/combat.md`);
  The hull's side along its length is one curve, `_beamAlongLength`: the half-beam at the waterline as a share of the type's, from -1 (stern) to 1 (bow) of the half length, sampled from the builder's `half_beam` at its stations, where the bow's overhang has moved them (`HalfBeamAt(z)`).
  Above the water a type with `_tumblehome` has its side fall in toward the rail, from 0.3 of the freeboard up, as the builder's `tumblehome` (`SideAt(z, height)`); the others' sides are vertical.
  A new type, or a hull reshaped in the builder, gets its curve sampled again; the marks sit on the side at their height.
- **the handling**: the fields below;
- **the cameras' rigs**: the chase's offset and look point, the bird's eye's and the helm's (`Rig(view)`, `docs/systems/camera-controller.md`);
- **the map icon**, its hull from above (`docs/systems/world.md`).

A new type is a model from the ship builder (`docs/building_assets.md`), an `SO_ShipType`, and a variant of `PF_Ship` carrying both.
A variant whose paint differs overrides the shared slots on its renderer, as `PF_Ship_Brig` does with `M_BrigTopsides` and `M_BrigTrim`.
The player's ship is the one `GameSceneWiring` wires to the input and `GameManager` registers and saves (`GameManager.PlayerShip`). Another ship that moves is steered through `SetInput` by a helm (`CircleHelm`, `docs/systems/combat.md`). A ship that does not move has its instance's `ShipController` removed, as the anchored brig does; it rides the waves where it lies, and keeps its `HullContact`, so it can be rammed.

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
- **`Hit(point, amount)`** takes health directly, at the point struck: what a cannonball calls (`docs/systems/combat.md`). `Damaged` reports each hit that did harm.
- **What damage does**, by the type's numbers:
  - `ShipController`'s top speed is `MaxSpeed` times `SpeedFactor`, from 1 down to `DamagedSpeedFloor` (0.6) when ruined, less an equal share of what is above the floor for each mast lost, never below the floor;
  - `WaveRider` settles the hull by `Settle`, up to `DamagedSinkShare` (0.5) of the freeboard;
  - and lists it toward the side the damage landed on, `ListDeg`: `DamagedListDeg` (8 degrees) times the starboard damage less the port damage.
- **The hits kept.** Each hit is kept in the hull's local space (point and amount), the latest `KeptHits` (16), oldest first (`HitCount`, `GetHit`): what the hull's marks are drawn from.
- **Saved.** `ToSaveData` gives the health lost, the damage on each side and the kept hits; `Restore` puts them back, the end that goes down first being the last hit's. The health lost is saved on its own: the sides' sum can round to a hair under 1 for a ruined hull. `GameManager` saves and restores the player's ship this way (`docs/systems/save-system.md`); the brig is not saved and is whole when the scene loads.
- **Marks** (`HullMarks`, on the `Marks` child of `PF_Ship`'s `Hull`, so they ride with it): a quad for each kept hit, on the side it landed on at `HalfBeamAt(z)`, a few centimetres off it and facing out (the side's slope turns it toward the bow or stern), between the waterline and the rail. A hit's size sets the mark's, 1.2 m up to 3.5 m at a quarter of the health; from a tenth up it is a hole, below a splintered patch. The texture is a 2 x 2 atlas from `textures.py hull_marks` (`T_HullMarks`, two patches and two holes), drawn with `M_HullMark` (URP Lit, alpha clip). All the marks are one mesh, rebuilt on `Damaged` and `Restored`, so a damaged ship costs one draw more and a whole one none (its renderer is off). Nothing is cut into the hull.
- **Sails** (`ShipSails`, on `PF_Ship`'s root, its masts set on each variant): at anchor (`_set` off, the anchored brig and the giant) each mast shows its sails furled. Under way each standing mast shows the sailing state set for the band of the wind its heading puts it in: the builder sets every mast's sails for eleven winds off the bow (`SO_SailTrim`: close-hauled at 67.5 degrees, then every 22.5 degrees round to dead astern, on either tack), and the band shown is the nearest, held until another is more than `HysteresisDeg` (5 degrees) nearer, so a ship holding its course does not flicker between two. The flag turns about the masthead to stream downwind. The flag flies on a staff above the tallest masthead, clear of the sails. A broken mast is left alone. The wind is the world's (`docs/systems/world.md`), given by `GameSceneWiring` to the player's ship and through each `ShipRespawn` to the others; it is seen, not sailed.
- **The types**: the sloop (one mast, 6 guns a side), the brig (two masts, 9 a side), the frigate (three masts, a quarterdeck and forecastle, 13 a side on a covered gun deck, two chasers and two stern guns, 44 m on deck) and the giant (three masts, the look of the Amerigo Vespucci, 15 a side on a covered deck under a painted row, two chasers and two stern guns, 82 m on deck). Each is a variant of `PF_Ship` with its `SO_ShipType` (`docs/building_assets.md`).
- **Masts** (`ShipMasts`, on `PF_Ship`'s root, its masts set on each variant): each mast is the model's `<Name>Mast<i>` with its sail states and stump (`docs/building_assets.md`); a `MastParts` holds where it stands (along the hull and its foot, measured from the stump) and the objects that go when it breaks (the mast, its furled sails, every band of its set sails), the flag with the tallest mast. Only the objects showing go over the side.
  - Each hit may bring down the standing mast nearest it. The chance follows the health after the hit, from `MastBreakChanceWhole` (5%) to `MastBreakChanceRuined` (50%), and a hit smaller than `MastBreakingHit` (15% of the health) rolls that share of it. A full-speed grounding from whole leaves the hull at about 70% and breaks a mast about one time in five; a nudge almost never.
  - A broken mast's shown objects go into a `FallingMast` at its foot, which tips them over the side away from the blow (`MastFallDeg`, 80 degrees, in `MastFallSeconds`, 2.5 s, faster as it goes), then sinks them (`MastSinkSpeed`, 1.5 m/s, for `MastSinkSeconds`, 8 s) and removes them. No physics: the ship sails on and leaves it. The stump shows.
  - The save keeps the broken masts' numbers; a restore hides their objects and shows their stumps.
  - Cost: each mast is its own objects, so a two-masted brig draws 7 more slots than with its masts in the hull, the sloop 2 (the scene went from 48 to 57 material slots with both in view).
- **Sinking.** At 0 health `SinkingStarted` fires, the ship no longer answers the helm (`ShipController` zeroes its input) and goes under over `SinkSeconds` (8 s): `WaveRider` lowers the hull by `SinkDepth`, slowly at first (30 m at the end), and tips it toward the end last hit, up to `SinkPitchDeg` (25 degrees). Then `Sunk` fires and the ship's object is turned off, so its hull no longer collides. For the player's ship that is game over (`docs/systems/game-manager.md`); another ship comes back later through its `ShipRespawn` (`docs/systems/combat.md`).
- `ShipDamageTests` (EditMode): a ram shared by mass, a nudge doing no harm, one blow for a grounding with the throttle held, health stopping at 0, the speed, sink and list from the damage, a nearly ruined hull sailing near the floor speed, a ruined one ignoring the helm, going under by the end hit and sinking once, the hits kept in the hull's space with the oldest dropped, and a save restored to the same hull (a ruined one going under again). `SaveRestoreTests` (PlayMode): the damage and the hull bar back on Continue. `ShipMastsTests` (EditMode): the nearest standing mast brought down, falling away from the blow and sinking, the chance rising as the health drops and less for a small hit, the speed's share per mast and the floor, the broken masts restored. `BuiltModelTests`: each mast's four objects, and the hull's side along its length matching the type. `HullMarksTests` (EditMode): a mark on the side hit, on the surface and facing out, bigger for a bigger hit, following the kept hits, drawn on a restore, and on a tapering hull sitting on the narrower side, turned toward the bow. `GameOverTests` (PlayMode): the player's ship sinking to the game over screen and back to the menu with no save. `WaveRiderTests`: a damaged hull settled lower and listing toward the side hit.

---

## Riding the waves (`WaveRider`)

The ship stays kinematic and rides the water without physics, as movement is kinematic across the game ([locked decisions](../functional-specs.md#locked-decisions)).

- `PF_Ship`'s root carries `ShipController`, `WaveRider`, a `WakeSource` (its wake, `docs/systems/water.md`), a `HullContact`, a `ShipDamage`, a `ShipMasts` and a kinematic `Rigidbody`; a `Hull` child, on the `Ship` layer, carries the box collider and, on a variant, the model.
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
