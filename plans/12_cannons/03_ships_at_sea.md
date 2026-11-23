---
status: done
---

# Phase 03 - Ships at sea

## Overview

A second brig under sail, circling (Q7), and sunk ships other than the player's coming back (Q8).
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. `CircleHelm`: steers a ship's `ShipController` along a circle through `SetInput`.
2. `ShipController` no longer assumed to be the player's: every moving ship runs aground, the player's is still the one wired and saved.
3. `ShipRespawn`: a sunk ship comes back whole at its start, 60 s later, when the player is more than 300 m from that start.

## Plan

- **`CircleHelm`** (`Game.Gameplay`): a centre, a radius (45 m, Q7 as corrected), a turn (starboard), a throttle (0.5). Each frame it heads for a point a look-ahead along the circle from the ship's nearest point on it, steering by the heading error, clamped; a ram or a drift puts it back. Stops steering while its ship sinks.
- **`ShipController`**: its summary and the system page stop calling it the player's. `GameSceneWiring` still injects input into, registers and follows its own `_ship`; a ship with no input holds what its helm set.
- **Ground for every moving ship**: `GameSceneWiring` hands the world to each `ShipRespawn`, which wires `UseGround` on every copy it makes; the player's stays as is.
- **`ShipRespawn`** (`Game.Gameplay`), on a spawner in the Game scene per ship that comes back: the scene's ship instance is its template, kept inactive; at start it makes a copy at the template's pose (references to the water, ripples and `Cannonballs` carried by `Instantiate`), moved into the spawner's scene, since `Instantiate` puts it in the active scene. On the copy's `Sunk`, it waits `_delay` (60 s) and until the player's ship is beyond `_clearance` (300 m) from the start, then destroys the sunk copy and makes a new one. Fallen masts are destroyed objects, so a new copy is the only whole ship.
- **The scene**, by a throwaway editor script: the anchored brig made a template under a `ShipRespawn`; a second `PF_Ship_Brig` showing its `Mast<i>Sailing` states, with `ShipController`, `CircleHelm`, side `Pirates`, under its own `ShipRespawn`; the circle inside the start clearing, the only water clear in every world: centre (70, -70), radius 45.
- **Tests**: EditMode: the helm converging onto the circle from inside and outside, round the way it is told, back on it after a push. PlayMode: the respawn waiting for the delay and the distance, a whole copy at the start (`Destroy` needs play mode); the circling brig keeps a circle for 60 s without running aground.

## Out of scope

- Ships that choose where to sail or chase; saving other ships.

## Done when

- Tests pass; gates green; a capture of the circling brig under sail.

## What the implementation found

- **The circle could not be 350 m out** (Q7 a): the islands change with the seed of each new game, and only the start clearing (160 m) is water in every world. The circle is inside it, 90 m across; Q7 in `00_start.md` carries the correction.
- **Six PlayMode tests found "the" ship by type**, and a sailing brig is a second `ShipController`. `GameManager.PlayerShip` (the ship it registers and saves) is what they ask for now.
- **The brig prefab's default is anchored** (sails furled), so the sailing brig's instance shows its `Mast<i>Sailing` states.
- **The sails are trimmed to one wind** (the builder's), so on half the circle they are set for a wind from the wrong side. Left: a ship whose sails follow its heading belongs with ships that choose where to sail.
- Copying the anchored brig's scene references onto the new instance with `SetPropertyModifications` dropped the instance's own name and pose (not yet recorded); set again and recorded.
- A sinking test helper: the sink time is a private field of the type; the respawn test sets it by reflection, as `ShipControllerTests` sets the type.
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 250/250, PlayMode 26/26.
