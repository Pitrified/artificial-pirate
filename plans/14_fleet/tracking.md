# Fleet - implementation tracking

Gun carriages in place of boxes, sails trimmed to a wind as the ship turns, chase and stern guns, a middle ship type, and a giant galleon the size of the Amerigo Vespucci. Analysis, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- The sails' trims are baked in bands, one shown per mast by the heading against a wind per world; the wind is seen, not sailed.
- Chase and stern guns are batteries by bearing in `Broadside`, carried by the frigate and the giant only.
- The frigate brings the builder's parts for bigger ships; the giant has the Vespucci's look and lies at anchor in a start clearing widened for it.

## Phases

| #  | Phase                | Plan | Status |
| -- | -------------------- | ---- | ------ |
| 01 | Carriages            | [`01_carriages.md`](01_carriages.md) | planned |
| 02 | The wind             | [`02_wind.md`](02_wind.md) | planned |
| 03 | Chase and stern guns | [`03_chase_guns.md`](03_chase_guns.md) | planned |
| 04 | The frigate          | [`04_frigate.md`](04_frigate.md) | planned |
| 05 | The giant            | [`05_giant.md`](05_giant.md) | planned |
| 06 | Verify               | [`06_verify.md`](06_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-24 : bootstrapped the folder on `feat/14_fleet` from `main` at `6a7c4d7`: carriages are boxes; the sails baked for one wind, which the game does not have; two batteries, beam only; two types over a shared builder with one flush deck and one row of ports. Proposed truck carriages, a wind per world with baked trim bands, batteries by bearing, a frigate as the middle type, a galleon at the Vespucci's size. Q1 to Q7.
- 2026-11-24 : the user: Q1 a (test how the jumps look; the ropes matter), Q2 a (the sailed wind a note), Q3 a, Q4 a, Q5 b (the Vespucci's own look), Q6 a (islands for big ships a note), Q7 a. Folded in; sub-plans 01 to 06 written. Review of the sub-plans: the flag's origin is the model's, not the masthead (the wind phase builds it at the masthead); the boom's clearance loop runs per band; the frigate's strength figure marked as one to try. `VolleyFired`'s one subscriber is `GameSceneWiring`. Waiting on the user's go.
- 2026-11-24 : the user: the islands are placed further from the start, so the giant anchors in a widened start clearing (`StartClearing`, 160 m now). Phase 05 corrected.
