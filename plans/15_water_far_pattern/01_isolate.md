---
status: in progress
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
