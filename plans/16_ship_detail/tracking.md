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
| 1  | Rope look                | [`01_rope_look.md`](01_rope_look.md)       | done        |
| 2  | Sag and rope ends        | [`02_sag_and_ends.md`](02_sag_and_ends.md) | done        |
| 3  | Boats and davits         | [`03_boats.md`](03_boats.md)               | done        |
| 4  | Verify                   | [`04_verify.md`](04_verify.md)             | planned     |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-25 : cut feat/16_ship_detail from main (12033bb); read the rope, boat and davit code and the Tar material; analysis and Q1 to Q5 in 00_start.md.
- 2026-11-26 : Q1 c (sag, clearance along segments), Q2 a, Q3 a, Q4 a, Q5 a folded in; phases 1 to 4 written.
- 2026-11-26 : phase 1 review: `line` also draws the head rails in `Trim`, so the rope is a primitive of its own; footropes are ropes too; about 1180 ropes shown on the giant (7.2k triangles), three sides kept; tangents import as MikkTSpace.
- 2026-11-26 : phase 1 done: T_Rope and its normal map, the rope primitive mapped by diameter, hemp running rigging; tar raised to a lighter brown after the first helm capture read blue-black. Gates green, clearance 0.
- 2026-11-26 : phase 2 review: no Python tests in the builder, a scratch check instead; ratlines stay straight (about 1000 on the giant); straight ropes keep two points; clearance is the only reader of a rope's ends (checked).
- 2026-11-26 : phase 2 done: ropes sag by kind, clearance along segments (0 everywhere), deadeyes and lanyards, brace blocks; giant +4.0k triangles. The tarred ropes glinted silver against the light: rope materials matte (no environment reflections or highlights). Gates green.
- 2026-11-26 : phase 3 review: the giant's boats hang along its open rail, so the davits stand outside the posts on the side below the sill, clear of the tumblehome; the falls hang vertical, so no sag; nothing else found.
- 2026-11-26 : phase 3 done: open boats with transom, sheer, thwarts and oars on curved spar davits with blocks and hemp falls; giant 49.7k triangles. FightTests picked the anchored giant after the reimport changed the search order; it now takes the anchored ship nearest the start. Gates green.
