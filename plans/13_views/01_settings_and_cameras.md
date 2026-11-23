---
status: planned
---

# Phase 01 - Settings and cameras

## Overview

A Settings screen, and the three cameras it chooses between, with a HUD button to cycle them while sailing.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. `GameSettings` (`Game.Systems`): the camera, the helm's motion and the ship, kept in `PlayerPrefs`, with an event when one changes.
2. A Settings screen (`ScreenId.Settings`, UI Toolkit) from the menu and the pause screen, closing back.
3. Three camera rigs per ship type; `CameraController` follows the chosen one and eases between them.
4. A camera button on the HUD cycling the three.

## Plan

- **`GameSettings`**: a plain class owned by `GameManager` (as `SaveSystem` is), `CameraView` (`Chase`, `BirdsEye`, `Helm`), `HelmRides` (bool), `PlayerShip` (`Sloop`, `Brig`); read once at boot, written on each change; `Changed` raised. Keys and defaults in one place. The ship row shows in development builds only (`Debug.isDebugBuild`).
- **The screen**: `Settings.uxml` with `Pause.uss`'s look; a row of toggle buttons per setting, the chosen one lit; Back. `UISystem` binds it as Diagnostics (a `SettingsPanel`), Menu and Pause get a Settings button. The spec's Screens table gains the row in phase 05; `spec_check.py` fails until then, so the row goes in this phase.
- **Rigs**: `SO_ShipType` gains a `CameraRig` (offset, look point) for the bird's eye and the helm beside the chase's present fields (kept as they are: the chase rig). The numbers by a throwaway editor script from each model: the bird's eye about (0, 45, -22) looking at (0, 0, 14) for the sloop, in proportion for the brig; the helm 1.7 m over the deck at the tiller or wheel, the builder's numbers for where the deck and the helm are, looking at a point 60 m ahead at the horizon's height.
- **`CameraController`**: `SetTarget(transform, hull)` and `SetView(view, helmRides)`; each frame the wanted pose is the current rig's from the root (chase, bird's eye, steady helm) or from the hull (riding helm); a switch blends from the pose at the switch to the new one over `SwitchSeconds` (0.5 s); the chase keeps its smoothing, the bird's eye smooths its position only, the helm none. The shake is laid over all of them. `GameSceneWiring` hands it the type's rigs and the settings, and follows `Changed`.
- **HUD button**: a uGUI button beside the pause button, an eye glyph, cycling Chase, Bird's eye, Helm through `GameSettings`.
- **Tests**: `GameSettings` round trip and defaults (EditMode, its keys cleared); the camera at each rig's pose on a still ship, the switch reaching the new pose after the blend, the riding helm rolling with the hull and the steady one not; the screen's buttons setting the choice (PlayMode, menu to settings and back); the HUD button cycling.
- **Captures**: each camera on each ship.

## Out of scope

- Applying the ship choice (phase 02); volumes on the screen.

## Done when

- Tests pass; gates green; the six captures look right.

## What the implementation found
