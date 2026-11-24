---
status: planned
---

# Phase 04 - The frigate

## Overview

The middle type (Q4 a), and with it the builder's parts for bigger ships, which the giant reuses.
Context: [`00_start.md`](00_start.md), Decisions; depends on [`02_wind.md`](02_wind.md) and [`03_chase_guns.md`](03_chase_guns.md).

## Goals

1. Builder parts, each a parameter and named for what it is: a raised quarterdeck and forecastle (their lengths and heights, with ladders and rails), a third mast ship-rigged with a spanker, chase and stern ports.
2. A frigate from a reference: about 45 m, three masts, one gun deck of ports with guns on the quarterdeck and forecastle as the reference shows.
3. In the game: its prefab variant, `SO_ShipType` (handling, strength, ball damage, guns and chasers, camera rigs, map icon), in the ship choice (development builds).

## Plan

- **Reference**: a frigate with published particulars and a sail plan or profile drawing we may keep (licence in `CREDITS.md`), measured for the main proportions only.
- **Builder**: the raised decks change the deck height along the hull (`deck_height`), the bulwark and rail above them, the helm's place and `helm_eye`; the guns on them sit at their deck. Sloop and brig rebuilt unchanged (their triangles the same).
- **Chase and stern ports** (moved here from phase 03): bow and stern ports as parameters (count and height over the deck), the barrels `<Name>GunBow<i>` and `<Name>GunStern<i>` turning about their trunnions, the report printing their muzzles for `SO_ShipType`'s bow and stern guns; the variant's `Broadside` barrels in battery order (starboard, port, bow, stern).
- **Unity**: the FBX, the variant from `PF_Ship`, the type asset, the map icon from `textures.py map_icons`, the ship choice extended to a third kind (`PlayerShipKind`), a player's frigate in the Game scene inactive like the brig; all by a throwaway editor script.
- **Tuning**: speed and turning between the brig and a giant; strength between the brig's and the giant's (a figure to try, such as the brig needing about 40 hits); its bar's height and the camera rigs measured from the model.
- **Tests**: `BuiltModelTests` covers it as it covers the others (masts, bands, barrels at muzzles, map icon); `PlayerShipTests` sails it.
- **Capture**: its three views, and from the chase beside the brig.

## Out of scope

- A frigate at sea as an enemy (the giant is the new enemy; Q6).

## Done when

- Tests pass; gates green; triangles and draw calls within the budget with the frigate and two brigs in view; the captures read.

## What the implementation found
