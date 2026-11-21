---
status: planned
---

# Phase 01 - Frame times and a baseline

## Overview

The game is capped at 60 fps, so the frame rate only says a frame fits in 16.7 ms, not how much room is left.
This phase puts the CPU and GPU frame times in the diagnostics report and takes a baseline on the phone, so each later phase is measured against it.
Context: [`00_start.md`](00_start.md) Q6.

## Goals

1. CPU and GPU frame times, as histograms (min, p05, p50, mean, p95, max), in the diagnostics report, for the session and the last 60 s.
2. `docs/performance-budget.md` reads the counts as guides and the frame times as the measure.
3. A baseline report from the phone before any graphics change.

## Plan

- Turn on `enableFrameTimingStats` in the player settings, through an editor script in batch mode (deleted before the commit).
- `DiagnosticsRecorder`: read `FrameTimingManager` each frame into two histograms, the way it records frame ms now; no allocation per frame (a preallocated `FrameTiming[1]`, as `DebugOverlayController` has).
- `DiagnosticsReport`: two rows, `cpu ms` and `gpu ms`; when the device reports no timings, one line saying so instead of rows of zeros.
- `docs/performance-budget.md`: the draw and triangle numbers kept, marked as guides that point at a cost; the phone's GPU and CPU times at p95 as the measure; how to read them from the report.
- `docs/systems/` page for diagnostics, if there is one, gets the two rows.
- A dev APK; the user runs it: sail around both ships for a minute with the camera on them, paste the report.

## Out of scope

- Any change to the ships.
- Per-pass GPU profiling (the GPU time is the whole frame).

## Done when

- Gates green; a test for the report's two rows and the no-timings line.
- The phone's report carries nonzero GPU times, written in the log as the baseline with the pose it was taken from.

## Pitfalls

- On some Android drivers `gpuFrameTime` stays 0 even with the stats on; then the fallback is the frame time with the cap lifted for a run (a debug toggle), raised as a question before building it.
- Frame timing stats cost a little themselves; measure with them on in every run, baseline included, so the comparison is fair.
