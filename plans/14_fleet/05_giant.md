---
status: planned
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
