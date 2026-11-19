---
status: done
priority: 0
description: |
  Ships collide with each other: a ship sailed into another stops against its hull and can
  slide along it or back away, instead of passing through.
---

# Ships collide

Asked 2026-11-19 by the user: "tiny tweak to the ships. ships should collide with each other. plan".

## Where it stands

- `ShipController` moves the root's transform directly: a scalar speed along `forward` and a yaw rate. It makes no physics queries.
- `PF_Ship`'s root has a kinematic `Rigidbody`. Its `Hull` child carries a `BoxCollider` sized from the `SO_ShipType` (half length, half beam, draft and freeboard), and `WaveRider` heaves, pitches and rolls that child.
- Two kinematic bodies never collide in PhysX. So the sloop sails through the scene's `AnchoredBrig`. Floating barrels and crates (dynamic, `Buoyancy`) are pushed out of either hull, as they should be.
- Every object is on the Default layer. `DynamicsManager` has `m_AutoSyncTransforms` 0.
- Only the player's ship moves today. The brig lies at anchor.

## Plan

Asked by the user on the first draft: "if you ram into the other ship, it should be pushed a bit, you decelerate".

Keep the ships kinematic. Each moving ship resolves its own overlap after it moves, and a ram shares its speed with the ship it hits, by mass, as two hulls that stay together (an inelastic collision). No dynamic bodies: the handling stays as it is until two hulls touch.

1. **A `Ship` layer** (user layer 6) on `PF_Ship`'s `Hull`. The variants and the scene's brig inherit it. An editor script makes the change, deleted afterwards.
2. **`HullContact`**, a new component on `PF_Ship`'s root, on every ship: the player's, and also the anchored brig, which has no `ShipController`.
   - It holds the hull's collider and a **mass**: the displacement from the type's size (length x beam x draft x a block coefficient x the water's density). It is computed, not a new knob. The brig comes out a few times the sloop.
   - It holds a **drift** velocity on the water plane, from being rammed. The drift moves the root each frame and fades under the water's drag over a few seconds, so a pushed ship glides a few metres and stops.
   - `Push(velocity)` adds to the drift.
3. **`ShipController.Tick`, after the move**:
   - `Physics.OverlapBoxNonAlloc` around its own hull, against the `Ship` layer only, into a small buffer allocated once. Its own collider is skipped.
   - For each other hull, `Physics.ComputePenetration` with its own hull's pose as just moved, flattened to the water plane, and the root moves out by it.
   - **The ram**: the closing speed `v` along the contact normal is shared by mass, `v' = v * m1 / (m1 + m2)`. The other ship gets `Push(normal * v')`, and the rammer's speed drops by what it lost along the normal.
     - A sloop ramming the brig slows hard, and the brig moves a little.
     - Kept at full throttle, the sloop goes on shoving the brig slowly, each frame being another small ram.
     - A glancing touch has little closing speed: it slides along the side and costs little speed.
   - The ship's own drift (if it is rammed later) is part of its velocity in the same sum.
4. **Tests**, EditMode:
   - head-on into a hull on the `Ship` layer: the rammer ends outside it, slower but not stopped, and the other ship drifts away from it;
   - a heavier ship is pushed less, and its rammer loses more speed;
   - the drift fades to rest;
   - at an angle, the rammer slides along the other ship;
   - a box on the Default layer (a crate) changes nothing;
   - backing away from contact works.
5. **Docs**: `docs/systems/ship-controller.md`. Remove "no physics queries" and the line on unsteered ships, and describe the contact, the mass and the drift.
6. **Gates**, then a capture of the sloop against the brig, and a commit. The cost is one overlap query a frame per moving ship, so no new APK is needed unless asked.

## Notes

- **Hull shape.** The collider is a box, so where two bows meet, their corners touch before the hulls do: a gap of up to about a metre at the ends. If that shows, the fix is a convex collider from a simplified hull, written out by the builder.
- **The push is along the water only.** A pushed ship drifts, it does not turn: a ram on the bow does not spin it. Spin would need its moment of inertia, and that belongs with real sailing physics.
- **A pushed ship's drift does not check for hulls itself**, so a third ship in its way would be passed through. Today there are only two ships.
- **The island** is not part of this: the ship still sails onto it (the docs' v0.2 constraint).

## Answers

The user: "No anchor pull. Ram no sound no shake. Go".

## What the implementation found
- 2026-11-19. As planned: the `Ship` layer (6), `HullContact` on `PF_Ship`'s root with its type set per variant, and `ShipController.Tick` calling it after the move. The masses: sloop about 136 t, brig about 505 t.
- **A slow shove handed over too little to keep.** At full throttle against the brig, each frame gives it under 1 cm/s, and the drift's rest threshold (then 1 cm/s) zeroed it every frame: the brig moved 0.1 m in 10 s, where a step-by-step simulation of the same rule gives about 8 m. The threshold is now 1 mm/s, and the brig is shoved.
- `TagManager` was reserialized by Unity on save (`serializedVersion` 3, the empty rendering layer names dropped). That is Unity's own format change.
- `SequenceCaptureTests` gained `CAPTURE_START` ("x,z,yaw") so a capture can aim the ship at something.
- **Capture**: the sloop from 40 m off the brig's beam at full throttle, side and top views. It stops against the brig's side without passing through, and keeps shoving it slowly sideways, its bow at the brig's side.
- **Gates**: compile 0 warnings, EditMode 154/154 (8 new in `HullContactTests`), PlayMode 19/19. No APK: one overlap query a frame for the player's ship.
