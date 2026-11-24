# Fleet - implementation tracking

Gun carriages in place of boxes, sails trimmed to a wind as the ship turns, chase and stern guns, a middle ship type, and a giant galleon the size of the Amerigo Vespucci. Analysis, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- The sails' trims are baked in bands, one shown per mast by the heading against a wind per world; the wind is seen, not sailed.
- Chase and stern guns are batteries by bearing in `Broadside`, carried by the frigate and the giant only.
- The frigate brings the builder's parts for bigger ships; the giant has the Vespucci's look and lies at anchor in a start clearing widened for it.

## Phases

| #  | Phase                | Plan | Status |
| -- | -------------------- | ---- | ------ |
| 01 | Carriages            | [`01_carriages.md`](01_carriages.md) | done |
| 02 | The wind             | [`02_wind.md`](02_wind.md) | done |
| 03 | Chase and stern guns | [`03_chase_guns.md`](03_chase_guns.md) | done |
| 04 | The frigate          | [`04_frigate.md`](04_frigate.md) | done |
| 05 | The giant            | [`05_giant.md`](05_giant.md) | planned |
| 06 | Verify               | [`06_verify.md`](06_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-24 : bootstrapped the folder on `feat/14_fleet` from `main` at `6a7c4d7`: carriages are boxes; the sails baked for one wind, which the game does not have; two batteries, beam only; two types over a shared builder with one flush deck and one row of ports. Proposed truck carriages, a wind per world with baked trim bands, batteries by bearing, a frigate as the middle type, a galleon at the Vespucci's size. Q1 to Q7.
- 2026-11-24 : the user: Q1 a (test how the jumps look; the ropes matter), Q2 a (the sailed wind a note), Q3 a, Q4 a, Q5 b (the Vespucci's own look), Q6 a (islands for big ships a note), Q7 a. Folded in; sub-plans 01 to 06 written. Review of the sub-plans: the flag's origin is the model's, not the masthead (the wind phase builds it at the masthead); the boom's clearance loop runs per band; the frigate's strength figure marked as one to try. `VolleyFired`'s one subscriber is `GameSceneWiring`. Waiting on the user's go.
- 2026-11-24 : the user: the islands are placed further from the start, so the giant anchors in a widened start clearing (`StartClearing`, 160 m now). Phase 05 corrected.
- 2026-11-24 : phase 01 - review before: the ports sat 0.3 to 0.45 m over the deck, too low for a truck carriage; `gun_ports.above_deck` puts each port over the deck at its station. Truck carriages and a ringed barrel; sloop and brig rebuilt, the types' guns moved up. About 390 triangles a gun. Captures from both helms. Gates green (EditMode 263, PlayMode 31).
- 2026-11-24 : phase 02 - review before: the trim's formula only suited the one wind; the flag's origin was the model's; the scene chose set or furled by per-object overrides. A wind per world from the seed; seven bands baked per mast (close-hauled at 60 degrees: at 45 the sails pressed into the shrouds), `Trim.eased` clearing every band; `ShipSails` showing the band by the heading with 8 degrees of hysteresis, furled at anchor, a broken mast left alone, the flag turned downwind. 13 new tests. Gates green (EditMode 276, PlayMode 31). The phone judges the jumps in phase 06.
- 2026-11-24 : phase 03 - review before: no type carries chasers yet, so the builder's bow and stern ports moved to phase 04. `Battery` and four batteries by bearing in `Broadside`; chase arc 30 degrees; the type's bow and stern guns; a barrel's rest from its trunnions to its mouth. 3 new tests, the broadside and model tests moved to batteries. Gates green (EditMode 279, PlayMode 31).
- 2026-11-24 : phase 04 - review before: the builder had one flush deck and one row of ports. Raised decks, gun port rows (open on carriages, covered showing the muzzles), chase and stern ports; the frigate after the Hermione (reference CC BY 2.0), its rig scaled from the brig's after my first numbers crossed ropes at every wind; close-hauled moved to 67.5 degrees for every type and a test ties the trim asset to the builder. Type, variant, icon, scene ship, ship choice. Helm camera beside the wheel. Gates green (EditMode 290, PlayMode 31).
