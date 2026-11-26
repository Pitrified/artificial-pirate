---
status: done
---

# Phase 4 - Verify

## Overview

The docs and the phone.
Context: [`00_start.md`](00_start.md).

## Goals

1. `docs/building_assets.md`: the rope texture and slots, sag and the clearance check along segments, deadeyes and blocks, the open boats and davits; its stop condition in line with `docs/performance-budget.md` (starting guesses, the phone's frame time decides).
2. The spec: nothing in the game's scope changes (look only); checked, `Tools/spec_check.py` green.
3. A dev APK; the user's read on the phone (the ropes from the helm and the chase camera, the boats) and the diagnostics report near the giant.

## Done when

- Docs in the branch; gates green; the user's read and report.

## What the implementation found
The docs went in with the phases' findings; the spec check stayed green (look only, nothing in the game's scope changed).
Phone run on dev 44b9053, the player in the giant fighting both brigs and the anchored giant: 60 fps, GPU p50 12.3 ms and p95 13.3 ms (12.3 and 13.8 after the water work), CPU p50 16.8 ms, 5 slow frames, 0 problems.
The user's read, asked about the ropes, the sag, the far braces and the boats: "looks good"; no dashing of the far braces reported.
