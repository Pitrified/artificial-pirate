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
| 01 | Hull damage and handling | [`01_hull_damage.md`](01_hull_damage.md) | planned |
| 02 | Sinking and game over    | [`02_sinking.md`](02_sinking.md) | planned |
| 03 | HUD bar and save         | [`03_hud_and_save.md`](03_hud_and_save.md) | planned |
| 04 | Marks on the hull        | [`04_hull_marks.md`](04_hull_marks.md) | planned |
| 05 | Masts that break         | [`05_masts.md`](05_masts.md) | planned |
| 06 | Verify                   | [`06_verify.md`](06_verify.md) | planned |

## Log

- 2026-11-20 : bootstrapped the folder on `feat/09_ship_damage`: research, options A to E, the user chose A, B and C and answered five questions in chat; Q1 to Q6 open; phases 01 to 06 drafted.
- 2026-11-20 : folded in Q1 to Q6: the functional spec does not bind this game (damage, not combat); marks as quads; each hit may break the nearest mast, more likely as health drops; a rough share of speed per mast above a floor; no healing; the brig not saved.
- 2026-11-20 : the user validated the plan ("commit and go"); phases planned.
