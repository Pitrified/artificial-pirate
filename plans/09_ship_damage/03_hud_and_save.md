---
status: planned
---

# Phase 03 - HUD bar and save

## Overview

The player sees how damaged the hull is, and the damage survives a Continue.
Context: [`00_start.md`](00_start.md), depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. A hull health bar on the HUD, by the minimap.
2. The save holds the hull's health and hits (and, after phase 05, the broken masts).

## Plan

- The bar: a uGUI `Image` (filled) on the HUD canvas with a small `HullBar` script reading the player's `ShipDamage`, wired by `GameSceneWiring`. It updates on `Damaged`, not each frame. Colour from green to red, from a style asset like the minimap's.
  Added to the scene by a throwaway editor script.
- `ShipDamage` keeps its hits in the hull's local space (point and amount, a fixed array, the oldest dropped when full) and the damage on each side, and can be restored from them.
- The save: `GameSaveData` version 4 with a `damage` block (health, the hits in hull space); `GameManager` writes and restores it with the ship's pose. A version 3 save resets, as the save system does on a mismatch.
- Tests: EditMode, the bar's fill and colour from health; PlayMode, damage restored on Continue.
- Docs: `save-system.md` and `functional-specs.md` §9 (the schema), `ui-system.md` (the bar).

## Out of scope

- Marks drawn from the restored hits (phase 04 draws whatever hits there are, restored or new).

## Done when

- The tests pass; the gates pass; a HUD capture with a half-damaged hull.
