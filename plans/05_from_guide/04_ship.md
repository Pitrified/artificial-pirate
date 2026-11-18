---
status: planned
---

# Phase 04 - The ship on the water

## Plan

- The ship rides the waves, kinematically: each frame, the `WaveField` height at bow, stern, port and starboard gives a target height, pitch and roll, smoothed; planar movement unchanged.
- A `RippleSource` on the hull; a kinematic Rigidbody and collider so the props bounce off it.
- Tests: on flat water the ship stays level at sea level; on a wave it rises and pitches with it; movement tests unchanged.
- `docs/systems/ship-controller.md` updated.

## Done when

- Gates green; a capture sequence of the ship sailing through the waves past the props.

## What the implementation found
