---
status: done
---

# Phase 05 - The giant

## Overview

A three-master with the Amerigo Vespucci's size and look (Q5 b), a pirate at anchor in a wider start clearing, and the player's in development builds (Q6 a).
Context: [`00_start.md`](00_start.md), Decisions; depends on [`04_frigate.md`](04_frigate.md).

## Goals

1. The type: the Vespucci's particulars (checked against a source), its long black hull with two white bands along the rows of ports, a raised poop and forecastle, a figurehead, square sails on all three masts and a spanker.
2. A second row of gun ports, the lower row's guns not aimed (part of the hull, or closed ports) if the draw calls ask for it.
3. At anchor in the Game scene inside a wider start clearing, on water deep enough for its draft, coming back when sunk like the brigs; in the ship choice.

## Plan

- **Reference**: the Vespucci's particulars and a profile or sail plan we may keep; main proportions only.
- **Builder**: bands of colour along the hull as a parameter (the white bands), a second row of ports, a figurehead as a simple part; the rest from the frigate's parts.
- **Unity**: as the frigate (FBX, variant, type, map icon, ship choice, a player's giant inactive in the scene), and an `AnchoredGiant` with its `ShipRespawn`.
- **Its place**: the start clearing widened (`SO_WorldSettings.StartClearing`, 160 m now) so the giant anchors inside it with water to swing and fight round it, and islands start further from the origin; the radius set from the giant's length and the anchorage, the seabed there checked against its draft. `WorldGeneratorTests` already checks the clearing is open water for several seeds.
- **Tuning**: slow, wide turning; strength so the frigate needs many volleys; bar height, camera rigs, the minimap icon's length.
- **Tests**: `BuiltModelTests`; a PlayMode check that the anchored giant floats clear of ground in a few seeds.
- **Capture**: the giant at anchor from the sloop's chase, and its three views as the player's.

## Out of scope

- Islands made for ships this size (Q6: a note for the spec's Next).
- The giant under way as an enemy.

## Done when

- Tests pass; gates green; the budget measured with the giant in view; the captures read.

## What the implementation found
- **Reference**: the Amerigo Vespucci's particulars (Wikipedia: 82.4 m between perpendiculars, 100.5 m overall, 15.56 m beam, 7.3 m draft, masts 50, 54 and 43 m) and a public-domain broadside photograph (New York, 1976), kept as `references/giant/vespucci_broadside.jpg`. The model is 82.4 m on deck and 104.5 m overall, its rail 10 m over the water.
- **The look**: `stripes` paints bands of the trim along the side (two white bands, `M_GiantTrim`, a white copy of the brig's trim); the black topsides; a row of ports with `guns: false` painted in the lower band; the armed row of 15 a side in the upper band on a covered deck; two chasers and two stern guns: 34 barrels, the lower row costing no objects.
- **The rig took the longest**: the brig's rig scaled above each mast's deck. With the rail so high the main shrouds splay wide, and the main course met them at every brace that keeps it full; it is left off (clewed up), as brigs often sail. The first jib's stay crossed the fore topgallant on the run; a search over 8 layouts of its head, its tack and the fore mast's place found its head at 38.5 to 39.5 m clear (39.5 is used). The masts stand nearly upright (rake 1 to 2 degrees). Tried and dropped: narrower tops, wider shroud spacing, flatter sails (which put the sails aback sooner), shorter yards; none is kept in the builder.
- **The start clearing is 340 m** (`SO_WorldSettings`), and the giant lies at anchor at (-150, 0, 200), yawed 30 degrees, with its own `ShipRespawn`; a play test checks the water under its whole hull is deeper than its draft in three new worlds. The player's giant starts at (0, 0, 140), clear of the floating props.
- **The bars stand over the rail**: `ShipBars` places each bar 6 m over its type's freeboard (the brig's still at 9 m), where the fixed 9 m was below the giant's rail.
- Handling: 6.5 m/s, 9 degrees a second; strength 4 (the brig needs about 67 hits); the helm camera 2 m to starboard, as on the frigate. `PlayerShipKind.Giant` and a Giant button.
- Gates: spec pass, compile 0/0, EditMode 300/300, PlayMode 32/32 (the map test now counts the giant). Captures: its chase, bird's eye and helm views, and at anchor from the start.
