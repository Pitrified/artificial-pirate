---
status: done
---

# Phase 06 - The ship's wake

The phone run (phase 05) showed the wake as a trail of circles.
Asked 2026-11-19 by the user: "Main one is the circles, they are the most unnatural. The fact that ship can leave the wake area is silly, yes, make it follow the ship. It is a bit strong but we will tune that after we change wake type. So we go for b and c, change the test to assess the boat moving".

## Why circles

`RippleSource` on the hull drops one round dent at the hull's centre every 0.25 s: at 8 m/s, a ring every 2 m, each spreading on its own.
The ripple square is fixed (48 m at (2, 40)); the ship sails out of it in seconds.

## Decisions

Answered 2026-11-19 by the user, both as recommended:

- **The foam trail lives in the ripple field**, as a third channel next to the height and the step before: stamped by the hull, fading over about 2 s, drawn by the water shader on the wavy surface. No new mesh or draw call. The trail is as long as the square reaches behind the ship.
- **The hull stamps every simulation step** instead of a ring every 0.25 s, so the rings merge. Ripples in the field travel at about 4 m/s (0.71 texel a step, 0.1875 m texels, 30 steps/s): a ship faster than that leaves a V of ripples by itself, about 30 degrees each side at 8 m/s.

## Plan

1. **The capture drives the ship**: `SequenceCaptureTests` takes `CAPTURE_THROTTLE` and `CAPTURE_STEERING` and holds them on the ship (the input left unwired), and checks that the ship moved when given throttle. A baseline capture of the circles.
2. **The square follows a target** (B): `WaterRipples` gets an optional transform to follow; at each step the square's centre moves to it, snapped to whole texels, and the step reads the previous state shifted by those texels, so ripples stay where they are in the world. Impulses are kept in world units until the step. The scene's square follows the ship.
3. **The wake** (C): the field's texture gains a foam channel (RGBA half), faded each step and spread a little to its neighbours; `WaterRipples.AddFoam` stamps it. A `WakeSource` on the ship's root, in place of the hull's `RippleSource`: while moving, every frame a ripple dent at the hull and foam at the stern and both bow shoulders, scaled by speed and by the frame's time so the phone's 60 frames and the capture's 30 add up the same. The water shader foams where the channel is.
4. Tests: the square moves and a ripple stays put in the world; foam is stamped and fades; a moving `WakeSource` leaves foam behind it. Captures of the ship sailing, straight and turning. Docs: `water.md`, `ship-controller.md`.

Strength is tuned on the phone later, as the user said.

## Done when

- Gates green; a capture of the ship sailing shows a V and a foam trail and no ring chain, the wake following the ship past the old square's edge.
- HUMAN on the phone: the wake looks like a wake; 60 fps held.

## What the implementation found

- 2026-11-19. **The capture sails**: `SequenceCaptureTests` takes `CAPTURE_THROTTLE` and `CAPTURE_STEERING`, leaves the ship's input unwired and holds them, and asserts the ship moved more than 1 m under throttle. Full throttle with steering 0.15 for 8 s: 56.6 m.
- **Baseline** (chase camera, 5 fps): a chain of rings behind the ship; from about 5 s in, past z 16, no wake at all: the ship had left the square.
- **`WaterRipples`**: an optional `_follow`; at each step's first blit the centre moves onto it by whole texels and the shader reads the previous state at `uv + _Shift`, nothing from outside the square. Impulses and foam queue in world units and are put in the square's uv at the step, so a step that moves the square places them right. The texture is RGBA half: height, height a step ago, foam. Foam fades by `_foamFade` (0.985 a step) and spreads by `_foamSpread` (0.15 toward the neighbours' mean); the border absorbs ripples as before, not foam.
- **`WakeSource`** on `PF_Ship`'s root, the hull's `RippleSource` removed (a throwaway editor script, then deleted; the scene's stale overrides on the old component removed). Each frame it moves faster than 0.5 m/s: a dent of radius 0.6 m at the centre, `-speed * 0.03 * dt`; foam `speed * 1 * dt` at the stern (radius 0.6 m) and at both bow shoulders (0.3 m). A move faster than 40 m/s in one frame is a teleport (a loaded save) and leaves nothing. `Water.shader`: the foam channel, `_WakeFoam` 1, solid where fresh and broken up by the detail normal where it thins.
- **After** (same run): a foam trail behind the stern, no rings, the wake following the ship to the end of the run. From above (eye 30 m up): the trail and faint ripple arms about 30 degrees each side. The V arms are faint at the chase camera; strength is the phone's to tune.
- Seen in passing: on a straight course the ship sails onto the island; it has no land collision (unchanged here).
- `WaterRippleTests`, 3 new: a following square moves by whole texels and a ripple keeps its world place; foam is stamped, about half gone after 1.5 s, spread beyond its stamp, and makes no ripples; a `WakeSource` at 8 m/s on game-sized texels leaves foam on its path behind, none ahead or 2.5 m to the side, and ripple energy ahead under a tenth of behind.
- Docs: `water.md` (the parts, the square rule, "A wake", the cost), `ship-controller.md` (the ship's parts and wiring), `headless-unity.md` (the capture's new variables).
- Gates: compile 0 warnings, EditMode 136/136, PlayMode 19/19. Dev APK 73,685,386 bytes; the build left git unchanged except Unity's re-save of `M_WaterRippleSim` with the shader's new properties, kept.
- 2026-11-19. The user, on the captures: "Looks neat"; then "looks good, complete this effort". Closed on that. Not checked on the phone: 60 fps with the RGBA ripple step (the phase 05 run held 60 fps with the RG one) and the strength tuning (`_dentStrength`, `_foamRate`, `_WakeFoam`, `_foamFade`).
