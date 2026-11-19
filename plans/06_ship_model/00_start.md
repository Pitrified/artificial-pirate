---
status: done
priority: 0
description: |
  A ship model in place of the box hull, built so several ship types come from one builder:
  hull, deck, masts and sails from a type's numbers, the Unity side reading a type's size in
  one place.
---

# A ship model

Asked 2026-11-19 by the user: "we now work on making a ship model. Quite general as there will be several of ship types."

## Where the ship stands

- `PF_Ship` is a root and a `Hull` child. The hull is Unity's cube at scale 1 x 0.5 x 2 with a `BoxCollider`; there is no model.
- The root carries `ShipController` (kinematic, tuned by `SO_ShipTuning`: 8 m/s, 60 °/s), `WaveRider`, `WakeSource` and a kinematic `Rigidbody`.
- The hull's size is written three times, each a hand-set number:
  - `WaveRider._halfLength` 1 and `_halfBeam` 0.5;
  - `WakeSource._halfLength` 1 and `_halfBeam` 0.5;
  - the hull's scale, which sets the collider.
  `WaveRider._waterlineOffset` 0 puts the box's centre on the water. `WakeSource`'s dent and foam radii suit a 2 m hull.
- The camera follows the root at `SO_CameraTuning._localOffset` (0, 3, -6), set for a 2 m hull.
- The scene's sizes: the island is flat out to 26 m with a beach to 40 m; the ripple square is 48 m and follows the ship; floating barrels and crates are about 1 m.
- The asset pipeline (`docs/building_assets.md`, the `build-asset` skill) makes props:
  - one builder module per object, `PARAMS` and `build(params)`, loaded by `build.py <name>`;
  - origin at the base centre, Z up, -Y toward the viewer;
  - material slots shared by name (`Wood`, `Iron`, `CrateWood`, `Sand`, `Grass`);
  - `BuiltModelTests` checks each imported model's size, pivot, transforms and slots.
  A builder can import a shared module (`builders` is a package), so a type can be a few lines over a shared builder.
- The spec has no ship types, type selection or other ships; combat is out of scope for v0.2 (§14).

## Questions

### Q1 - Scale

The box is 2 m long. A real sloop of the period is about 12 to 20 m on deck; a brig 25 to 30 m. The scene was sized around the box.

- **A (recommended): real proportions at real size**, a small sloop of about 12 m as the first type.
  The numbers come from the references at face value; the barrels and crates already are real size.
  Follow-on changes:
  - the camera offset grows with the ship, so it goes into each type's data (Q4);
  - the wake's radii and strength scale with the beam;
  - the island (52 m of land) reads small next to a 12 m ship. Rescaling it is a parameter change and a rebake of the depth map, but it is left to a later plan unless you want it here.
- What scale costs (asked by the user, 2026-11-19):
  - The GPU's cost depends on pixels and triangles, not metres.
    - The water is one fixed grid, 260 m square, 33.8 k triangles, whatever the ship's size.
    - The surface's fragment work is per pixel on screen. Seeing 300 m of sea costs about the same as seeing 50 m.
  - What does grow with a bigger ship:
    - **Waves.** The longest wave is 12 m (`SO_WaterSettings`). A 12 m ship lies across it nearly flat, so the waves get longer. That is a settings change, free at run time.
    - **The world.** About 20 lengths of a 12 m ship cross the 260 m grid. A sea to sail needs the grid to follow the camera, snapped to whole cells. That costs no more triangles.
    - **The wake.** The 48 m ripple square trails about two ship lengths behind. Keeping the trail as long means a square of about 100 m: at 256 texels that is 0.4 m texels and coarser ripples, at 512 texels four times the step's cost.
    - **What fills the view.** A wider view shows more islands and ships, each a few draws. This is the real limit, and it is counted per object in view, not per metre.
    - **Shimmer far off.** Waves shorter than a pixel alias. The detail fades with distance in the shader.
  - Too much is the budget: 150 k triangles and 80 draws in view. Measured now: 50 to 64 k triangles and 16 to 39 draws (`docs/systems/water.md`, Cost), at 60 fps on the phone.
- **B: game scale**, real proportions shrunk to about 4 to 6 m so the scene, camera and wake stay close to now. Every type is shrunk by the same factor, so the types still compare right. The props then look oversized next to the ship.

### Q2 - One builder for every type

- **A (recommended): a shared ship builder and a thin module per type.**
  - `builders/ship.py` makes a ship from a dict of numbers:
    - the hull, lofted from stations along its length: length, beam, depth, draft, sheer, bow and stern shape, deadrise;
    - the deck and a bulwark;
    - masts and their spars from a list;
    - sails from a list, each square or fore-and-aft;
    - a bowsprit and a rudder.
  - `builders/sloop.py` is `PARAMS` for a sloop over `ship.build`, so `build.py sloop` works with no change to `build.py`.
  - A new type is a new parameter set, plus a new part kind only when a type needs one (a galleon's stern castle).
- **B: one builder per type, written whole.** Each type would read best on its own, but the hull, masts and sails would be duplicated.

### Q3 - What the model has

- **Recommended:**
  - hull, deck, bulwark, masts, spars and bowsprit;
  - sails set and fixed in shape, with a slight belly;
  - a rudder;
  - no rigging lines: one-pixel lines shimmer on a phone, and they cost triangles for little.
  - no cannons or gun ports: combat is out of scope in v0.2. Ports can be added later as a hull part.
  - nothing moves: no furling and no sails swinging with the wind (that needs wind, which the game does not have).
- Budget: at most 3 k triangles and at most 5 material slots per ship. The scene allows 150 k triangles and 80 draw calls, and each slot is a draw call per ship. No LOD for now, since the camera stays close; LODs are the upgrade if many ships share the screen.
- What 3 k triangles is:
  - in this repo:
    - the crate, 72;
    - the barrel, 1978;
    - the island, about 7.7 k;
    - the water, 33.8 k.
  - common ranges (rough, not measured here):
    - blocky low-poly props, a few hundred to a couple of thousand;
    - mobile characters and hero vehicles, about 5 to 20 k;
    - detailed PC and console ships with planks, rigging and guns, 50 k and up.
  
  3 k is a clean low-poly ship: a lofted hull and solid sails, no plank geometry.

### Q4 - How Unity knows a ship's type

- **A (recommended): an `SO_ShipType` holds a type's numbers in one place**:
  - the hull's half length, half beam and waterline offset;
  - its movement tuning, which takes over `SO_ShipTuning`'s fields (they become per type);
  - the camera offset.
  How it is wired:
  - `ShipController` takes the type in place of `_tuning`;
  - `WaveRider` and `WakeSource` read the half length and half beam from it, so their own fields go;
  - the hull's `BoxCollider` is sized from it, in the prefab, by the set-up editor step;
  - `BuiltModelTests` checks each type's numbers against its model's bounds, so the builder and the asset cannot drift apart.
  - Each type is a prefab variant of `PF_Ship` (`PF_Ship_Sloop`) with its model and its `SO_ShipType`.
- **B: per-prefab numbers as now**, set on each variant's components. Simpler today, but three copies of the size per type.

`SO_ShipType` changes no save: the save keeps position and yaw. A saved ship's type is not needed while there is one player ship.

### Q5 - First types

- **Recommended: two, chosen to differ where the builder has to bend:**
  - a **sloop**, the player's ship: one mast, fore-and-aft mainsail and jib, low flush deck;
  - a **brig**: two masts with square sails, about twice the sloop's length, a raised quarterdeck.
- The brig is placed in the Game scene as an anchored ship: it rides the waves with no controller, so both types are seen side by side and tested in play. It has no AI and no sailing; other ships moving is other work.
- Later types, not in this plan: a schooner (two fore-and-aft masts) costs only numbers; a galleon needs the stern castle part.

### Q6 - Look

- **Recommended:** flat colours in shared slots, matched to a reference photograph per type as `docs/building_assets.md` §5 says:
  - `HullWood`, dark and tarred;
  - `DeckWood`;
  - `Spar`;
  - `SailCloth`, off-white;
  - `Trim`, a painted band.
  The slots are shared, so the types share colours. A type with its own paint gets a material override on its prefab variant when one is wanted.
- References: Commons photographs of replica ships, side-on, under licences the doc allows; real numbers from the replicas' published dimensions.
- Shape is matched in the probe. The palette gets one measured pass in a Unity capture per type.

### Q7 - Phases

Proposed, each closing on the gates and a commit:

1. **References**: photographs and real numbers for the sloop and the brig, and a "what right looks like" list for each.
2. **The ship builder and the sloop**: `ship.py`, `sloop.py`, and shape passes in the probe until the list reads.
3. **The sloop in the game**:
   - `SO_ShipType`, and the components reading it;
   - `PF_Ship` carrying the model, with its collider and camera offset;
   - the wake's radii set for its beam;
   - `BuiltModelTests` and a palette pass;
   - a sequence capture of it sailing and riding the waves.
4. **The brig**: its parameters and any new parts (the quarterdeck); `PF_Ship_Brig` anchored in the bay; captures.
5. **Verify**: dev APK, and on the phone (HUMAN): 60 fps held and the look.

## Decisions

Answered 2026-11-19 by the user: "all recommended, plan them". Earlier on Q2 and Q4: "keep it tidy code no hacks"; and in general few code comments, none that list current contents.

- **Q1: A**, real size. The sloop is about 12 m. The camera offset and the wake's size go per type. The waves get longer. The island's size and a grid that follows the camera are left to later plans.
- **Q2: A**, a shared `builders/ship.py` and a module of numbers per type.
- **Q3**:
  - hull, deck, bulwark, masts, spars, bowsprit and rudder;
  - fixed sails with a belly;
  - no rigging, no guns, nothing moving;
  - at most 3 k triangles and 5 slots per ship, no LOD.
- **Q4: A**, `SO_ShipType` holds the hull's size, the movement tuning (in place of `SO_ShipTuning`) and the camera offset; a prefab variant per type; a test ties the numbers to the model.
- **Q5**, a sloop for the player and a brig anchored in the bay.
- **Q6**, flat colours in five shared slots, matched to a replica's photograph per type.
- **Q7**, the five phases as proposed.
