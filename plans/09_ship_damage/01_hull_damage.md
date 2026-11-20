---
status: done
---

# Phase 01 - Hull damage and handling

## Overview

The core: a ship's hull health, the hits that lower it, and what a damaged ship does on the water.
Everything later reads this.
Context: [`00_start.md`](00_start.md).

## Goals

1. Every ship with a `HullContact` has a `ShipDamage` with hull health from 1 to 0.
2. Rams hurt both ships and groundings hurt the ship that ran aground.
3. Damage lowers the top speed, sits the ship lower and lists it.

## Plan

- `ShipDamage` (Gameplay) on the ship's root:
  - `Health` (0 to 1), `Hit(Vector3 worldPoint, float amount)`, an event `Damaged(point, amount)`;
  - hits are kept in the hull's local space (point and amount), a fixed array, for the marks and the save.
- The amounts, in the ship type (`SO_ShipType`) so a bigger hull takes more:
  - a ram: the energy the ship takes, `0.5 * mass * closing^2 * share`, over the hull's strength, below a threshold nothing;
  - a grounding: the speed at impact squared, the same way.
  `HullContact.Collide` reports each ram's closing speed and point to both ships' `ShipDamage`; `ShipController` reports the speed when it runs aground, once per grounding, not every frame it pushes against the shore.
- What damage does, each numbers in the ship type:
  - `ShipController`'s top speed is scaled from 1 at full health down to a floor that keeps the ship playable (phase 05 also takes a share per lost mast, above the same floor);
  - `WaveRider` takes an extra sink, down to a limit near the deck edge, and a list toward the side the hits were on.
- The brig and the sloop prefabs get `ShipDamage` through a throwaway editor script.
- Tests (EditMode): a ram hurts both by mass share, a gentle nudge does nothing, a grounding hurts once, health never below 0, the top speed, sink and list follow health; a hit at a point is kept in hull space.
- `docs/systems/ship-controller.md`: a damage section.

## Out of scope

- Sinking at 0 (phase 02), the HUD and the save (03), marks (04), masts (05).

## Done when

- The tests pass; the gates pass; a capture of a damaged sloop sitting low and listing.

## What the implementation found
- 2026-11-20. Review before the code: holding the throttle against a shore or a hull rebuilds only one frame's acceleration of closing speed (about 0.08 m/s), below the harmless speed, so "once per grounding" needs no state of its own; the threshold does it.
- A blow costs `share * (v^2 - h^2) / s^2`, `share` being the other's share of the two masses (1 against a shore), the same share `HullContact` already used for the speeds. So the damage numbers are speeds (`HarmlessSpeed` 1.5, `SinkingSpeed` 14 m/s), not energies: a full-speed grounding takes about 31%, the sloop ramming the brig at 8 m/s about 25% and the brig about 7%.
- `HullContact` raises `Struck` on both hulls of a ram and, through `RunAground`, on a grounding; the point is the closest point on the other hull (`Physics.ClosestPoint` with its pose, no transform sync needed).
- The list needs only the damage taken on each side, two numbers; keeping each hit's point is left to phase 03 (the save) and 04 (the marks), where something reads them.
- The sink is a share of the type's freeboard (0.5), so no per-type number; the type's other damage numbers are defaults shared by the sloop and the brig, the mass doing the rest.
- `ShipDamage` on `PF_Ship`'s root, wired to the controller and the rider, by a throwaway editor script (deleted); the variants and the scene's brig inherit it.
- The sequence capture takes `CAPTURE_DAMAGE`: whole against 90% damage from the chase camera, the sloop listing to starboard and lower in the water.
- Gates: compile 0 errors 0 warnings, EditMode 202/202, PlayMode 22/22.
