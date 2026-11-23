# Artificial Pirate - functional spec

What the game is now, what is in and out of its scope, and the decisions that hold across it.
How each part works is in its system page, linked from [Systems](#systems); this page does not repeat it.
A feature that changes anything here changes this page in its own branch ([Keeping this page current](#keeping-this-page-current)).

---

## The game

A 3D naval game for Android phones, played in landscape.
The player sails a sloop with one virtual stick across a sea that grows as it sails: islands generated from a seed, cell by cell, a minimap and a full map of what has been explored.
Ships are built from parts (hull, masts, sails, rigging, fittings, guns) by the asset builders, so several types share one model; the sloop and a brig exist.
Ships fight with their broadsides: a ship's guns fire on their own at a ship of another side in range on its beam, so the player fights by steering, and the barrels turn to the shot.
Hulls take damage from the balls where they strike, and when they ram a ship or run aground: a damaged ship is slower, sits lower and lists, its hull shows the hits, a badly damaged one smokes, and a hard hit can bring a mast down.
A ruined hull sinks; the player's sinking ends the game. A pirate brig lies at anchor near the start and another sails a circle; a sunk one comes back.
The session is saved and continued from the menu.

Priorities, in order: a steady frame rate on a mid-range phone, code an agent can read and change safely, a fast loop from change to device, then looks.

---

## Scope

Something moves between these lists through a plan in `plans/`, which updates this page.

**In the game:** sailing (kinematic), the chase camera and its shake, the generated world and its maps, stylized water with waves, shore foam and ripples, floating props, two ship types, ship against ship and against the shore, broadsides that fire on their own (sides, aimed barrels, the balls' hits, flash, smoke, splinters, splashes), hull damage, smoke from a wreck, broken masts, sinking and game over, a ship sailing a circle on its own, sunk ships coming back, bars over other ships, the save, the diagnostics report and the debug overlay.

**Next** (wanted, not yet planned; work toward it is not blocked): ships that choose where to sail and chase, sails that follow the heading, more ship types, things to find on islands, sound, chain shot at the masts, a choice of cameras in the settings (one from behind the helm), chase and stern guns (on bigger ships), a giant ship (a three-masted galleon, the size of a tall ship such as the Amerigo Vespucci).

**Not planned** (do not build without a plan that moves it): multiplayer, an economy or progression, cloud save, in-app purchases, ads, analytics, localization, accessibility settings, controller support, tablet or foldable layouts, realistic water physics.

---

## Locked decisions

| Concern        | Decision                                  | Why                                    |
| -------------- | ----------------------------------------- | -------------------------------------- |
| Engine         | **Unity 6.4** (`6000.4.4f1`)              | Supported Update release, kept until the next LTS ships, then moved to it |
| Render         | **URP**                                   | Best mobile performance for the look   |
| Scripting      | **C# / .NET Standard 2.1**                | Unity 6's default                      |
| Backend        | **IL2CPP**, ARM64 only                    | Required for the Play Store            |
| Input          | **Unity Input System**                    | Actions abstract touch and keyboard    |
| Movement       | **Kinematic / arcade**                    | Deterministic, no physics instability  |
| Water          | **Custom URP shader (stylized)**: Gerstner waves, shore foam from a depth map, a 256x256 ripple step ([water](systems/water.md)) | Reactive in one draw; no fluid simulation |
| Models         | **Built by script**: Blender builders export FBX ([building_assets](building_assets.md)) | Repeatable, reviewable, one change for every ship type |
| Camera         | **Third-person chase**                    | Mobile-friendly, no free look          |
| Orientation    | **Landscape only**                        | One layout to design for               |
| Scenes         | **Additive scenes over a persistent root** | Clean lifetimes                       |
| Save           | **JSON file in `Application.persistentDataPath`** ([save-system](systems/save-system.md)) | Inspectable, portable |
| Builds         | **Local scripts** (`Tools/build-android.sh`) | No CI                               |

Anything not listed is **not decided**: raise it in a plan rather than introducing a silent default.

---

## Platform

- Reference device: a Pixel 7-class phone (mid-range Android, 2022 on); the budgets are in [performance-budget](performance-budget.md).
- Google Play: `targetSdkVersion` 35 or above, ARM64 only, AAB for releases ([build-and-release](build-and-release.md)).

---

## Lifecycle

```
App launch
  -> Boot (init systems, load Persistent additively)
  -> Loading -> Menu
  -> Game (additive over Persistent) -> Pause / Resume
  -> Back to the menu (unload Game), or the player's ship sinks -> Game over -> Menu (the save deleted)
  -> Quit
```

- [GameManager](systems/game-manager.md) owns every state change (`GameState`: Boot, Loading, Menu, Playing, Paused, GameOver).
- No scene owns global state; scenes are passive, GameManager wires them.

---

## Scenes

| Scene        | Loaded as | Lifetime              | Contents                                |
| ------------ | --------- | --------------------- | --------------------------------------- |
| `Boot`       | Single    | First frame only      | `GameBootstrap` only                    |
| `Persistent` | Additive  | Whole session         | GameManager, Input, Save, Audio, UI root |
| `Menu`       | Single    | While in the menu     | Main menu UI                            |
| `Game`       | Additive  | While playing         | The ships, water, the generated world, floating props, the HUD, the camera |

---

## Controls

| Action         | Touch                                  | Editor (keyboard) |
| -------------- | -------------------------------------- | ----------------- |
| Throttle       | Virtual stick, Y axis                  | `W` / `S`         |
| Steering       | Virtual stick, X axis                  | `A` / `D`         |
| Pause          | Pause button, top left                 | `Esc`             |
| Map            | Tap the minimap                        | Click the minimap |
| Debug overlay  | Three-finger tap (development builds)  | `F1`              |

---

## Screens

UI Toolkit screens, in the safe area, routed by `UISystem` by `ScreenId` ([ui-system](systems/ui-system.md)):

| Screen | `ScreenId` | What it offers |
| ------ | ---------- | -------------- |
| Main menu | `Menu` | New Game, Continue (when a save exists), Settings, Diagnostics, Quit |
| Pause | `Pause` | Resume, Main Menu, Settings, Diagnostics |
| Map | `Map` | The explored world over the paused game: centre on the ship, reset zoom, close |
| Game over | `GameOver` | The ship has sunk: Main Menu |
| Diagnostics | `Diagnostics` | The diagnostics report: copy, share, the previous session's |
| Settings | `Settings` | The camera (Chase, Bird's eye, Helm), the helm camera steady or riding; in development builds the ship sailed from the next game; Back |

Also: the loading screen (at least 0.5 s, so it does not flash), and the Game scene's HUD in uGUI (the stick, the pause button, the minimap, the hull bar, bars over the other ships near the player's).

---

## Systems

Each has a page under [systems/](systems/):

| System | Page | Also in the page |
| ------ | ---- | ---------------- |
| Entry point, loads Persistent | [game-bootstrap](systems/game-bootstrap.md) | |
| State machine, scene coordination | [game-manager](systems/game-manager.md) | game over |
| Async additive load and unload | [scene-loader](systems/scene-loader.md) | |
| Input abstraction | [input-manager](systems/input-manager.md) | |
| Ships | [ship-controller](systems/ship-controller.md) | ship types, hull contact, damage, masts, hull marks, riding the waves |
| Combat | [combat](systems/combat.md) | sides, guns and barrels, balls, effects, ships at sea, coming back, bars over ships |
| Chase camera | [camera-controller](systems/camera-controller.md) | |
| The world | [world](systems/world.md) | islands, streaming, the water's depth, the minimap and map |
| Water | [water](systems/water.md) | waves, floating props, ripples, shore |
| JSON save | [save-system](systems/save-system.md) | the save's schema |
| Music and sound | [audio-manager](systems/audio-manager.md) | |
| UI Toolkit roots, screens, safe area | [ui-system](systems/ui-system.md) | the HUD, the hull bar |
| Logging | [logging](systems/logging.md) | the diagnostics report, the debug overlay |

---

## Errors

| Failure                       | Behaviour                               |
| ----------------------------- | --------------------------------------- |
| Scene load fails              | Log an error, return to the menu        |
| Save file corrupt             | Back it up and start fresh ([save-system](systems/save-system.md)) |
| Missing component reference   | Log an error, the system stays in a safe state |
| Input device disconnected     | Neutral input until it is back          |

Never swallow an exception silently; never crash in normal play.
Logging goes through `Game.Systems.Log`; release builds keep warnings and errors ([logging](systems/logging.md)).

---

## Release checks

A build goes to testers when all of these hold on the reference phone:

- Cold start to the menu in under 5 s.
- Menu to game to menu, repeatedly, without leaks.
- New Game, sail, pause, resume; quit, relaunch, Continue restores the ship, its damage and the world.
- The ship sinks to game over and back to the menu, with no save left.
- A fight with each brig: both sides fire and hit, the sunk brig comes back.
- 60 fps held, by the diagnostics report, as [performance-budget](performance-budget.md) measures it.
- No errors or warnings in logcat in normal play.

---

## Keeping this page current

- A plan in `plans/` says in its start file what it changes on this page (scope, a decision, a screen, a control, a system).
- The change lands in the feature's own branch, by its last phase; that phase's "Done when" names this page, as it names the system pages.
- `Tools/spec_check.py`, run by `Tools/gates.sh`, fails when a system page is missing from [Systems](#systems), a `ScreenId` from [Screens](#screens), or when the save's version in [save-system](systems/save-system.md#schema) differs from the code's.
