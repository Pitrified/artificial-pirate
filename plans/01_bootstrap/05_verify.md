---
status: done
---

# Phase 05 - Verify

## Overview

The repo as a newcomer gets it: a fresh clone, nothing from this session's working tree.

## Plan

- On the box, in a fresh clone of the branch:
  - `git lfs ls-files` is empty and `.gitattributes` has the LFS rules;
  - `Tools/setup.sh` runs;
  - compile with 0 errors and 0 warnings, EditMode and PlayMode green, from a fresh `Library/`;
  - `Tools/build-android.sh dev` builds `com.pitrified.artificialpirate`;
  - link check and `python3 Tools/plans.py check` pass.
- HUMAN, from g4: push the branch to `Pitrified/artificial-pirate`, then `main`.
- HUMAN, on a device: the `dev` APK installs next to any build of the guide (different id), the vertical slice plays, and logcat shows no errors or exceptions from the game (the source's phase 8 item).

## Done when

- The box checks pass in the fresh clone.
- The device run is recorded in [`tracking.md`](tracking.md)'s log.

## What the implementation found

- 2026-11-17, box checks in a fresh clone of `feat/01_bootstrap` at `8838925`, all pass:
  `git lfs ls-files` empty with the LFS rules in `.gitattributes`; `Tools/setup.sh` exit 0 (warns that `bundletool` is missing, needed only for AABs); `plans.py check` and the link check pass;
  compile exit 0 with 0 warnings after the one-time Shader Graph import errors; EditMode 114/114; PlayMode 4/4; `dev` APK 70 MB, `com.pitrified.artificialpirate`, `Artificial Pirate`, `arm64-v8a`.
- The first clone, in the session's scratch directory under `/tmp`, failed to import packages with error -122 (`EDQUOT`): `/tmp` on the box is a RAM-backed tmpfs with a per-user quota. A Unity project with its `Library/` belongs on disk.
- Open: the HUMAN push and device run.
- 2026-11-17, device run by the user: the `dev` APK, copied to the phone with `scp` from Termux and installed from Downloads, works.
  No adb on the phone (no USB to a computer, no wireless debugging), so logcat was not read; that check moves to [`03_performance`](../03_performance/00_start.md), whose profiling session has the phone on USB anyway.
