---
status: done
---

# Phase 01 - Reproduce and isolate

## Overview

Find which term draws the lines before choosing a fix.
Context: [`00_start.md`](00_start.md).

## Goals

1. A capture from the game's chase camera behind the giant that shows the lines.
2. One run per lead with its term off: the detail map's three layers (lead 1), the filtering (lead 2), the Gerstner waves (lead 3), the ripple square (lead 4).

## Plan

- `SequenceCaptureTests` with CAPTURE_SHIP Giant and the chase camera, a few frames, at the phone's aspect.
- The toggles are temporary edits to `Water.shader` (and the map's import for anisotropy), reverted before any commit; frames go to the session scratchpad.
- Up to five runs, about 3 to 5 minutes each.

## Out of scope

- Any fix that stays.

## Done when

- The lead (or leads) drawing the lines is named, with the frames that show it, in "What the implementation found".

## What the implementation found

Captures of the game's chase camera behind the giant at 1872x864 (the Pixel's 2340x1080 at render scale 0.8), frames in the session scratchpad; the shader edits reverted after each run.

- As now: rows of bands across the middle distance, with a diagonal grain.
- Detail off (the waves alone): long, straight, evenly spaced parallel crests out to the horizon. This is the pattern.
  Far out the short waves fade by the pixel rule, so only the 40 m and 24 m waves light the water, and two plane waves draw a perfectly regular interference.
- Waves off (the detail alone): an even grain, a faint diagonal bias from the map's streaks, no rows.
- The 83 m patch layer off: the near layers mip down to flat a little way out, and the waves' lines show bare. The patch layer is what partly hides them now; its own diagonal bias adds the grain.
- Not run: the ripple square (about 24 m around the ship, the lines are far past it) and anisotropic filtering (a fix to try, not a cause).
- So lead 3 is the cause, the patch layer's streaks a lesser part of lead 1.
