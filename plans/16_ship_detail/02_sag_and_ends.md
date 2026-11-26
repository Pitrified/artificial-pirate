---
status: done
---

# Phase 2 - Sag and rope ends

## Overview

Ropes hang, and end in something.
After the look, since the sag moves ropes and can move the clearance count, which this phase brings back to 0.
Context: [`00_start.md`](00_start.md), depends on [`01_rope_look.md`](01_rope_look.md).

## Goals

1. A rope is a polyline of about six segments, hanging parabolically straight down under the line between its ends; the drop at the middle a fraction of the horizontal span, per kind (running rigging visibly, stays a little, shrouds and ratlines none).
2. The rope primitive (`mesh.rope`, already taking points) draws the sagging rope as one tube, its UVs by length along the polyline.
3. `Rope` holds its points; `clearance.py` tests every segment, `END_GRACE` measured along the rope from either end.
4. Deadeyes in pairs with lanyards where each lower shroud meets the channel, the shroud starting at the upper deadeye; blocks at the yards' ends where the braces and lifts are made fast.

## Plan

- The builder has no Python tests: a scratch script first, a hand-made sagging rope through a sail whose straight chord clears it, reported by `clearance.crossings` once the check follows the segments.
- Sag fractions as named constants in `rigging.py`, tuned on the probe and the captures.
  A rope that does not sag keeps two points, so it costs nothing more; ratlines stay straight (about a thousand on the giant: six segments each would add about 30k triangles).
- Deadeyes go in the mast's parts with their shroud; the topmast shrouds' feet at the tops get none for now.
- Ratlines start above the upper deadeyes.
- Deadeyes `ShipWood` discs, lanyards `Tar`; blocks `ShipWood`, small boxes or six-sided shells.
- Rebuild every type; the clearance count 0 on every band and the anchored state, the eased trims checked against the previous build's.
- Probe renders; one helm and one chase capture of the giant.

## Out of scope

- The boats' falls' sag and blocks (phase 3).

## Done when

- Clearance 0 everywhere; gates green; the triangles per type reported; the captures shown to the user.

## What the implementation found
The scratch check (a sagging rope through a sail whose chord clears it) failed on the old `Rope` and passed once the check walked the segments.
Clearance stayed 0 on every type, band and the anchored state with the sag; every band's sail state has the same triangle count, so no band fell back to square yards.
Triangles shown, one state: sloop 16.7k (+1.0k), brig 28.8k (+2.6k), frigate 34.6k (+3.9k), giant 48.4k (+4.0k).
A side capture shows the braces drooping between the masts and the deadeyes on the channels.
From 75 m the braces break into dashes: under a pixel, the aliasing MSAA settled on the phone; the batch capture may not apply it, so the phone judges it.
A near capture against the light showed the tarred ropes silver-white: the sky's reflection at grazing angles on a thin tube with a strong normal map.
The rope materials now have environment reflections and specular highlights off (a throwaway editor script, deleted); the next capture reads brown rope with its lay.
The trims are not printed by the build, so "the eased trims against the previous build's" was checked only through the per-band triangle counts.
