# Ship graphics - implementation tracking

Better-looking ship fittings: the lantern, the helm, the hatches and a companionway, the bulwark and its rail, the masts and the taffrail, with wood textures where the parts are flat colour and more detail in the shapes. Judged by the phone's frame times rather than by draw and triangle counts.
Analysis, proposals and answers in [`00_start.md`](00_start.md).

## Key decisions

- Both textures and shapes, the wood first; all of it in the ship builder, both ships from one change.
- The counts in `docs/performance-budget.md` are guides; the phone's CPU and GPU frame times, against a baseline taken before any change, are the measure.
- `ShipWood` split into `Spar` (grain along) and `ShipWood` (boards), each with a normal map; the lantern's glass in its own emissive material.
- Gratings as bars, not an alpha-cut texture (alpha clipping is the costly case on the Mali); no realtime light in the lantern.
- The sloop keeps a tiller; one companionway per ship; the taffrail an open rail on balusters.

## Phases

| #  | Phase                          | Plan | Status  |
| -- | ------------------------------ | ---- | ------- |
| 01 | Frame times and a baseline     | [`01_frame_times.md`](01_frame_times.md) | in progress |
| 02 | Wood                           | [`02_wood.md`](02_wood.md) | planned |
| 03 | Lantern and helm               | [`03_lantern_helm.md`](03_lantern_helm.md) | planned |
| 04 | Hatches, bulwark and rail      | [`04_hatches_bulwark.md`](04_hatches_bulwark.md) | planned |
| 05 | Masts and taffrail             | [`05_masts_taffrail.md`](05_masts_taffrail.md) | planned |
| 06 | Verify                         | [`06_verify.md`](06_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-21 : bootstrapped the folder on `feat/10_ship_graphics` from `main` at `c8e1dee`: close captures of both ships in `Game.unity`, the builder read for each part, triangle counts (sloop 6 132, brig 10 788); proposals and Q1 to Q5.
- 2026-11-21 : the user: the draw and triangle limits are proxies, not fixed; reassessed: a first phase for CPU and GPU frame times on the phone, two wood materials with normal maps, rounder masts, the lantern's own material; Q6 on the budget doc.
- 2026-11-21 : folded in Q1 to Q6, every recommendation accepted; phases 01 to 06 planned.
- 2026-11-21 : review of the sub-plans: checked the claims they rest on (the report already has session and 60 s sections; `DebugOverlayController` already reads `FrameTimingManager`; frame timing stats are off; boxes share 8 vertices and poles carry no UVs); no findings.
- 2026-11-21 : phase 01 - frame timing stats on; `DiagnosticsRecorder` reads `FrameTimingManager` each frame (frames with no timing left out); `cpu ms` and `gpu ms` rows in the session and last-minute statistics, or "not reported by this device"; budget doc and logging doc updated; gates green (EditMode 233, PlayMode 24). The baseline waits for the phone.
