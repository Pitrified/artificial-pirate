---
status: in progress
---

# Phase 06 - The view of a sinking ship

## Overview

The phone read found the helm view broken while the player's ship sinks: the hull goes under and tips while the ship's root stays at the surface, so a riding helm goes under with it and a steady one hangs over the water.
Q8 a: the chase is shown once the player's ship is sinking, the setting unchanged.
Context: [`00_start.md`](00_start.md), The phone read; found by [`05_verify.md`](05_verify.md).

## Goals

1. Once the player's ship is sinking, the camera eases to the chase, whatever view is chosen.
2. The setting stays as chosen; a change of setting while sinking still shows the chase.

## Plan

- `ShipDamage.SinkingStarted`, raised once when the hull's health runs out (a hit at no health is already ignored).
- `GameSceneWiring` shows one view: the chase while the player's ship is sinking, else the setting's; on the setting's change, on `SinkingStarted`, and on `Restored` (a save restored mid-sink).
- **Tests**: EditMode, `SinkingStarted` raised once; PlayMode, with the helm chosen, the player's ship sunk by hits: the camera shows the chase and the setting is still the helm; a change of setting while sinking keeps the chase.
- The spec's Next gains "the helm view in a milder sea" (Q9 a); `camera-controller.md` says the chase shows a sinking ship.
- A dev APK for the user.

## Done when

- Tests pass; gates green; the user sees a sinking from the helm setting on the phone.

## What the implementation found
