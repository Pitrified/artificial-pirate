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
| 02 | The player's ship      | [`02_player_ship.md`](02_player_ship.md) | in progress |
| 03 | Map icons              | [`03_map_icons.md`](03_map_icons.md) | planned |
| 04 | App icon               | [`04_app_icon.md`](04_app_icon.md) | planned |
| 05 | Verify                 | [`05_verify.md`](05_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-23 : bootstrapped the folder on `feat/13_views` from `feat/12_cannons` at `af3fd99`: no app icon set; the minimap's ship a triangle, other ships not shown; one camera; no settings screen (Diagnostics the pattern); one player ship, which everything points at. Proposed a Settings screen in `PlayerPrefs`, three camera rigs per type, both player ships in the scene with the choice applied at load, silhouettes from the builder's numbers as map icons, the icon a still from the game. Q1 to Q7.
- 2026-11-23 : `main` fast-forwarded to `af3fd99` (the cannons), at the user's word. The user: Q1 to Q3 a; Q4 both, a choice; Q5 a, super small; Q6 both maps; Q7 to be assessed, stills or a flat SVG. Folded in; sub-plans 01 to 05 written.
- 2026-11-23 : review of the sub-plans: both player ships saved inactive with the chosen one turned on in `Awake` (turning one off after its `Awake` races it); the flat icon drawn with Pillow from `textures.py` rather than an SVG needing cairo. Corrected in place. Waiting on the user's go.
- 2026-11-23 : phase 01 - review before: nothing to correct. `GameSettings` (`PlayerPrefs`); the Settings screen (`ScreenId.Settings`, from menu and pause); rigs per type (bird's eye, helm from the builder's `helm_eye`); `CameraController` views with a half-second ease and a steady or riding helm; a CAM button on the HUD (a uGUI copy of the pause button, which is an `OnScreenButton`). 6 new tests. Gates green (EditMode 261, PlayMode 28). Captures of the sloop's bird's eye and helm.
