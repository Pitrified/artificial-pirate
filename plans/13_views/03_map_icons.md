---
status: done
---

# Phase 03 - Map icons

## Overview

Boats on the maps: each kind of ship its own small icon, the player's and every other ship's, tinted by side.
Context: [`00_start.md`](00_start.md), Decisions.

## Goals

1. An icon per ship type from the builder's numbers, set on the type.
2. The minimap shows every ship in its window; the full map every ship in its picture.

## Plan

- **Icons**: `textures.py map_icons` writes `T_MapIcon_<Type>` for each ship builder: the hull's plan from above (the half-beam at the stations, the bow's point, the transom), filled white on clear, bow up, 32 px tall for the brig and the sloop to the same scale (about 19 px). No masts or details. `SO_ShipType.MapIcon` (a texture), set by a throwaway editor script; `BuiltModelTests` or a small test checks each type has one.
- **Minimap**: the arrow becomes the player's type icon; a pool of icons (8) for the other ships within the window, found as the bars find them (the ships' layer around the player's ship, the window's half diagonal), placed by their offset from the player's ship over the window's metres, turned with their heading, tinted by side; hidden beyond the window, and when sinking.
- **Full map**: `IUISystem.ShowMap` takes a `MapMarker[]` (where on the picture, heading, icon, tint), the player's first; `GameManager.OpenMap` finds the ships over the picture's area once as it opens. `MapPanel` places a marker per ship; `MapArrow` is removed.
- The tints are `SO_MapStyle`'s (player, enemy, ally).
- **Tests**: the icon's shape (the bow's row narrow, the widest row near the middle, longer for the brig); the minimap's markers for ships in and out of the window, tinted by side; the full map's markers placed from the world.
- **Capture**: the HUD with the minimap near both brigs; the full map.

## Out of scope

- Ships beyond the explored area on the full map are shown too (the map does not hide them); revisiting that is for the user.

## Done when

- Tests pass; gates green; the captures read at the minimap's size.

## What the implementation found

- **The icons are the hull's deck outline only**, 64 px squares (`textures.py map_icons`, needing Blender's modules loaded for the ship builders' `mathutils`), bow up. The sloop's and the brig's outlines are near alike in proportion; their length on the maps tells them apart (`SO_MapStyle`: 0.5 px a metre on the minimap, the brig 17 px and the sloop 10; 1 px a metre on the full map; at least 8).
- **The full map takes markers**: `IUISystem.ShowMap(map, MapMarker[], ...)`, the player's first; `GameManager.OpenMap` finds the ships over the picture once (the map pauses the game, so they stay put while it shows). `MapArrow` is gone.
- The minimap's icons are copies of the player's icon image, made once (a pool of 8).
- The full map's screen is a UI Toolkit overlay, which the sequence capture does not draw; its markers are checked by a test.
- Capture: the minimap with the white sloop between the two red brigs, each turned to its heading.
- Gates at the phase's commit: spec pass, compile 0/0, EditMode 263/263, PlayMode 30/30.
