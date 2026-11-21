---
status: planned
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
