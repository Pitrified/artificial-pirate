---
status: done
---

# Phase 03 - Lantern and helm

## Overview

The lantern reads as a yellow block and the brig's wheel as a millstone. Both get real shapes; the lantern's glass glows.
Context: [`00_start.md`](00_start.md) "Lantern", "Helm", Q2, Q3; depends on [`02_wood.md`](02_wood.md).

## Goals

1. A lantern with a frame, glass panes and a cap, the glass in its own emissive material.
2. A wheel on the brig: rim, hub, spokes with handles, a pedestal.
3. The sloop's tiller made better: a rudder head at the transom, a curve, a grip.

## Plan

- `props.lantern`: four iron corner posts, top and bottom plates, a glass pane inset on each side in a `LanternGlass` slot, the brass cap with a ring; the post kept.
- `M_LanternGlass`: URP Lit, warm emission, so it reads lit in daylight; no realtime light.
- `fittings.furniture`, wheel: a ring of 16 segments, a hub, 8 spokes running past the rim as handles, two posts and a cross piece as the pedestal; all `Spar`.
- Tiller: a rudder head standing above the transom, the tiller from it in two or three segments rising slightly, a thinner grip at the end.
- Rebuild, captures from the stern poses, counts logged.

## Out of scope

- A light from the lantern; a night.
- The capstan.

## Done when

- Gates green; captures of both sterns show a lantern with panes and a spoked wheel.
- Counts logged.

## What the implementation found

- URP rewrites `_EMISSION` from the material's GI flags on import, so the glow set by script vanished on the next start until the flags were `RealtimeEmissive`; written into `docs/building_assets.md` pitfalls.
- The first glow (emission 2.5) tonemapped to a pale cream; 1.3 on a darker base reads as a warm lit lamp.
- A `ring` helper in `mesh.py` (a smooth hoop) for the rim and the lantern's ring, ready for the mast hoops.
- Counts: sloop 6 616 triangles, brig 12 016; both ships in view 56 226 triangles, 48 material slots.
