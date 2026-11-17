---
status: draft
priority: 0
description: |
  Profile the profile APK on a mid-range Android device against docs/performance-budget.md and fix
  regressions in the budget's order until five minutes of play hold the budget.
---

# Performance pass

Moved on 2026-11-17 from phase 10 of [unity-setup-project's `docs/tracking.md`](https://github.com/Pitrified/unity-setup-project/blob/main/docs/tracking.md#phase-10---performance-pass), when the game was split into this repo.
Nothing of it was started there.

## Plan

The reference device is a Pixel 7 (the budget and `functional-specs.md` §2). The budget and the protocol are [`docs/performance-budget.md`](../../docs/performance-budget.md) (§6 profiling, §7 the order to fix things in).

- HUMAN: build the `profile` APK (`Tools/build-android.sh profile`; the agent can build it on the box) and attach the Unity Profiler to the device.
- HUMAN: capture 60 s of play and compare it with the budget.
- HUMAN: a Frame Debugger snapshot; draw calls at most 80.
- AI: fix what is over budget, in the order of §7; each fix closes on the batch gates.
- HUMAN: profile again, until the budget holds for 5 minutes of sustained play.

## Done when

- A 5-minute capture on the reference device within budget, its numbers in the log.
