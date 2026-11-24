---
status: planned
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
- **Builder**: `gun_ports` gets bow and stern ports as a parameter (`chase_ports`, `stern_ports`: count and height), the barrels `<Name>GunBow<i>` and `<Name>GunStern<i>` turning about their trunnions, the report printing their muzzles.
- **Shake and effects**: as for a broadside volley.
- **Tests**: `BroadsideTests` extended: a bow battery bears on a ship ahead within its arc and not on the beam; a stern battery astern; a type with no chasers unchanged; `FightTests` unchanged and green.

## Out of scope

- Chasers on the sloop or the brig.

## Done when

- Tests pass; gates green; the sloop and brig fight as in the last phone run.

## What the implementation found
