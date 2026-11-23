---
status: planned
---

# Phase 04 - App icon

## Overview

The app's icon, a brig at sea: candidates made, the user picks, then it is set for Android.
Context: [`00_start.md`](00_start.md), Q7.

## Goals

1. Candidates: three or four stills from the game, and a flat SVG drawn by hand.
2. The user's pick set as the Android icon.

## Plan

- **Stills**: `SceneCapture` from the Game scene, the sailing brig, from a few angles (its bow quarter, abeam, low over the water), square, 1024 px; the brig inside the middle 66 % (the adaptive icon's safe circle).
- **Flat drawing**: a brig's silhouette with its sails over a sea and a sky, a few flat colours from the palette, drawn with Pillow's polygons by a function in `textures.py` (the script is the source; an SVG renderer such as `cairosvg` needs the cairo library, which this box may lack), 1024 px.
- **Shown to the user** as a sheet, with each at the phone's icon size (about 48 px) beside its full size; the user picks.
- **Set**, by a throwaway editor script: `Assets/Art/Icon/T_AppIcon.png` (and its source), as the default icon and Android's legacy and round icons, and as the adaptive icon's background with a clear foreground (or the brig as the foreground over a sea background, when the pick allows).
- A dev APK.

## Out of scope

- An icon for other platforms; a splash screen.

## Done when

- The user's pick on the phone's home screen, from the APK.

## What the implementation found
