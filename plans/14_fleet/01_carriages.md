---
status: done
---

# Phase 01 - Carriages

## Overview

The guns' base is a box, plain from the helm. The cheapest phase and builder-only, so it goes first; every later type gets the carriages for free.
Context: [`00_start.md`](00_start.md).

## Goals

1. A truck carriage under every gun: two cheeks stepped down toward the breech, a front and a rear axle, four trucks.
2. A barrel that reads as a cannon: a breech knob and a ring or two, still turning about its trunnions.

## Plan

- `props.cannons`: the carriage from parts (cheeks, axles, trucks), its trunnion height under the barrel's trunnions, in the hull's mesh, the palette's wood; sized from the barrel, not per type.
- `props.barrels`: a breech knob and reinforcing rings on the pole; the mouth stays where `muzzle_at` puts it, so `Guns` and `BuiltModelTests` hold.
- A probe of a gun close up, and a capture from the helm of the sloop and the brig.
- Rebuild Sloop.fbx and Brig.fbx; the triangles before and after in the log (a few hundred a gun).

## Out of scope

- How the guns aim or fire.

## Done when

- `BuiltModelTests` green; gates green; the capture from the helm shows carriages, read by the user.

## What the implementation found
- **The ports were too low for a carriage**: their centres were at a fixed height over the water, 0.3 to 0.45 m over the deck, so the barrel lay almost on the planks with no room for trucks. `gun_ports.above_deck` replaces `z`: each port's centre is that far over the deck at its station (the sloop 0.55 m, the brig 0.65 m), so the ports follow the sheer and the guns' muzzles moved up with them (the types' `Guns` set by a throwaway editor script). The ports and their lids stay inside the bulwark.
- The carriage: two cheeks in three steps, a bed, two axletrees and four eight-sided trucks; the barrel ten-sided with three rings, a knob and trunnion pins. About 390 triangles a gun: the sloop from 9,554 to 14,210, the brig from 16,450 to 23,434. A giant with two decks of guns would carry tens of thousands on them alone; its lower deck may need a lighter gun (phase 05).
- The first frame of a sequence capture showed the brig's colours wrong (dark sails, a pale hull); the next frames are right. A capture's first frame is not to be read.
- Gates: spec pass, compile 0/0, EditMode 263/263, PlayMode 31/31.
