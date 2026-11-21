# Ship damage - implementation tracking

Ships take damage from rams and groundings: the hull's health falls, the ship slows, sits lower and lists, shows marks and holes, can lose masts, and sinks at 0, which is game over for the player.
Brainstorm, options and questions in [`00_start.md`](00_start.md).

## Key decisions

- Health and handling, marks on the hull, and masts as their own breakable parts (options A, B, C); no runtime mesh deformation or fracture.
- One `ShipDamage` per ship with a `HullContact`, taking hits as a world point and an amount, so the cannons call the same thing later.
- The player's ship sinking is game over, back to the menu, the save deleted; a harbour comes later.
- No repair in this effort. Damage is saved and shown on the HUD.

## Phases

| #  | Phase                    | Plan | Status  |
| -- | ------------------------ | ---- | ------- |
| 01 | Hull damage and handling | [`01_hull_damage.md`](01_hull_damage.md) | done |
| 02 | Sinking and game over    | [`02_sinking.md`](02_sinking.md) | done |
| 03 | HUD bar and save         | [`03_hud_and_save.md`](03_hud_and_save.md) | done |
| 04 | Marks on the hull        | [`04_hull_marks.md`](04_hull_marks.md) | done |
| 05 | Masts that break         | [`05_masts.md`](05_masts.md) | done |
| 06 | Verify                   | [`06_verify.md`](06_verify.md) | done |

## Log

- 2026-11-20 : bootstrapped the folder on `feat/09_ship_damage`: research, options A to E, the user chose A, B and C and answered five questions in chat; Q1 to Q6 open; phases 01 to 06 drafted.
- 2026-11-20 : folded in Q1 to Q6: the functional spec does not bind this game (damage, not combat); marks as quads; each hit may break the nearest mast, more likely as health drops; a rough share of speed per mast above a floor; no healing; the brig not saved.
- 2026-11-20 : the user validated the plan ("commit and go"); phases planned.
- 2026-11-20 : phase 01 - `ShipDamage` (health, blows from `HullContact.Struck`, `Hit`), the top speed, sink and list from damage; tests; gates green, a capture of the damaged sloop. Hit points not kept yet (phases 03, 04).
- 2026-11-20 : phase 02 - sinking (`ShipDamage`, `WaveRider`), `GameState.GameOver`, the game over screen, the save deleted; menu quit no longer writes an empty save; gates green, a sinking capture, dev APK.
- 2026-11-20 : phone test of the phase 02 APK: the sloop took about 13 rams on the brig to fall to 6% health, since a slower ship rams softer; the user chose to raise the speed floor from 0.4 to 0.6 to keep it playable (rams then also hit harder late on).
- 2026-11-20 : phase 03 review: a filled `Image` draws nothing without a sprite, so the bar's fill is a child stretched to the health; the colours are two fields on `HullBar`, not a style asset; health is derived from the damage on each side; the bar binds after the restore; the phone's version 3 save will reset.
- 2026-11-20 : phase 03 - `ShipDamage` keeps the latest 16 hits in hull space, saves and restores; `GameSaveData` version 4 with `damage` (the health lost saved on its own: the sides' sum rounds, correcting the review's line above); `HullBar` under the minimap, green to yellow to red; gates green (EditMode 215, PlayMode 24), a HUD capture.
- 2026-11-20 : phase 04 review: the hull has no mesh collider to ray against, and above the water its sides are vertical at the builder's `half_beam`, so the type carries that profile and marks sit on it; one mesh of 16 quads per ship (one draw) instead of a pool of quads (up to 32 draws against the 80 budget); a `Restored` event so restored hits are drawn.
- 2026-11-20 : phase 04 - `T_HullMarks` atlas (`textures.py hull_marks`), `M_HullMark`, the hull profile on `SO_ShipType` sampled from the builder, `HullMarks` on `PF_Ship`'s hull (one mesh, off when whole), `ShipDamage.Restored`; marks made bigger after the first capture; gates green (EditMode 221, PlayMode 24), a ram capture.
- 2026-11-20 : phase 05 - the builder splits each mast (`<Name>Mast<i>`, its sail states, its stump), clearance 0 on both ships; `ShipMasts`, `MastParts`, `FallingMast`; mast numbers on `SO_ShipType`; the speed's share per mast; broken masts saved; `BuiltModelTests` for the masts and the hull's side; `CAPTURE_BREAK_MAST`; gates green (EditMode 232, PlayMode 24), a capture of both ships losing a mast; 57 draws with both in view.
- 2026-11-20 : phase 06 - dev APK built from `01f3eff` (clean tree, 74,495,159 bytes) with phases 01 to 05; the phone run is the user's. The fresh-clone gate run is not done yet (a full import, about ten minutes).
- 2026-11-21 : phase 06 - phone run of the `01f3eff` APK: the sloop sank after 9 hits to game over, a mast broke, quit and Continue restored health, hits, position and the broken mast; the user saw the marks and the bar's colour; 60 fps, no problems. The fresh-clone gate run was not done. Phase and folder done.
