---
status: in progress
---

# Phase 02 - The player's ship

## Overview

The ship chosen in the settings is the one the player sails, from the next New Game or Continue.
Context: [`00_start.md`](00_start.md), Decisions; depends on [`01_settings_and_cameras.md`](01_settings_and_cameras.md) for `GameSettings`.

## Goals

1. The Game scene holds a player's ship of each kind; the chosen one is on when the scene starts, the other off.
2. Everything that follows the player's ship follows the chosen one.

## Plan

- **The scene**, by a throwaway editor script: a `PlayerBrig`, a `PF_Ship_Brig` instance with the same scene references as `Ship`, side `Player`, its sails set, inactive, at the same start place.
- **`GameSceneWiring`**: `_ships` (one per `PlayerShip` value) in place of `_ship`; both are saved inactive in the scene, and in `Awake` it turns on the chosen one (turning the other off after its own `Awake` and `OnEnable` had run would race them), and keeps it as the ship it wires (input, `RegisterShip`, camera, hull bar, bars, shake, map, ground).
- **What else points at `Ship`**: the minimap's target and each `ShipRespawn`'s player get a public setter, called by the wiring; a check over the scene for any other reference to `Ship` (the throwaway script lists them).
- **The save** keeps no kind: the damage and place go onto the ship that sails; a broken mast's number beyond its masts is already ignored.
- **Tests (PlayMode)**: with the brig chosen, a new game sails the brig: it is `GameManager.PlayerShip`, the camera follows it, the minimap is on it, the sloop is off; the choice back to the sloop on the next game. The setting's key is restored after.

## Out of scope

- A ship's kind in the save; choosing in release builds.

## Done when

- Tests pass; gates green; a capture of the brig sailed from the chase camera.

## What the implementation found
