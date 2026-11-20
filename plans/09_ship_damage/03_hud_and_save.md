---
status: done
---

# Phase 03 - HUD bar and save

## Overview

The player sees how damaged the hull is, and the damage survives a Continue.
Context: [`00_start.md`](00_start.md), depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. A hull health bar on the HUD, by the minimap.
2. The save holds the hull's health and hits (and, after phase 05, the broken masts).

## Plan

- The bar: under the minimap on the HUD canvas, a dark uGUI `Image` holding a fill `Image` whose right anchor is the health (a filled `Image` needs a sprite; the stretched child does not), with a small `HullBar` script reading the player's `ShipDamage`, bound by `GameSceneWiring` after the ship's save is restored. It updates on `Damaged`, not each frame. Colour from green to red, two colours on `HullBar` (no style asset for two colours).
  Added to the scene by a throwaway editor script.
- `ShipDamage` keeps its hits in the hull's local space (point and amount, a fixed array, the oldest dropped when full) and the damage on each side, and can be restored from them.
  The health lost is saved on its own (the sides' sum can round to a hair under 1 for a ruined hull, which would come back afloat); the end that goes down first is the last hit's.
- The save: `GameSaveData` version 4 with a `damage` block (health, the hits in hull space); `GameManager` writes and restores it with the ship's pose. A version 3 save resets, as the save system does on a mismatch. The save on the phone is version 3, so the next APK starts a new world.
- Tests: EditMode, the bar's fill and colour from health; PlayMode, damage restored on Continue.
- Docs: `save-system.md` and `functional-specs.md` §9 (the schema), `ui-system.md` (the bar).

## Out of scope

- Marks drawn from the restored hits (phase 04 draws whatever hits there are, restored or new).

## Done when

- The tests pass; the gates pass; a HUD capture with a half-damaged hull.

## What the implementation found

- A filled uGUI `Image` draws nothing without a sprite, so the fill is a child stretched to the health.
- Green to red straight through RGB is a muddy brown at half; the bar goes green, yellow, red.
- The sides' sum rounds, so a ruined hull could come back with a hair of health and afloat; the save keeps the health lost on its own.
- The hits are in the riding hull's space (the `Hull` child, scale 1 on the prefab), which is what the marks will be parented to.
- Capture: the HUD at half damage, the bar yellow and half full under the minimap.
