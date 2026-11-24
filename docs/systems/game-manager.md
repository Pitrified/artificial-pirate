# 📄 `docs/systems/game-manager.md`

## Overview

Central authority over **application state, lifecycle, and high-level
coordination**. Owns references to all systems.

---

## Responsibilities

- Own the global game-state machine
- Coordinate scene transitions via SceneLoader
- Initialize/teardown core systems
- Provide access to shared services (Save, Audio, Input, UI)
- Handle app-level events (focus, pause, quit)

---

## Ownership & Lifetime

- Created in Boot scene during `GameBootstrap`.
- Lives in Persistent scene.
- **Controlled singleton**: `GameManager.Instance` for read-only access; never
  set from outside.
- Survives all scene loads.

---

## Game states

```
enum GameState { Boot, Loading, Menu, Playing, Paused, GameOver }
```

### Allowed transitions

```
Boot     → Loading
Loading  → Menu
Menu     → Loading       (when starting/continuing a game)
Loading  → Playing
Playing  → Paused
Paused   → Playing
Paused   → Loading       (returning to menu)
Playing  → Loading       (returning to menu)
Playing  → GameOver      (the player's ship has sunk)
Paused   → GameOver      (it went under with the game paused)
GameOver → Loading       (returning to menu)
```

**Game over.** `RegisterShip` listens to the ship's `ShipDamage.Sunk` (`docs/systems/ship-controller.md`).
When it fires, the save is deleted (the menu offers no Continue), the pause and map screens are hidden, input is turned off and the game over screen is shown.
Its Main Menu button raises `OnReturnToMenuRequested`, and `ReturnToMenuAsync` goes back without saving.


Any other transition is rejected with a logged warning.

---

## Core data

- `GameState CurrentState { get; }`
- `GameState PreviousState { get; }`
- References (assigned by Bootstrap):
  - `SceneLoader`
  - `SaveSystem`
  - `InputManager`
  - `AudioManager`
  - `UISystem`
- `GameSettings Settings`: the player's settings in `PlayerPrefs`, made at `Initialize` and handed to the UI: the camera (`CameraView`), whether the helm camera rides the hull, and the ship sailed (`PlayerShipKind`: Sloop, Brig, Frigate, Giant; offered in development builds only, a release sails the sloop). Each change is written at once and raised as `Changed`. The PlayMode tests run on default settings (`SettingsIsolation`).
- `ShipController PlayerShip`: the ship `GameSceneWiring` registered, the one saved and followed. The Game scene holds a player's ship of each kind, inactive; the wiring turns on the chosen one in `Awake` and points what follows the player's ship (the world, the water, the ripples, the minimap, the respawns) at it (`Follow`).

---

## Public interface

```
void OnBootComplete()                 // called by GameBootstrap
Task StartNewGameAsync()
Task ContinueAsync()                  // requires SaveSystem.HasSave
Task ReturnToMenuAsync()
void PauseGame()
void ResumeGame()
void QuitGame()
event Action<GameState, GameState> StateChanged;   // (previous, next)
```

---

## App-level events

- `OnApplicationPause(true)` while `Playing` → auto-PauseGame and Save.
- `OnApplicationQuit` → Save if `Playing` or `Paused`.
- A save needs a registered ship: quitting from the menu writes nothing.
- Focus loss on mobile counts as pause.

---

## Interactions

### Uses
- SceneLoader (scene transitions)
- SaveSystem (persistence)
- InputManager (enable/disable, pause polling)
- AudioManager (snapshots on pause)
- UISystem (screen routing)

### Used by
- UI controllers (menu/pause buttons)
- GameBootstrap (handoff)
- Debug overlay (state inspection)

---

## Constraints

- **No gameplay logic.** No ship, camera, or world references.
- **No direct scene API calls.** Everything through SceneLoader.
- Deterministic state machine: each transition is a single method.
- No `Update` work beyond polling pause input.

---

## Failure handling

- Invalid transition → log warning, no-op.
- Scene load failure (from SceneLoader) → fall back to Menu.
- Save failure on quit → log error, do not block quit.
