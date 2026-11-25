# Water far pattern - implementation tracking

The far water seen from the chase camera shows the small ripples lined up into faint bands of lines.
Find which term of the water draws them and break it up. Analysis, leads and questions in [`00_start.md`](00_start.md).

## Key decisions

- Isolate before fixing: a capture that reproduces it, then one run per lead with that term off (00_start Q1).
- The last phase is the user's read on the phone (00_start Q2).

## Phases

| #  | Phase                 | Plan | Status |
| -- | --------------------- | ---- | ------ |
| 01 | Reproduce and isolate | [`01_isolate.md`](01_isolate.md) | done |
| 02 | Fix                   | [`02_fix.md`](02_fix.md) | done |
| 03 | Verify                | [`03_verify.md`](03_verify.md) | in progress |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-25 : picked up on feat/15_water_far_pattern, recut from main; analysis from the shader, the settings and the map's generator: the map is diagonal streaks, three unrotated layers. Q1 and Q2 answered a.
- 2026-11-25 : phase 01 done - three captures behind the giant: the lines are the Gerstner waves far out, where only the 40 m and 24 m waves are resolved and draw a regular interference; the detail map alone is an even grain; the patch layer partly hides the lines and adds a diagonal bias. Ripple square and anisotropy not run. Fix question raised as 00_start Q3.
- 2026-11-25 : phase 02 done - Q3 a: the far waves' strength swings in patches (the detail map at about 140 m a repeat, two readings), the patch layer turned 37 degrees; 280 m patches tried first, still lines. Captures before and after behind the giant. Gates green: EditMode 306/306, PlayMode 32/32. Phase 03 started: the dev APK.
