---
status: done
---

# Phase 02 - Renames

## Overview

Source Q2, Q7 and Q8: the repo, the Unity project and the Android app all take the game's name.

## Plan

- `git mv UnitySetupPrpj ArtificialPirate`; `Tools/build-android.sh`, `.gitignore`, `.gitattributes`, `.vscode/settings.json` and the docs follow.
- `ProjectSettings.asset` through a temporary editor method run with `-executeMethod`, deleted before the commit:
  `productName` `Artificial Pirate`, Android `applicationIdentifier` `com.pitrified.artificialpirate`, and the Windows Store package name and description, which carry the old folder name.
- `artificial-pi` to `artificial-pirate` in scripts, docs, `BuildPipeline.ProductName`, the `CreateAssetMenu` paths and the water shader's name.
  Materials reference the shader by GUID, and no code looks it up by name, so the rename breaks nothing; checked by the compile, the tests and a capture.

## Done when

- `git grep` finds no `UnitySetupPrpj`, `artificial-pi` (as a whole word) or `unitysetup`.
- Compile with 0 warnings, EditMode and PlayMode green.
- The `dev` APK reports `com.pitrified.artificialpirate` and the label `Artificial Pirate` (`aapt2 dump badging`).

## What the implementation found

- 2026-11-17. `ProjectSettings.asset` set by a temporary `Phase02Rename` editor method in batch mode: product name, Android id, Windows Store package name and description. Deleted before the commit.
- One `UnitySetupPrpj` stays, on purpose: `projectName` in `ProjectSettings.asset` is the linked Unity Cloud project's name, read-only from the editor API and tied to `cloudProjectId`; it goes with [`00_start.md`](00_start.md) Q1.
- Gates after the renames: EditMode 114/114, PlayMode 4/4, no `error CS` or `warning CS`.
  Game scene captured before and after the shader rename: the water renders with its material, and the only difference is inside the animated water region below the horizon (mean 0.4 of 255), as in the source's control pair.
- The first `dev` APK build was stopped by Claude Code for low memory on the box (swap full; an idle Gradle daemon from the source's earlier build held 6 GB). It left `Assets/Resources/PerformanceTestRun*`, deleted. The APK check is still open.
- 2026-11-17, after stopping the idle Gradle daemon (user's go): `Tools/build-android.sh dev` under `xvfb-run -a`, exit 0, 70 MB. `aapt2 dump badging`: `com.pitrified.artificialpirate`, label `Artificial Pirate`, `arm64-v8a`.
