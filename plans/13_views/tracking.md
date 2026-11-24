# Views - implementation tracking

An app icon of a brig at sea, boats on the minimap with an icon per kind of ship, three cameras picked in a new settings screen, and a choice of the player's ship for testing. Analysis, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- A Settings screen from menu and pause, in `PlayerPrefs`: camera, helm motion (steady or riding, both to try), and the ship in development builds.
- Cameras are rigs per ship type; a HUD button cycles them.
- Map icons are small silhouettes per type from the builder's numbers, tinted by side, on the minimap and the full map.
- The app icon is the user's pick among candidates (stills from the game, a flat SVG).

## Phases

| #  | Phase                  | Plan | Status |
| -- | ---------------------- | ---- | ------ |
| 01 | Settings and cameras   | [`01_settings_and_cameras.md`](01_settings_and_cameras.md) | done |
| 02 | The player's ship      | [`02_player_ship.md`](02_player_ship.md) | done |
| 03 | Map icons              | [`03_map_icons.md`](03_map_icons.md) | done |
| 04 | App icon               | [`04_app_icon.md`](04_app_icon.md) | done |
| 05 | Verify                 | [`05_verify.md`](05_verify.md) | done |
| 06 | The view of a sinking ship | [`06_sinking_view.md`](06_sinking_view.md) | done |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-23 : bootstrapped the folder on `feat/13_views` from `feat/12_cannons` at `af3fd99`: no app icon set; the minimap's ship a triangle, other ships not shown; one camera; no settings screen (Diagnostics the pattern); one player ship, which everything points at. Proposed a Settings screen in `PlayerPrefs`, three camera rigs per type, both player ships in the scene with the choice applied at load, silhouettes from the builder's numbers as map icons, the icon a still from the game. Q1 to Q7.
- 2026-11-23 : `main` fast-forwarded to `af3fd99` (the cannons), at the user's word. The user: Q1 to Q3 a; Q4 both, a choice; Q5 a, super small; Q6 both maps; Q7 to be assessed, stills or a flat SVG. Folded in; sub-plans 01 to 05 written.
- 2026-11-23 : review of the sub-plans: both player ships saved inactive with the chosen one turned on in `Awake` (turning one off after its `Awake` races it); the flat icon drawn with Pillow from `textures.py` rather than an SVG needing cairo. Corrected in place. Waiting on the user's go.
- 2026-11-23 : phase 01 - review before: nothing to correct. `GameSettings` (`PlayerPrefs`); the Settings screen (`ScreenId.Settings`, from menu and pause); rigs per type (bird's eye, helm from the builder's `helm_eye`); `CameraController` views with a half-second ease and a steady or riding helm; a CAM button on the HUD (a uGUI copy of the pause button, which is an `OnScreenButton`). 6 new tests. Gates green (EditMode 261, PlayMode 28). Captures of the sloop's bird's eye and helm.
- 2026-11-23 : phase 02 (in progress) - review before: six components follow the player's ship, not two (`Follow` setters added). A player's brig in the scene, both player ships inactive, the chosen one turned on in `Awake` (release builds the sloop); the brig's scene references set directly (copied modifications targeted the sloop's prefab; pruned). Found: the settings tests left Brig in the editor's `PlayerPrefs` (a delete never saved), so every PlayMode test sailed the brig and a floating barrel sat on its deck at the start; teardowns now save, and `SettingsIsolation` runs the PlayMode suite on default settings. Gates green (EditMode 261, PlayMode 29). Left: a capture of the brig sailed and its three views, then close.
- 2026-11-23 : phase 02 closed: the player's brig starts at (0, 0, 72), clear of the floating props; the brig's helm crew moved off the helm view (model rebuilt); `CAPTURE_SHIP`; captures of the brig's three views. Gates green (EditMode 261, PlayMode 29).
- 2026-11-23 : phase 03 - review before: nothing to correct. `T_MapIcon_<Type>` from the builders' hulls; `SO_ShipType.MapIcon`; tints and sizes on `SO_MapStyle`; the minimap's ships (a pool of 8) and the full map's `MapMarker`s; `MapArrow` removed. 2 new tests. Gates green (EditMode 263, PlayMode 30). Capture of the minimap with both brigs.
- 2026-11-23 : phase 04 (in progress) - candidates made: three stills of the sailing brig from the game (bow quarter, abeam low, high three-quarter; the first framing too tight, retaken nearly twice as far so the brig sits in the adaptive icon's safe circle) and a flat drawing by a Pillow script (black hull, ochre band, cream sails, a black flag, flat sea and sky). Shown to the user as a sheet with each at 48 and 96 px. Waiting on the pick.
- 2026-11-23 : phase 04 - the user picked D, the flat drawing. `textures.py app_icon` (`T_AppIcon`, `T_AppIconClear`); set as the default, legacy, round and adaptive icons; in the APK at every density.
- 2026-11-23 : phase 05 - review before: nothing to correct. The spec (the game, Scope, a Settings decision row, Controls' camera button, the HUD, the map's ships; a choice of cameras leaves Next) and the pages (`camera-controller.md`, `ui-system.md`, `world.md`, `game-manager.md`, `ship-controller.md`, `building_assets.md`, `headless-unity.md`) updated; spec check pass, links 0 broken, citations clean. Dev APK built with the icon. Waiting on the user's phone run and read.
- 2026-11-23 : phase 05 - the user's phone read: the icon, the cameras, both ships and both maps work; GPU p50 10.3 ms (cannons 11.8). The helm view is sickening in both motions, and sinking in the helm view breaks it (the hull goes under and tips while the root stays up). Q8 and Q9 raised; phase 05 stays open on them.
- 2026-11-23 : the user: Q8 a (the chase while the player's ship sinks), Q9 a (the helm kept; a milder sea to Next). Folded in; phase 06 written and started. Review before: a hit at no health returns early, so a sinking start is raised once; a save restored mid-sink is covered through `Restored`.
- 2026-11-23 : phase 06 - `ShipDamage.SinkingStarted`; the wiring shows the chase while the player's ship sinks, the setting kept; spec Next and the camera and ship pages. Gates green (EditMode 263, PlayMode 31); dev APK built at `cd43e2e`. Waiting on the user's sinking from the helm on the phone.
- 2026-11-24 : the user's read of `cd43e2e`: the view eased from the helm to the chase at both sinkings; GPU p50 11.8 ms, memory flat over two games. The report's fps maximum (56606, from a 0.0 ms frame) noted, not fixed. Phases 05 and 06 closed; the folder done, priority 0.
