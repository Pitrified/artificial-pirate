---
status: in progress
---

# Phase 04 - The fight

## Overview

What makes the fight readable from the player's ship: a health bar over other ships (Q9) and camera shake; then a PlayMode test of a whole fight.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. `ShipBars` on the HUD: a bar over every other ship within twice the range, red for another side, green for the player's.
2. Camera shake on the followed ship's volley and, harder, on its hits.
3. A PlayMode fight.

## Plan

- **`ShipBars`** (`Game.UI`, uGUI on the HUD canvas): a fixed pool of bars (4), styled like `HullBar`. Every frame: the ships within twice the range of the player's by `Physics.OverlapSphereNonAlloc` on the `Ship` layer, each bar placed over its ship's mast tops by the camera's `WorldToScreenPoint`, hidden behind the camera or beyond the range, its fill the health, its colour by side. Wired by `GameSceneWiring` with the player's ship and the camera.
- **Shake** (`CameraController`): `Shake(strength)` adds a decaying offset (a few centimetres to a metre, 0.3 s, smoothed noise, no allocation). `GameSceneWiring` subscribes the player's `Broadside.Fired` (small, once per volley) and `ShipDamage.Damaged` (by the amount). Off when paused.
- **Tests**: EditMode: a bar per ship in range, none for the player's own or beyond range, colour by side, fill by health; the shake decaying to nothing. PlayMode: the sloop placed on the brig's beam in the Game scene: both fire, both take damage, the bars follow, and with the guns made strong the brig sinks and comes back after the delay.

## Out of scope

- Sound; a reload indicator (declined).

## Done when

- Tests pass; gates green; a capture with the bars.

## What the implementation found
