---
status: done
---

# Phase 02 - Effects

## Overview

What firing and hitting look like: flash and smoke at the muzzle, splinters off a hit hull, a splash column, a dirt puff, and smoke from a badly damaged ship.
Context: [`00_start.md`](00_start.md), Effects; depends on [`01_guns_and_balls.md`](01_guns_and_balls.md) for the events.

## Goals

1. A small particle atlas made by script.
2. One ParticleSystem per kind in the Game scene, emitted at points by `CombatEffects`; no object per shot, no allocation per frame.
3. The five moments of the table in `00_start.md`, and smoke from a ship below a quarter of its health.

## Plan

- **The atlas**: `textures.py particles` writes `T_Particles`, a 2 x 2 atlas: a soft puff, a flash star, a splinter, a drop. One material, `M_Particles`, URP Particles Unlit, alpha blended; the flash's brightness from its colour over 1.
- **`CombatEffects`** (`Game.Gameplay`), in the Game scene, holding one ParticleSystem each: flash, smoke, splinters, splash, dirt. Each a world-space system, emitted particle by particle (`Emit(EmitParams, 1)`, each with its own direction in a cone: one call for the whole burst gives them all the same velocity); sizes, lifetimes and counts on the systems, set by the throwaway editor script that builds them.
  - flash: 3 particles, 0.08 s, along the muzzle's direction;
  - smoke: 6 to 8 puffs, growing, rising a little, 2.5 s;
  - splinters: 12 chips thrown out along the hull's normal, gravity, 1.2 s;
  - splash: 16 drops up and falling, 1 s, plus the ripple ring already made;
  - dirt: 8 brown puffs, 1 s.
- **Wiring**: `CombatEffects` subscribes to `Cannonballs`: its hull, water and ground events, and `Launched` (muzzle, direction), raised as a ball is fired; every ship already holds `Cannonballs`, so none needs another reference.
- **Damage smoke** (`DamageSmoke`, `PF_Ship`'s root): below `SmokeBelowHealth` (0.25) it emits a puff into the smoke system every interval at its heaviest kept hit, thicker while sinking.
- **Ball look**: `M_Cannonball`, the iron of the palette.
- **Capture**: a broadside, a hit and a miss, from the chase camera, as a sequence (the effects move).

## Out of scope

- Sound; camera shake (phase 04).

## Done when

- A capture sequence shows the flash, smoke, splinters, splash and dirt; tests: an effect emitted per event, damage smoke only below the threshold; gates green.

## What the implementation found

- **The water hid the smoke.** The particles and the water share the transparent queue and neither writes depth, so whichever sorted later won; with both centred on the ship the water often drew over the smoke wherever the sea was behind it. `M_Particles` is queued after the water (3010). The cost: a particle that falls below the surface (a splinter, a drop) shows through until it fades.
- **Damage smoke** started at the hit, which can be inside the hull at the waterline, and the smoke's cap on speed kept it there: it rises from the rail above the hit, lives 6 s (a burst may set its own life and colour), three puffs at a time, darker. The smoke's speed cap is 1 m/s (muzzle smoke still slows from 4).
- **The puff** in the atlas was too soft: the visible core under half the particle. Fuller now; a broadside leaves a bank of smoke along the side.
- In EditMode a particle system that has never stepped drops what is emitted into it; the tests step each system once when made. In play the engine steps them.
- The systems always simulate (`AlwaysSimulate`): they live at the scene's origin and emit anywhere.
- Captures: a volley's flash and smoke from off the firing side; splinters, dust and the balls arriving on the brig's side; a sloop at 20% health smoking from its deck.
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 245/245, PlayMode 24/24.
