# implementation tracking

Ropes that read as rope (a laid texture, tar and hemp, sag, deadeyes and blocks) and the giant's boats as open boats on curved wooden davits.
Analysis and decisions in [`00_start.md`](00_start.md).

## Key decisions

- Sag on the running rigging, with the clearance check following each rope's segments (`00_start.md` Q1).
- Two rope slots: `Tar` for the standing rigging, dark brown; `Hemp` for the running rigging (Q2).
- Open boats with a transom, sheer, thwarts and oars, on curved `Spar` davits with blocks and hemp falls; the giant only (Q3, Q4).
- Probe renders while building; a helm and a chase capture closing each of phases 1 to 3; the phone's read at the end (Q5).
- Cost is the phone's frame time, not a triangle count.

## Phases

| #  | Phase                    | Plan                                       | Status      |
| -- | ------------------------ | ------------------------------------------ | ----------- |
| 1  | Rope look                | [`01_rope_look.md`](01_rope_look.md)       | planned     |
| 2  | Sag and rope ends        | [`02_sag_and_ends.md`](02_sag_and_ends.md) | planned     |
| 3  | Boats and davits         | [`03_boats.md`](03_boats.md)               | planned     |
| 4  | Verify                   | [`04_verify.md`](04_verify.md)             | planned     |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-25 : cut feat/16_ship_detail from main (12033bb); read the rope, boat and davit code and the Tar material; analysis and Q1 to Q5 in 00_start.md.
- 2026-11-26 : Q1 c (sag, clearance along segments), Q2 a, Q3 a, Q4 a, Q5 a folded in; phases 1 to 4 written.
