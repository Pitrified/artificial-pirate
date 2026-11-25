# Water far pattern - implementation tracking

The far water seen from the chase camera shows the small ripples lined up into faint bands of lines.
Find which term of the water draws them and break it up. Analysis, leads and questions in [`00_start.md`](00_start.md).

## Key decisions

- Isolate before fixing: a capture that reproduces it, then one run per lead with that term off (00_start Q1).
- The last phase is the user's read on the phone (00_start Q2).

## Phases

| #  | Phase                 | Plan | Status |
| -- | --------------------- | ---- | ------ |
| 01 | Reproduce and isolate | [`01_isolate.md`](01_isolate.md) | in progress |
| 02 | Fix                   | [`02_fix.md`](02_fix.md) | planned |
| 03 | Verify                | [`03_verify.md`](03_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-25 : picked up on feat/15_water_far_pattern, recut from main; analysis from the shader, the settings and the map's generator: the map is diagonal streaks, three unrotated layers. Q1 and Q2 answered a.
