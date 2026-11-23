---
status: in progress
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
