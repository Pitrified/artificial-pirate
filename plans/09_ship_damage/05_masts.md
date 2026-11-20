---
status: done
---

# Phase 05 - Masts that break

## Overview

Masts become their own parts, so a hit can bring one down: it falls over the side and sinks, a stump stays.
Context: [`00_start.md`](00_start.md) Q3 and Q4, depends on [`01_hull_damage.md`](01_hull_damage.md).

## Goals

1. The builder makes each mast's parts separate from the hull, with a stump.
2. A hit may break the mast nearest it, more likely as the hull's health drops; the ship shows the stump.
3. The broken masts are saved.

## Plan

- Builder (`builders/ship`):
  - `body` keeps the hull, deck, fittings, crew and bowsprit;
  - each mast `i` gets `<name>Mast<i>` (mast, spars, tops, its standing rigging) and its sails in each state, `<name>Mast<i>Sailing` and `<name>Mast<i>Anchored`, with the running rigging that belongs to it;
  - `<name>Mast<i>Stump`, the mast cut a little above the deck, ragged top;
  - a jib or stay that runs to another mast goes with the mast it hangs from.
  The sloop and brig rebuilt; triangle counts noted (about the same, split across objects).
- Unity: `ModelSetup` keeps mapping slots; the prefab variants rebuilt by a throwaway editor script, each mast's objects under a `Mast<i>` node, the stump off.
- `ShipDamage` gets the masts (their place along the hull, from the prefab's `Mast<i>` nodes). Each hit rolls to break the standing mast nearest it, with a chance from a base at full health rising to a top at 0 health, both in the ship type; the roll uses a `System.Random` the tests can seed.
  Breaking hides the mast's objects, shows the stump, and drops a copy that tips over the side, settles in the water and sinks under `Buoyancy`, then is removed.
- Each mast lost takes an equal share off the top speed, which never goes below the floor from phase 01.
- The save gains the broken masts.
- `BuiltModelTests` updated for the new objects; EditMode tests for which mast a hit breaks, the chance rising as health drops (seeded), and the speed floor; PlayMode, a broken mast restored on Continue.
- Cost: each mast adds its objects' draws (one per slot). Measured in a capture before and after.

## Out of scope

- Cannons, torn sails, fire.

## Done when

- The tests pass; the gates pass; captures of each ship with a mast down; the draws in view within the budget (80); a dev APK.

## What the implementation found

- The model's objects cannot be moved under `Mast<i>` nodes inside a prefab instance, and need not be: `ShipMasts` holds each mast's objects (`MastParts`), placed and measured by a throwaway script from the stump's bounds.
- The fallen mast is a scripted tip and sink (`FallingMast`), not `Buoyancy`: a 30 m spar floating on four probes would be heavier and no better to look at. It falls away from the blow and the ship sails on from it.
- A roll on every hit broke masts on nudges; the chance is scaled by the hit's size up to `MastBreakingHit`.
- The flag goes with the tallest mast (the sloop's only one, the brig's main).
- `RemoveUnusedOverrides`, called to clear the overrides left on the old sail objects, also dropped the new active-state overrides made in the same pass; the states were set again in a second pass and checked.
- Draws: 48 to 57 material slots with both ships in view (`SceneCapture -captureBudget` from above), as counted ahead (+2 the sloop, +7 the brig); islands are not in that count, so the phone settles it. Triangles about the same (the stumps add 24 each).
- Capture: both ships losing mast 0, the sloop's falling to port and sinking, the brig keeping its mainmast and flag.
