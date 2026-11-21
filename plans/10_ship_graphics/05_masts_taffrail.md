---
status: done
---

# Phase 05 - Masts and taffrail

## Overview

After the wood and rounder poles of phase 02, the masts get their ironwork and fittings, and the taffrail stops being one gilt box.
Context: [`00_start.md`](00_start.md) "Masts", "Taffrail", Q5; depends on [`02_wood.md`](02_wood.md).

## Goals

1. Iron hoops up the lower masts, partners at the deck, a cap at each masthead, a rim on the tops.
2. The taffrail an open rail: balusters under a gilt cap rail, a carved board in the middle, scroll ends, quarter rails along the stern deck.

## Plan

- `rig.standing`: thin `Tar` rings every 1.5 to 2 m on the lower mast; a collar of wedges at the foot (in the mast's own object, so it goes with the mast); a cap block at the head. Tops: a rim around the platform.
- The stump: keeps its hoops below the break, so a broken mast matches the standing one.
- `fittings.stern`: a gilt cap rail across the stern, turned balusters under it (a lathe profile, `Spar` or `Trim`), a carved board at the middle and scroll ends in `Gilt`; quarter rails from the corners forward to the bulwark. The lantern's post stands on the new rail.
- Rebuild; `BuiltModelTests` for the mast objects; captures from the mast and stern poses, counts logged.

## Out of scope

- The running rigging and blocks.

## Done when

- Gates green; mast and stern captures show hoops, caps and the open taffrail.
- Breaking a mast still works (`ShipMastsTests`, `CAPTURE_BREAK_MAST`).
- Counts logged.

## What the implementation found

- The hoops and partners come from one helper shared by the mast and its stump. The brig's break starts about 0.95 m above the deck, under the first hoop at 1 m, so its stumps keep only the collar; the sloop's keeps one hoop.
- The balusters are `Trim`, so each ship's paint carries to them (red on the sloop, the brig's own trim); rails and carving `Gilt`. No new slot.
- `BuiltModelTests` needed no change: the mast objects and slots are the same.
- Counts: sloop 9 548 triangles (hull 5 038, mast 1 788), brig 16 444; both ships in view 63 278 triangles, 48 material slots, against 55 218 and 46 after phase 02.
