# implementation tracking

Villages to find on islands, with a pier for two brigs, and the world scale they need first: regions of open sea and archipelagos over the cell grid, the gaps halved, a settled island large enough for a village.
Analysis and decisions in [`00_start.md`](00_start.md).

## Key decisions

- Ships, buildings and the pier at true size; islands and gaps compressed (`00_start.md` Q1).
- A small village of 8 to 12 houses, a storehouse and a tavern; a pier about 50 x 8 m, a brig on each face (Q2, Q3).
- A region grid over the cells: open sea, archipelago, settled archipelago, one village per settled one (Q5).
- Gaps of 150 to 300 m inside an archipelago, 0.5 to 1 km between, a village every 1.5 to 3 minutes; starting targets, tuned on the phone (Q6).
- A village is seen and marked, nothing more; using the dock went to the spec's Next list (Q8).
- A fixed village near the start besides the generated ones (Q9).
- No battery, nothing to shorten crossings (Q4, Q7).
- Old saves load a different world; no migration before the first release (`docs/systems/save-system.md`).

## Phases

| #  | Phase                          | Plan                                             | Status  |
| -- | ------------------------------ | ------------------------------------------------ | ------- |
| 1  | Regions and the settled island | [`01_regions.md`](01_regions.md)                 | planned |
| 2  | Village and pier models        | [`02_village_models.md`](02_village_models.md)   | planned |
| 3  | Placement and the map          | [`03_placement.md`](03_placement.md)             | planned |
| 4  | Verify                         | [`04_verify.md`](04_verify.md)                   | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-26 : cut feat/17_island_finds; read the world, ships and budget; two research passes on real sizes and other games; analysis and Q1 to Q10 in 00_start.md.
- 2026-11-26 : the note that old saves need not survive before the first release went into `docs/systems/save-system.md`.
- 2026-11-28 : Q1 to Q10 folded in (Q3 a for two brigs, Q6 b, the rest as recommended); phases 1 to 4 written.
