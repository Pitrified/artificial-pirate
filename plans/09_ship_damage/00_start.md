---
status: done
priority: 0
description: |
  Ships take damage as they sail: rams and groundings hurt the hull, which shows marks and holes,
  sits lower, lists and slows, and sinks at the end, which ends the game. Masts become their own
  parts so they can break, ready for the cannons that come next.
---

# Ship damage

Asked 2026-11-20 by the user: "ok we now work on live damage of the boat. Search online for options. How hard it is to break the models?"
Then, on the options: "Do a b c, new folder, some plans", with the answers:
"1 all those cause damage. Cannons will arrive soon / 2 sink and game over, when we have an harbour we'll go there / 3 not yet / 4 yes / 5 yes".

## Where the ship stands

- **Impacts.**
  - A ram is resolved in `HullContact.Collide`: the closing speed along the contact normal is shared by mass, and the other ship takes its share as a drift.
  - Grounding is `HullContact.Aground`: `ShipController` undoes the step and sets the speed to 0, so the speed at impact is known just before it is zeroed.
  - The anchored brig has a `HullContact` but no `ShipController`; it can be rammed.
- **The model.**
  - The builder (`Tools/asset_builders/builders/ship`) makes each ship as one `body` mesh (hull, deck, fittings, crew, masts, spars, standing rigging), a flag, and two sail states (`Sailing`, `Anchored`), each holding every sail and the running rigging.
  - The hull is a closed loft of 21 stations, one loop each; its UVs are in metres along the length and up the height.
  - Materials are URP Lit per slot, `Assets/Art/M_<slot>.mat`, shared by every ship.
- **The ship's pose.** `WaveRider` sets the hull child's heave, pitch and roll from the water; `_waterlineOffset` lifts or lowers it.
- **State, save, HUD.**
  - `GameState` has Boot, Loading, Menu, Playing, Paused; no game over.
  - The save (`GameSaveData`, version 3) holds the ship's pose and the world.
  - The HUD is a uGUI canvas (joystick, pause, minimap), shown only while playing.
- **The spec.** `docs/functional-specs.md` §14 puts combat out of scope for v0.2; the spec was written for the original sample project and does not bind this game (Q1).
- `CannonballLauncher` is a water sample that lobs balls at the water; it hits nothing.

## The options looked at

Research (web, 2026-11-20): mobile games do not fracture meshes live; they show damage with textures or shaders and bake anything that breaks.
Sea of Thieves, the closest reference, keeps the hull whole and shows holes that let water in, and has separate damage for masts.

- **A. Health and handling.** Hull health; damage lowers the top speed, sits the ship lower and lists it; at 0 it sinks. Cheap, no art.
- **B. Marks and holes on the hull.** Drawn at each hit point. Small cost.
- **C. Masts that break.** The builder makes each mast (with its spars, sails and rigging) its own object, plus a stump; a broken mast falls and the stump stays. A draw or two more per mast.
- D. Runtime mesh dents: on a 21-station hull they come out as soft bumps, not broken wood. Rejected.
- E. Real fracture (Cell Fracture pieces or runtime Voronoi): colliders and physics per piece, fights the code-built hull, costly on a phone. Rejected.

The user chose A, B and C.

## Proposed approach

- **One damage component per ship**, `ShipDamage`, on every ship that has a `HullContact` (the player's sloop and the brig):
  - hull health from 1 down to 0, from hits with a world point and an amount, so cannons plug in later through the same call;
  - a ram hurts both ships, by the energy each takes: the closing speed squared times the mass share, above a threshold so a gentle nudge does nothing;
  - grounding hurts by the speed at impact squared, above a threshold;
  - each hit is also kept as a mark (point and size) for B, and may break the mast nearest it (C), with a chance that grows as health drops (Q3).
- **What damage does** (A), each scaled by the hull's damage, numbers in the ship type:
  - the top speed falls with the hull's damage and with each mast lost (a rough equal share per mast), never below a floor that keeps the ship playable (Q4);
  - the ship sits lower (`WaveRider`'s waterline offset), down to the deck edge;
  - it lists toward the side with the most damage, a few degrees.
- **Sinking.** At 0 the ship stops answering the helm, goes down by the stern or bow over a few seconds and is gone.
  The player's ship sinking is game over: a screen with a line and **Return to menu**; the save is deleted, so Continue is not offered. The brig sinking just removes it until the scene reloads.
- **The HUD** shows the hull's health as a bar by the minimap.
- **The save** gains the hull health, the marks and the broken masts (version 4).
- **Marks** (B): small textured quads laid on the hull at each hit, splintered wood and a dark hole for a big one, one material, a fixed pool per ship.
- **Masts** (C): the builder emits each mast as its own objects (standing, its sails in each state) and a stump; `ShipDamage` hides a broken mast's objects, shows the stump and drops a copy that tips over and sinks.

## Questions

- Q1: The spec says combat is out of scope for v0.2 and must not leak into v0.2 code. Damage from rams and grounding is not combat, but it is built for the cannons that are coming.
  - a. Update the spec: add damage and sinking to §6 and §8, take nothing off §14 until cannons are planned.
  - b. Also take combat off §14 now.
  - Recommended: a, because this effort adds no weapons; the cannons' plan moves combat in.
  - ANS: a and b, read together: `functional-specs.md` was written for the original sample project, not this game, so its §14 does not bind here. This effort is damage, not combat. The spec is touched only where code points at it (the save schema in §9).
- Q2: How marks are drawn.
  - a. Textured quads laid on the hull at each hit, URP Lit with alpha clip, a pool of about 12 per ship. No change to the hull's materials; on a curved hull a quad can stand off a little at its corners.
  - b. A hull shader of our own that paints marks from a list of hit points. It follows the hull exactly, but means rewriting the lighting, shadows and fog URP Lit gives for every hull slot.
  - c. URP decal projectors. Built in, but the decal pass costs on the phone's GPU and needs the renderer set up for it.
  - Recommended: a, because it is the cheapest that reads at the camera's distance, and the hull is nearly flat over a hit's size.
  - ANS: ok (a).
- Q3: What breaks a mast before cannons exist: a ram or a grounding rarely brings a mast down.
  - a. A hard enough hit near a mast (a ram head on at speed, a fast grounding) breaks it; the code also takes a hit at a point, which is what a cannonball will call.
  - b. Nothing yet: masts become their own parts and can break, but only tests break them until the cannons come.
  - Recommended: a, so you can see it on the phone; the threshold sits high enough that only full-speed impacts do it.
  - ANS: Neither as written: each hit has a chance to break the mast nearest it, and the chance grows as the hull's health drops.
- Q4: Does a broken mast slow the ship? Its sails are gone.
  - a. Yes, the top speed falls by that mast's share of the sail area.
  - b. No, only hull damage slows it, until wind and sail power exist.
  - Recommended: a, because a stump with full speed reads wrong, and the sail areas are in the builder's numbers.
  - ANS: a, with a floor on the top speed so the ship stays playable; a rough share per mast, no sail-area sums.
- Q5: Does the hull heal on its own? You said no repair yet, so damage stays until the ship sinks or a new game starts.
  - a. No healing in this effort.
  - b. A slow pump: health creeps back to a cap (for example 50%) while not hit.
  - Recommended: a, as you said; the harbour will bring repair.
  - ANS: a, no healing; repair crews come later.
- Q6: What the brig keeps across a Continue: it is a scene object, not saved.
  - a. Nothing; it comes back whole on each load.
  - b. Save its damage and whether it sank, by an id.
  - Recommended: a, because it is a placeholder until real ships arrive with cannons.
  - ANS: a.
