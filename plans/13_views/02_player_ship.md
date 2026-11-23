---
status: done
---

# Phase 02 - The player's ship

## Overview

The ship chosen in the settings is the one the player sails, from the next New Game or Continue.
Context: [`00_start.md`](00_start.md), Decisions; depends on [`01_settings_and_cameras.md`](01_settings_and_cameras.md) for `GameSettings`.

## Goals

1. The Game scene holds a player's ship of each kind; the chosen one is on when the scene starts, the other off.
2. Everything that follows the player's ship follows the chosen one.

## Plan

- **The scene**, by a throwaway editor script: a `PlayerBrig`, a `PF_Ship_Brig` instance with the same scene references as `Ship`, side `Player`, its sails set, inactive, 17 m behind the sloop's start (0, 0, 72), so its longer hull clears the floating props ahead of it.
- **`GameSceneWiring`**: `_ships` (one per `PlayerShip` value) in place of `_ship`; both are saved inactive in the scene, and in `Awake` it turns on the chosen one (turning the other off after its own `Awake` and `OnEnable` had run would race them), and keeps it as the ship it wires (input, `RegisterShip`, camera, hull bar, bars, shake, map, ground).
- **What else points at `Ship`** (listed from the scene: the plan first named only the first two): the minimap's target, each `ShipRespawn`'s player, the world streamer's and the depth map's target, the water's and the ripples' follow. Each gets a public `Follow(Transform)`, called by the wiring in `Awake`. Both ships start at the same place, so nothing has moved away from them by then.
- **Release builds sail the sloop** whatever a development build left in `PlayerPrefs` on the same phone.
- **The save** keeps no kind: the damage and place go onto the ship that sails; a broken mast's number beyond its masts is already ignored.
- **Tests (PlayMode)**: with the brig chosen, a new game sails the brig: it is `GameManager.PlayerShip`, the camera follows it, the minimap is on it, the sloop is off; the choice back to the sloop on the next game. The setting's key is restored after.

## Out of scope

- A ship's kind in the save; choosing in release builds.

## Done when

- Tests pass; gates green; a capture of the brig sailed from the chase camera.

## What the implementation found

- **Six components follow the player's ship**, not the two first named: the world streamer, the depth map, the water, the ripples, the minimap and the respawns. Each has `Follow(Transform)`, called by the wiring's `Awake`.
- **Copying the sloop's scene references onto the brig** by its property modifications does not work: their targets are the sloop prefab's components. The four (the water, the ripples, the balls, the effects) are set on the brig's own components, and the stale modifications pruned.
- **The settings leaked into the other tests**: the EditMode settings test left Brig in the editor's `PlayerPrefs` (it deleted the keys but never saved the deletion), so every PlayMode test sailed the brig, and the brig, started where the sloop starts, sat on a floating barrel. The tests' teardowns save what they restore, and `SettingsIsolation` runs the PlayMode suite on default settings whatever a developer chose in the editor.
- **The brig starts 17 m further back** (0, 0, 72): from the sloop's start its bow reached the floating props.
- **The helm view had a sailor's head beside it**: the two crew flanking the brig's wheel moved forward and outboard (`brig.py` crew), the model rebuilt (16,450 triangles, unchanged).
- `CAPTURE_SHIP` sails a ship in the sequence capture, through the settings, which the suite puts back.
- Captures (the brig): the chase with both other brigs in view, the bird's eye over the start island, the helm over the wheel along the deck.
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 261/261, PlayMode 29/29.
