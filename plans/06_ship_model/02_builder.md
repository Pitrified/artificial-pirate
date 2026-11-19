---
status: done
---

# Phase 02 - The ship builder and the sloop

## Plan

- **`builders/ship.py`** builds a ship from a dict of numbers, as functions per part:
  - **Hull.** Lofted from stations along the length, each a cross-section given by the beam, depth and deadrise at that station. Parameters:
    - the beam's distribution: bow fullness, stern fullness, a transom or a pointed stern;
    - the sheer: rise toward the bow and the stern;
    - the keel's rocker;
    - the draft.
    One closed, smooth mesh. Its rim and transom edges are marked sharp.
  - **Deck and bulwark.** The deck follows the sheer inside the hull. The bulwark is a thin band above the sheer, flat shaded on top.
  - **Masts and spars** from a list, each with:
    - its position along the length and its rake;
    - its height, and its diameter at the deck and at the head;
    - its yards or boom and gaff.
    They are tapered cylinders with few sides.
  - **Sails** from a list, each square (hung from a yard) or fore-and-aft (gaff, boom, or a jib on a stay). Each sail is a quad grid with a belly along its depth and two sides, so it shows from behind.
  - **Bowsprit and rudder.**
  - **One mesh object** per ship, its parts joined, with slots `HullWood`, `DeckWood`, `Spar`, `SailCloth` and `Trim`.
- **Origin on the design waterline, amidships**, Z up, the bow toward +Y.
  - It is not the props' base centre: a hull is placed by its waterline. With this origin, `WaveRider`'s waterline offset is 0 for every type.
  - The bow is away from the viewer, in the shared convention's -Y-toward-the-viewer.
  - Recorded in `docs/building_assets.md` as the ships' exception.
- **`builders/sloop.py`**: `PARAMS` from phase 01's numbers, each with its unit and source, and `build = ship.build`. `build.py sloop` needs no change in `build.py`.
- **Shape passes in the probe** (`build.py sloop --no-fbx --probe rN.png --view AZ EL --reference ...`):
  - one class of change per pass, in a scratch folder;
  - side-on against the photograph until the sloop's list reads;
  - then a three-quarter view for the hull's sections.
- **Budget** from `build.py`'s print: at most 3 k triangles and 5 slots.
- **Code**: small functions per part, typed, no copied blocks; comments only where the reason is not in the code.

## Done when

- The sloop's list reads in the probe, or a line cannot and the findings say why.
- At most 3 k triangles and 5 slots.
- `build.py sloop` writes `ArtificialPirate/Assets/Art/Models/Sloop.fbx`, identical on a rebuild.
- `docs/building_assets.md` has the ship origin and the shared builder pattern.

## What the implementation found

- 2026-11-19. **`builders/ship.py`**:
  - **Hull.** 21 stations, each a loop: deck, bulwark, rail, trim band, topsides, boot top, then 5 points down a superellipse bilge to the keel, and the other side mirrored.
    - Toward the ends the underwater sections narrow into a V. The keel sweeps up at the forefoot.
    - The transom is a flat, raked cap; the stem is a narrow cap.
  - **Masts** are tapered 8-sided poles raked aft. Yards, boom, gaff and bowsprit are 6-sided.
  - **Sails** are 4 x 4 grids bellied in the middle, two-sided as separate copies, so they show from both sides with back-face culling.
    - Square sails hang between two yards.
    - A gaff sail fills the gaff and the boom.
    - A jib runs from the mast to a point on the bowsprit.
  - Slots: `HullBottom`, `Topsides`, `Trim`, `ShipWood`, `SailCloth`. The plan's `HullWood`/`DeckWood`/`Spar` became `ShipWood` for the deck and spars, and `Topsides`, because both ships paint the hull in two colours above a dark bottom.
- **`builders/sloop.py`**: round numbers from phase 01; `build` is `ship.build`.
- **Probe passes**, one round, as the user asked for less detail:
  - side-on against `providence_side.jpg`: the hull's length, low freeboard and sheer, the mast's place and height (1.4 times the hull), the long boom and the steeved bowsprit read as in the photograph;
  - three-quarter against `providence_sail.jpg`: the gaff main, two square sails above it, and two jibs.
  - From the side the square sails are edge-on, as in any side view of a square rig.
  - Probes are in `~/.cache/claude-verify/ship/`.
- **Budget**: 1,358 triangles, of which 910 are the hull and 320 the sails; 5 slots. Size 12 x 34 x 30 m, with the boom and bowsprit included.
- `build.py sloop` writes `Sloop.fbx`, byte-identical on a rebuild. `ruff` is clean.
- `docs/building_assets.md`: "Families of objects: ships" covers the shared builder, the waterline origin, the bow toward +Y and the five slots. Which way the bow faces in Unity is checked in phase 03.
- The FBX is committed in phase 03 with the `.meta` that `ModelSetup` writes, so its GUID is fixed from the first commit.
