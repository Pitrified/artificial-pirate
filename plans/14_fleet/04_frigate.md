---
status: done
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
- **Reference**: the Hermione (2014 replica of the 1779 French frigate): 65 m overall, 11.24 m beam, 5.78 m draft (Wikipedia); a stern-quarter photograph (CC BY 2.0) kept as `references/frigate/hermione_stern.jpg`. The model is 44.2 m on deck and 65 m with the bowsprit.
- **The main battery is covered**: `gun_ports` takes rows; a row with `z` (a covered gun deck) shows the barrels' muzzles through its ports and no carriages, a row with `above_deck` stands on carriages. The frigate's 13 a side are covered; two chasers fire through the bow and two guns through the transom (`chase_ports`, `stern_ports`), 30 barrels in all.
- **Raised decks** (`raised_decks`: a quarterdeck to 0.24 of the length, a forecastle from 0.86): the rail and the deck step up within 0.4% of the length, the hull lofted either side of each step (`hull.stations`) so it stands near upright.
- **The rig is the brig's scaled to each mast**: my first numbers put the jib stays through the fore topsails at every wind and the courses into the shrouds; the brig's proportions clear. **Close-hauled is 67.5 degrees** for every type (at 60 the frigate's courses still pressed into the lee shrouds); the trim asset and its default were updated, and a test now reads the builder's `WIND_BANDS_DEG` against `SO_SailTrim`, since the two had drifted.
- **The helm camera stands 1.5 m to starboard** on the frigate: on the centreline the mizzen filled the view 4 m ahead. The two hands by the helm moved aft of it.
- The prefab is the brig's variant copied with the frigate's model in place (types, collider, masts, sails, barrels in battery order, the brig's black and ochre paint); the player's frigate is in the Game scene, inactive, its scene references copied from the player's brig; `PlayerShipKind.Frigate` and a Frigate button in the ship row; `ShipKinds` counts the enum.
- Handling: 7.5 m/s, 15 degrees a second; strength 2.4 (the brig needs about 40 hits).
- Drawn at once: the frigate 23,036 triangles in 38 objects (the brig 20,120 in 24, the sloop 12,596 in 16). The frigate and two brigs are about 63 k triangles, within 150 k; the draw calls are the open question for the phone (each barrel is an object).
- Gates: spec pass, compile 0/0, EditMode 290/290, PlayMode 31/31. Captures: the frigate's chase, bird's eye and helm views.
