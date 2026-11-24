---
status: planned
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
