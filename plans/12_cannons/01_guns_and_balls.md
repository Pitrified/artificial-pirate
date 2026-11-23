---
status: in progress
---

# Phase 01 - Guns and balls

## Overview

The working core: a ship fires its broadside at a ship of another side in its arc and range, the balls fly, and a hit takes health where it lands. Plain balls, no effects, so the rules can be tested on their own.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. Every ship type knows where its guns are, from the builder, checked against the model.
2. `Broadside` on `PF_Ship`'s root: a side, two batteries, volleys within the arc and range, aimed with a lead and a spread.
3. One ball pool in the Game scene: balls move on their arc, a hull hit calls `ShipDamage.Hit` at the point, a miss ends in the water or on land.
4. The sloop and the anchored brig fight in the Game scene.

## Plan

- **The guns' places.** `SO_ShipType` gains `_guns`: the starboard muzzles in the hull's local space (Unity axes), the port side mirrored; and `_ballDamage`, `_hullStrength` (Q3: 0.06 both, strength 1 and 1.6). The builder prints the muzzles (`build.py <type> --guns`, from `gun_port_centres` and `BARREL_OUT`) and a throwaway editor script writes them into `SO_Ship_Sloop` and `SO_Ship_Brig`. `BuiltModelTests`: each muzzle has a hull vertex within a few centimetres (the barrel's mouth).
- **`SO_Gunnery`** in `Assets/Settings/`, shared by every ship: range 90 m, arc 20 degrees, muzzle speed 60 m/s, the spread at full range, 0.12 s between guns, reload 5 s, how often a battery looks for a target (0.25 s), the ball's radius.
- **`Broadside`** (`Game.Gameplay`), next to `ShipDamage`:
  - its side (an enum, `ShipSide`: `Player`, `Pirates`, room for more), its `ShipDamage`, `HullContact` and the scene's `Cannonballs`;
  - each battery, every look: the nearest hull of another side within range and within the arc of its beam, by `Physics.OverlapSphereNonAlloc` on the `Ship` layer and the hull's `Broadside` (`GetComponentInParent`, no allocation); none while its own ship sinks;
  - loaded and with a target: a volley, one gun every gap, then the reload; a target lost mid volley stops it;
  - each gun's ball: aimed at a point along the target's hull at mid-freeboard, led by the target's velocity over the flight time (each `Broadside` keeps its root's velocity from the change in position, which covers a sailing ship and a drifting anchored one alike), solved for the low arc at the muzzle speed; the yaw clamped to the arc; a random spread (seeded) growing with the distance;
  - `Fired(Vector3 muzzle, Vector3 direction)` for the effects and the camera.
- **`Cannonballs`** (`Game.Gameplay`), one in the Game scene: a fixed pool (32) of sphere renderers, one material, no colliders. Each frame each live ball moves on its arc; a `Physics.RaycastNonAlloc` from the last position to the new against the `Ship` layer, the nearest hit that is not the firer's hull (a muzzle amidships can sit inside its own hull's box, which a plain linecast would hit first); then the water (`WaterSurface.HeightAt`) and the ground (the world's `HeightAt`).
  - On a hull: `ShipDamage.Hit(point, firer.BallDamage / target.HullStrength)`; `HitHull(point, normal)`.
  - On the water: the ripple ring (`WaterRipples.AddImpulse`); `HitWater(point)`.
  - On land: `HitGround(point)`.
  - A ball out of balls is skipped and logged once. No allocation per frame (`AllocationScan` in a test).
- **The scene**, by a throwaway editor script: `Cannonballs` with the water, ripples and world; `Broadside` on `PF_Ship` with `SO_Gunnery`; the sloop's side `Player`, the brig's `Pirates`; the `Cannonballs` reference on each instance.
- **Tests (EditMode)**: the arc (a ship on the beam is a target, one ahead is not, the 20 degrees both ways), the range, the own side never a target, the nearest chosen; a volley's gaps and the reload; the solved arc hitting a still target with no spread, and a moving one with the lead; a ball's hull hit at the point with the damage scaled, its own hull skipped; a ball into the water; no allocation per frame; the guns' places against the model.

## Out of scope

- Effects, smoke, the camera, the health bars, sailing ships, respawn, moving barrels.

## Done when

- The tests above pass; gates green (spec, compile 0/0, EditMode, PlayMode).
- A capture of the sloop beside the brig shows balls in flight and hull marks appearing on the side hit.

## What the implementation found
