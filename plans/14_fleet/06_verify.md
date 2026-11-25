---
status: in progress
---

# Phase 06 - Verify

## Overview

The docs and the phone.
Context: [`00_start.md`](00_start.md).

## Goals

1. The spec: Scope (the carriages, the wind as seen, chase and stern guns, the frigate, the giant; four types), Next (the sailed wind, islands for ships the size of the giant; sails that follow the heading, more types, chase guns and the giant leave it), the game's paragraph.
2. The pages: `combat.md` (batteries, chasers, the giant at anchor), `ship-controller.md` (the bands, the wind, the types), `world.md` (the wind; the giant's anchorage), `building_assets.md` (the new parts, the bands), `game-manager.md` and `ui-system.md` where the ship choice changed.
3. The phone: the sails' jumps while turning, the carriages from the helm, the frigate and the giant sailed and fought, the diagnostics report near the giant against the views run.

## Plan

- Write the docs; spec check, links and citations clean.
- A dev APK; the user's run and read.

## Done when

- Docs in the branch; gates green; the user's read and report.

## What the implementation found
The user's read of the first build: sails choppy (seven bands), cannons good, the frigate and the giant "not cool enough", the giant "a flat cube"; performance held 60 fps with GPU p50 12.8 ms near the giant.
The look passes that followed, against the Vespucci's profile drawing and photo and the Hermione's photo:

- Hull: tumblehome, sheer the bands, wales and covered ports follow, overhanging bow and counter (a warp of every object as it is made), a counter stern from a narrower waterline plan, more slices at the ends. The game's waterline curve, guns and helm are resampled from the builder after each reshape; a type's tumblehome reaches `SO_ShipType.SideAt` for the hull marks.
- Rig: six yards on the giant's fore and main, staysails on the stays between masts (giant, frigate, brig), a fourth jib, wider yards on the brig and the frigate, bobstay and martingale under every bowsprit, the flag on a staff above the tallest masthead, topmast shrouds led aft. Clearance stays at 0 on every type, band and the anchored state.
- Sails: eleven bands at 22.5 degrees with 5 degrees of hysteresis, a finer grid with folds from the head, reef bands and points, each sail its own wear, a faint glow through the canvas.
- Details: normal-mapped planks and canvas, the giant's red bottom and gilt line, scrolls ending the bands, a shaped figurehead under the bowsprit, anchors at catheads, deckhouses, boats on davits, more crew, the frigate's blue stern with two rows of windows and quarter galleries, the giant's stern gallery.
- Tried and dropped: a narrower waterline aft on the brig and the sloop (a box on a narrow bottom without a counter); a sail shader moving the cloth (two-sided sails would part along their normals; it needs its own UVs and shader).
- Triangles shown, one state: sloop 14.7k, brig 24.1k, frigate 30.4k, giant 40.1k.
