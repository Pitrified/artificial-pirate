---
status: done
---

# Phase 03 - Chase and stern guns

## Overview

`Broadside` knows two sides. Batteries by bearing, so a type can carry guns that fire ahead and astern (Q3 a: the frigate and the giant). The batteries come before the frigate so it is built with them; until then they are tested on a type made in the test.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. A battery is a bearing off the bow, an arc and its guns; the broadsides are two of them, as they behave now.
2. A type may carry bow and stern batteries: their muzzles, arcs and barrels from the builder.
3. The sloop and the brig fight as before.

## Plan

- **`Broadside`**: batteries in place of `_starboard` and `_port`, each with its bearing (90, 270, 0, 180), its arc and its range of the barrels; `VolleyFired` carries the battery. The targeting (nearest within each battery's arc), the volley, the reload, the barrels' aim held within the battery's arc, all per battery. No allocation per frame.
- **`SO_ShipType`**: the chasers' muzzles (bow and stern) and their arcs beside the broadside's `Guns`; none on the sloop and the brig.
- **Builder**: moved to [`04_frigate.md`](04_frigate.md), the first type that carries chasers, where the ports can be seen (corrected in the review before this phase).
- **Shake and effects**: as for a broadside volley.
- **Tests**: `BroadsideTests` extended: a bow battery bears on a ship ahead within its arc and not on the beam; a stern battery astern; a type with no chasers unchanged; `FightTests` unchanged and green.

## Out of scope

- Chasers on the sloop or the brig.

## Done when

- Tests pass; gates green; the sloop and brig fight as in the last phone run.

## What the implementation found
- **The builder's ports moved to the frigate**: no type here carries chasers, so bow and stern ports built now could not be seen or checked against a model; phase 04 builds them with the frigate.
- `Battery` (Starboard, Port, Bow, Stern) is the public name; `Broadside` keeps four of them with a bearing off the bow (90, 270, 0, 180), its arc (`SO_Gunnery.ArcDeg` for the broadsides, the new `ChaseArcDeg`, 30 degrees, for the chasers), its first barrel and count. `SO_ShipType.Gun(Battery, i)` and `GunCount(Battery)` replace `Gun(i, starboard)`; `SetChasers` sets the bow and stern muzzles. `VolleyFired` and `TargetOf` take the battery.
- A barrel's rest is now its axis from the trunnions to its mouth, so a chaser's barrel pointing ahead or astern rests as a broadside's does.
- Tests: chasers bear ahead and stern guns astern (and not on the beam), no chasers and nothing bears ahead or astern, a chaser volley hits the ship ahead; the model tests check every battery's barrels and mouths.
- Gates: spec pass, compile 0/0, EditMode 279/279, PlayMode 31/31 (`FightTests` unchanged).
