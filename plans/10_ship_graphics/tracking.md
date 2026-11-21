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
| 02 | Wood                           | [`02_wood.md`](02_wood.md) | done |
| 03 | Lantern and helm               | [`03_lantern_helm.md`](03_lantern_helm.md) | done |
| 04 | Hatches, bulwark and rail      | [`04_hatches_bulwark.md`](04_hatches_bulwark.md) | done |
| 05 | Masts and taffrail             | [`05_masts_taffrail.md`](05_masts_taffrail.md) | done |
| 06 | Verify                         | [`06_verify.md`](06_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-21 : bootstrapped the folder on `feat/10_ship_graphics` from `main` at `c8e1dee`: close captures of both ships in `Game.unity`, the builder read for each part, triangle counts (sloop 6 132, brig 10 788); proposals and Q1 to Q5.
- 2026-11-21 : the user: the draw and triangle limits are proxies, not fixed; reassessed: a first phase for CPU and GPU frame times on the phone, two wood materials with normal maps, rounder masts, the lantern's own material; Q6 on the budget doc.
- 2026-11-21 : folded in Q1 to Q6, every recommendation accepted; phases 01 to 06 planned.
- 2026-11-21 : review of the sub-plans: checked the claims they rest on (the report already has session and 60 s sections; `DebugOverlayController` already reads `FrameTimingManager`; frame timing stats are off; boxes share 8 vertices and poles carry no UVs); no findings.
- 2026-11-21 : phase 01 - frame timing stats on; `DiagnosticsRecorder` reads `FrameTimingManager` each frame (frames with no timing left out); `cpu ms` and `gpu ms` rows in the session and last-minute statistics, or "not reported by this device"; budget doc and logging doc updated; gates green (EditMode 233, PlayMode 24). The baseline waits for the phone.
- 2026-11-21 : review of phase 02 before coding: the plan's "boxes need 24 vertices" replaced by per-corner UVs on `MeshParts` (also fixes a pole's seam once smooth); tops and crosstrees in `Spar`, so masts add no draw.
- 2026-11-21 : phase 02 - `T_Spar` (long grain) and `T_ShipWood` (0.25 m boards), each with a normal map, 2 m tiles; `M_Spar` new, `M_ShipWood` textured; masts 16 sides, spars 10, smooth; UVs on boxes, poles, the stump and lofted strips; captures show grain on the masts and boards on the bulwark; gates green (EditMode 233, PlayMode 24). Phone time waits for phase 06.
- 2026-11-21 : phase 03 - review before coding found nothing to correct. Lantern: iron frame, glass in `M_LanternGlass` (emissive), brass cap and ring; brig wheel: rim, 8 spokes with handles, hub, pedestal; sloop tiller: rudder head, rising shaft, grip. The glow first did not show: URP needs emissive GI flags (doc updated). Gates green (EditMode 233, PlayMode 24).
- 2026-11-21 : phase 04 - review before coding: the companionway named by hatch index, not `t` (plan corrected); crew checked clear of the hatches. Coamings round a `Tar` opening, gratings of crossed bars, a companionway hood with a ladder (sloop main hatch, brig after hatch), timberheads every third stanchion, a chamfered cap rail. Gates green (EditMode 233, PlayMode 24).
- 2026-11-21 : phase 05 - review before coding found nothing to correct. Mast hoops, partners and a cap; a rim on the tops; the stump keeps the collar and hoops below its break; the taffrail open: balusters in `Trim`, gilt cap rail, carved board and boss, scrolls, quarter rails. A break-mast sequence still brings the mast down. Gates green (EditMode 233, PlayMode 24).
