# Views - implementation tracking

An app icon of a brig at sea, boats on the minimap with an icon per kind of ship, three cameras picked in a new settings screen, and a choice of the player's ship for testing. Analysis, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- To come with the answers to Q1 to Q7.

## Phases

| #  | Phase                  | Plan | Status |
| -- | ---------------------- | ---- | ------ |
| 01 | Settings and cameras   | to write | draft |
| 02 | The player's ship      | to write | draft |
| 03 | Map icons              | to write | draft |
| 04 | App icon               | to write | draft |
| 05 | Verify                 | to write | draft |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-23 : bootstrapped the folder on `feat/13_views` from `feat/12_cannons` at `af3fd99`: no app icon set; the minimap's ship a triangle, other ships not shown; one camera; no settings screen (Diagnostics the pattern); one player ship, which everything points at. Proposed a Settings screen in `PlayerPrefs`, three camera rigs per type, both player ships in the scene with the choice applied at load, silhouettes from the builder's numbers as map icons, the icon a still from the game. Q1 to Q7.
