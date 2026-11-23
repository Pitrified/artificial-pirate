---
status: done
---

# Phase 05 - Barrels that aim

## Overview

The barrels move (1b): turned within the 20 degrees toward each shot, raised to its elevation, run in on firing. Its own phase so the rest does not wait on it, kept only if the phone shows no cost (Q5).
Context: [`00_start.md`](00_start.md), Barrels that aim.

## Goals

1. Each barrel its own object from the builder, pivoted at its trunnions; the carriages stay in the hull.
2. `Broadside` turns and raises each barrel to its next shot, and recoils it.
3. The phone's gpu and cpu p95 in a fight unchanged against the end of phase 04: measured in phase 06's phone run, which decides whether the barrels stay.

## Plan

- **Builder**: `props.barrels` puts each barrel in `<Name>GunStarboard<i>` / `<Name>GunPort<i>` (one object per barrel, its origin at the trunnions, 0.4 of the barrel from the breech, pointing out; `make_object` takes an origin), the carriages stay in the hull. `BuiltModelTests`: one object per gun on its side; the mouths still at the type's muzzles.
- **The prefab variants**: each barrel's transform set on `Broadside` (an array, starboard then port), by the throwaway editor script.
- **Aim**: while a battery has a target, each barrel eases toward its gun's solved yaw (clamped) and elevation; fires from the barrel's mouth; runs in by `RecoilDistance` (0.5 m) at once and back out over most of the reload. No barrels set: as in phase 01.
- **Measure**: the phone run in a fight against phase 04's; if a cost shows, try the barrels as one mesh a side with the yaw done in the vertex shader before dropping it.

## Out of scope

- Elevation limits beyond what the arc needs; gun crews.

## Done when

- Tests: the barrel turning toward the target within the arc, recoiling and back; the model check; gates green; a capture. The phone's verdict is phase 06's.

## What the implementation found

- A barrel is aimed at its side's target's centre on the solved arc each frame (a solve per barrel, a few dozen a frame at most), not at the scattered point its next ball takes; the ball still leaves from the barrel's mouth, so the two differ by the scatter only.
- `FromToRotation` from the barrel's rest direction may roll it about its own axis; an eight-sided barrel does not show it.
- The triangles are unchanged (the barrels moved out of the hull's mesh): sloop 9,554, brig 16,450; 12 and 18 more objects, one material.
- In a capture the smoke hides the guns for two seconds after a volley; the turn shows after, the barrels off square and raised.
- In a fight capture with the sloop abeam of the brig, the brig's nine guns brought the sloop's mast down and sank it in about 4.5 s (Q3's numbers).
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 255/255, PlayMode 27/27.
