---
status: planned
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
