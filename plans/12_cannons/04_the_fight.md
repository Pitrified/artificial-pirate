---
status: done
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

- **`ShipBars`** (`Game.UI`, uGUI on the HUD canvas): a fixed pool of bars (4), styled like `HullBar`. Every frame: the ships within twice the range of the player's by `Physics.OverlapSphereNonAlloc` on the `Ship` layer, each bar placed over its ship's mast tops by the camera's `WorldToScreenPoint`, hidden behind the camera or beyond the range, its fill the health, its colour by side. Wired by `GameSceneWiring` with the player's ship and the camera; the bars sit under the HUD, so they hide with it.
- **Shake** (`CameraController`): `Shake(strength)` adds a decaying offset (a few centimetres to a metre, 0.3 s, smoothed noise, no allocation). `GameSceneWiring` subscribes the player's `Broadside.Fired` (small, once per volley) and `ShipDamage.Damaged` (by the amount). Off when paused.
- **Tests**: EditMode: a bar per ship in range, none for the player's own or beyond range, colour by side, fill by health; the shake decaying to nothing. PlayMode: the sloop placed on the brig's beam in the Game scene: both fire, both take damage, a bar shows over the brig; sunk, with the delay shortened and the player sailed clear, the brig comes back whole.

## Out of scope

- Sound; a reload indicator (declined).

## Done when

- Tests pass; gates green; a capture with the bars.

## What the implementation found

- **In the chase view the enemy is off the screen.** The camera looks ahead of the ship and a broadside fight puts the enemy on the beam, at the right or left edge or beyond: the capture from the game's own camera shows the smoke at the edge and no bar over the brig all fight long. The bars work (tested, and they show whenever the ship is on the screen), but the fight is hard to read. A camera matter, so raised as `00_start.md` Q11 rather than changed here.
- **The brig sinks the sloop in about two volleys** at 50 m (nine guns at 0.06, most hitting), as Q3's numbers say; the hull bar goes green to red in about eight seconds. Tuning for the phone run.
- The capture camera cannot show the bars where the player sees them: the bars are placed through the game's camera, the capture draws through its own. The EditMode test covers where they go.
- The camera's shake is laid over its smoothed position and taken off again before the next smoothing step, so it never feeds back into the follow.
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 252/252, PlayMode 27/27.
