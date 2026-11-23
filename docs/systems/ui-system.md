# 📄 `docs/systems/ui-system.md`

## Overview

UI infrastructure shared across scenes: a single root canvas/document, safe-area
handling, and screen routing.

---

## Responsibilities

- Provide a persistent UI root (so loading screen survives scene swaps)
- Apply safe-area insets to all panels
- Route between screens (Loading, Menu, Pause, Diagnostics, Map: the full map over the paused game, `docs/systems/world.md`; GameOver: the ship has sunk, Main Menu only)
- Provide a uniform way to show/hide modal overlays

---

## Ownership & Lifetime

- Lives in Persistent scene as `UIRoot`.
- Owned by GameManager.

---

## Technology choice

- **UI Toolkit** (`UIDocument` + `PanelSettings`) preferred.
- uGUI permitted only for v0.2 fallbacks (e.g. virtual joystick) where UI Toolkit is awkward.
- Mixing both in a single screen is forbidden.

---

## Layout rules

- One `PanelSettings` asset, mobile-tuned (reference resolution 1920×1080
  landscape, scale-with-screen).
- All root containers anchor to safe area; insets fetched from
  `Screen.safeArea` once per resolution change.
- Touch targets ≥ 48 dp on shortest side.
- No text below 14 sp (~28 px on Pixel 7).

---

## Public interface (conceptual)

- `ShowLoading()` / `HideLoading()`
- `ShowScreen(ScreenId id)` - Menu, Pause, Diagnostics, GameOver; `HideScreen(ScreenId id)`
- `ShowMap(Texture2D map, Vector2 shipUv, float shipYawDeg, float startView, float closestView)`, hidden with `HideScreen(ScreenId.Map)`; `OnMapCloseRequested`
- `IsAnyOverlayOpen { get; }`

---

## Screens

| Screen   | Source                | Lives in     |
| -------- | --------------------- | ------------ |
| Loading  | `Loading.uxml`        | Persistent   |
| Menu     | `Menu.uxml`           | Menu scene   |
| Pause    | `Pause.uxml`          | Persistent (overlay) |
| Map      | `Map.uxml`            | Persistent (overlay) |
| GameOver | `GameOver.uxml` (styled by `Pause.uss`) | Persistent (overlay) |
| Diagnostics | `Diagnostics.uxml` | Persistent (overlay), from the menu and the pause screen |

The HUD in the Game scene is uGUI (the joystick, the pause button, the minimap in `docs/systems/world.md`).
Under the minimap, `HullBar` shows the player's hull health: a dark bar holding a fill stretched from its left edge to the health, green when whole, yellow at half, red when ruined.
`GameSceneWiring` binds it to the ship's `ShipDamage` once the save is restored, and it redraws on `Damaged`, not each frame.
`HullBarTests` (EditMode): the fill follows the health and reddens.
Beside the pause button, the CAM button (a uGUI `Button`; the pause button is an Input System `OnScreenButton`) cycles the cameras through `GameSettings.NextCamera`.
The **Settings** screen (`UI/Settings.uxml`, `SettingsPanel`), from the menu and the pause screen as Diagnostics is, sets the camera, the helm camera's motion and, in development builds, the ship sailed from the next game; each row lights its chosen button, Back closes it. `GameManager` hands it the settings (`IUISystem.BindSettings`).
Over the other ships near the player's, `ShipBars` places a bar each from a pool of four, red for another side and green for the player's (`docs/systems/combat.md`); it sits under the HUD, so it hides with it.

---

## Interactions

### Uses

- UI Toolkit
- InputManager (to know when pause is requested)

### Used by

- GameManager (state-driven screen routing)
- SceneLoader (to show loading screen during transitions)

---

## Constraints

- No gameplay logic in UI scripts; UI talks to GameManager only.
- No direct scene loading from button handlers; emit intents.
- No real-time data binding in v0.2 (manual refresh on state change is fine).

---

## Failure handling

- Missing UXML/USS → log error, fall back to a plain text overlay so the user is not stranded on a blank screen.