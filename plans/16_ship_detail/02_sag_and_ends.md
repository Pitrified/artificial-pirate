---
status: planned
---

# Phase 2 - Sag and rope ends

## Overview

Ropes hang, and end in something.
After the look, since the sag moves ropes and can move the clearance count, which this phase brings back to 0.
Context: [`00_start.md`](00_start.md), depends on [`01_rope_look.md`](01_rope_look.md).

## Goals

1. A rope is a polyline of about six segments, hanging parabolically straight down under the line between its ends; the drop at the middle a fraction of the horizontal span, per kind (running rigging visibly, stays a little, shrouds and ratlines none).
2. `line` draws a tube through the points with shared rings, keeping phase 1's UVs (u by length along the polyline).
3. `Rope` holds its points; `clearance.py` tests every segment, `END_GRACE` measured along the rope from either end.
4. Deadeyes in pairs with lanyards where each lower shroud meets the channel, the shroud starting at the upper deadeye; blocks at the yards' ends where the braces and lifts are made fast.

## Plan

- A failing test first where the builder has them; otherwise the clearance report on a hand-made sagging rope through a sail.
- Sag fractions as named constants in `rigging.py`, tuned on the probe and the captures.
- Ratlines start above the upper deadeyes.
- Deadeyes `ShipWood` discs, lanyards `Tar`; blocks `ShipWood`, small boxes or six-sided shells.
- Rebuild every type; the clearance count 0 on every band and the anchored state, the eased trims checked against the previous build's.
- Probe renders; one helm and one chase capture of the giant.

## Out of scope

- The boats' falls' sag and blocks (phase 3).

## Done when

- Clearance 0 everywhere; gates green; the triangles per type reported; the captures shown to the user.

## What the implementation found
