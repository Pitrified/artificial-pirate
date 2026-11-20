---
status: done
---

# Phase 02 - Sinking and game over

## Overview

At 0 health a ship sinks. For the player's ship that ends the game; the brig is just gone.
Context: [`00_start.md`](00_start.md), depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. A ship at 0 health stops answering the helm and goes down over a few seconds, by the end that took the most water.
2. The player's ship sinking shows a game over screen with **Return to menu**; the save is deleted, so the menu offers no Continue.
3. A sunk brig is hidden and its hull no longer collides.

## Plan

- `ShipDamage` raises `Sunk` when it reaches 0, and drives the sinking: `WaveRider` hands the hull to a sinking pose (down by a set depth, pitched to one end) over a set time, then the ship is turned off.
- The player's controller ignores input once sinking.
- `GameState.GameOver`, reached from `Playing` when the player's ship has sunk. `GameManager` deletes the save and asks the UI for the game over screen; Return to menu goes through the existing `ReturnToMenuAsync`.
  No save on quit or pause from `GameOver`.
- The game over screen: a UI Toolkit document like Pause (`GameOver.uxml`, `ScreenId.GameOver`), a line ("Your ship has sunk") and the button. The HUD is hidden by the existing rule.
- Tests:
  - EditMode: the state machine allows `Playing -> GameOver -> Loading`, and nothing else from `GameOver`;
  - PlayMode: health to 0 sinks the ship, the screen shows, the save is gone, Return to menu reaches the menu.
- Docs: `game-manager.md` (the state), `ui-system.md` (the screen). `functional-specs.md` is left alone ([`00_start.md`](00_start.md) Q1).

## Out of scope

- A harbour to sail back to; repair.

## Done when

- The tests pass; the gates pass; a capture of the sloop going down; a dev APK.

## What the implementation found
- 2026-11-20. Review before the code: the game never stops time when paused (input is turned off), so a ship already going under keeps sinking behind the pause or the map; `Paused -> GameOver` is allowed for that.
- `ShipDamage` drives the sinking itself: `SinkDepth` grows with the square of the progress (slow at first) to 30 m over 8 s, `SinkPitchDeg` tips it toward the end last hit; then `Sunk` and the ship's object off. Phase 01's settling is renamed `Settle`, to keep it apart from sinking.
- A real bug on the way: quitting from the menu ran `PersistCurrentState`, which wrote a default save when there was none, so Continue would come back after a game over (and after a first launch). A save now needs a registered ship.
- The game over document is a GameObject beside Pause in the Persistent scene, added by a throwaway editor script (deleted); its layout reuses `Pause.uss`.
- `SequenceCaptureTests` asserts the game is still playing at the end, so a capture with `CAPTURE_DAMAGE=1` writes its frames and then fails, as expected: the sloop goes down bow first and is gone after about 7 s.
- The EditMode fake UI now records every hidden screen rather than the last one, since returning to the menu hides three.
- Gates: compile 0 errors 0 warnings, EditMode 210/210, PlayMode 23/23. Dev APK 74,173,403 bytes.
