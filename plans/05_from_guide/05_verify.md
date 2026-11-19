---
status: in progress
---

# Phase 05 - Verify

## Plan

- Fresh clone: gates green, dev APK.
- HUMAN on the phone: the ship on the water at 60 fps; a diagnostics report pasted here.

## Done when

- The phone report in this file.

## What the implementation found

- 2026-11-19. Fresh clone of `feat/05_from_guide` at `c2d4010` (LFS files real): gates green, compile 0 warnings, EditMode 136/136, PlayMode 16/16. The dev APK is the working tree's from `1695b57` (73,684,166 bytes; `c2d4010` changes a doc only).
- 2026-11-19, **the phone report** (Pixel 7 Pro, build `dev 25c95ca+dirty` of 23:17Z: the phase 04 tree, built before its commit `1695b57`): 74.7 s, 70 s of it playing. Frame time 16.8 ms at p05, median and p95: **60 fps held** with the water, the floating props and the riding ship. Memory 147 MB, the same as the guide's sample; heap flat at 10.5 MB; `gc alloc kB` 0.1 a frame at p05, median and p95; 7 collections, at the loads. 8 slow frames (0.19%): the boot and the Game scene's load (300 and 233 ms), one 33 ms frame at 53 s, the pause at 70 s, 133 ms opening the Diagnostics panel at 72 s (heap 11 to 14 MB, building the report). No problems.
- 2026-11-19, **the look on the phone**, from the user: the ship bobs right; a barrel was pushed by the ship and "interacted well"; the wake "looks a bit weird": a chain of circles, the main complaint, it stops where the ripple square ends, and it is a bit strong. Fixed in phase 06 ([`06_wake.md`](06_wake.md)).
