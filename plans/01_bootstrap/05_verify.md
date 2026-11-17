---
status: planned
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
