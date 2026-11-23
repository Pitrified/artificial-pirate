---
status: in progress
---

# Phase 05 - Barrels that aim

## Overview

The barrels move (1b): turned within the 20 degrees toward each shot, raised to its elevation, run in on firing. Its own phase so the rest does not wait on it, kept only if the phone shows no cost (Q5).
Context: [`00_start.md`](00_start.md), Barrels that aim.

## Goals

1. Each barrel its own object from the builder, pivoted at its trunnions; the carriages stay in the hull.
2. `Broadside` turns and raises each barrel to its next shot, and recoils it.
3. The phone's gpu and cpu p95 in a fight unchanged against the end of phase 04.

## Plan

- **Builder**: `props.cannons` puts each barrel in `<Name>Gun<side><i>` (one object per barrel, its origin at the trunnions, pointing out), the rest of the hull without them. `BuiltModelTests`: one object per gun, at the type's muzzles less the barrel's reach.
- **The prefab variants**: each barrel's transform set on `Broadside` (an array, starboard then port), by the throwaway editor script.
- **Aim**: while a battery has a target, each barrel eases toward its gun's solved yaw (clamped) and elevation; fires from the barrel's mouth; runs in by `RecoilDistance` (0.5 m) at once and back out over most of the reload. No barrels set: as in phase 01.
- **Measure**: the phone run in a fight against phase 04's; if a cost shows, try the barrels as one mesh a side with the yaw done in the vertex shader before dropping it.

## Out of scope

- Elevation limits beyond what the arc needs; gun crews.

## Done when

- Tests: the barrel turning toward the target within the arc, recoiling and back; the model check; gates green; a capture; the phone shows no cost, or the phase is recorded as dropped with the numbers.

## What the implementation found
