---
status: done
---

# Phase 01 - References

## Plan

- **Photographs**, from Wikimedia Commons by category, with the licence in the same API call (`docs/building_assets.md` §2):
  - for each type, a replica or a museum ship, side-on, whole, under CC0, public domain or CC BY;
  - one for the sloop, one for the brig;
  - thumbnails at 960 px, one at a time with a pause, a generic User-Agent;
  - kept in `Tools/asset_builders/references/sloop/` and `references/brig/`, each with a row in `CREDITS.md`.
- **Real numbers** for the same ships, from the replicas' published particulars:
  - length on deck, beam, draft and depth;
  - mast heights and positions;
  - sail areas or spar lengths where given.
  A photograph gives ratios; the particulars give metres. Where only ratios exist, a ratio is measured on the side-on photograph and noted as measured.
- **What right looks like**, for each type, a numbered list from its photograph:
  - length to beam, and freeboard to length;
  - the sheer and its lowest point;
  - bow rake and stern shape;
  - masts' count, positions and rake;
  - the sails' outline and how much they fill the rig;
  - the bulwark and the raised deck (the brig);
  - the palette's big areas.
- The lists and numbers are written here, under the findings.

## Done when

- Both photographs kept and credited.
- Both types' numbers written with their sources.
- Both lists written.
- Nothing in `ArtificialPirate/` changes. No gates are needed beyond `plans.py check`.

## What the implementation found
- 2026-11-19. **Downloads**: Wikimedia answers 429 to the generic User-Agent after one file and keeps refusing for more than 8 minutes, on thumbnails and originals alike. Originals redirect fine (`Special:FilePath`); thumbnails are generated on request and were refused first. The user chose a descriptive User-Agent, per Wikimedia's bot policy.
- **Sizes** (the user, 2026-11-19: "Providence + Niagara"). The only usable period sloop replica on Commons is the *Providence* (1976), at 20.3 m on deck, not the plan's 12 m. *Lady Washington* (brig, 1989) is also 20 m. The brig is the *Niagara* (snow-brig, rebuilt 1988), 33.7 m between perpendiculars. Real numbers at face value: the camera and the wake scale to a 20 m ship.
  - *Providence*: LOA 34 m, LOD 20.3 m, LWL 17.98 m, beam 6.16 m, draft 2.41 m ([classicsailboats.org](https://classicsailboats.org/?p=32729)).
  - *Niagara*: LBP 33.7 m, beam 9.8 m, depth 2.7 m, foremast 34.5 m, mainmast 36.1 m, sail area 1,177 m² ([Wikipedia](https://en.wikipedia.org/wiki/USS_Niagara_(1813))).
  - *Lady Washington*, kept for comparison: LOD 20 m, beam 7 m, draft 3 m, rig height 27 m ([Wikipedia](https://en.wikipedia.org/wiki/Lady_Washington)).
- **First photograph**: *Providence* starboard side at a pier, public domain (US Navy, via DPLA), whole hull side-on, sails furled. Hull white above a black bottom, a blue and a red band at the rail.
- 2026-11-19. **The descriptive User-Agent works.** The user gave the repo's URL and a contact address. With them, Commons served about 30 originals one at a time with a 6 s pause and no refusal. The originals were downscaled here.
- **Candidates.** Commons categories, filtered to allowed licences:
  - `Providence (ship, 1976)`: 4 files;
  - `Niagara (ship, 1990)`, the 1988 rebuild: 29 files;
  - `USS Niagara (ship, 1813)`: the 1913 reconstruction and paintings, not used;
  - `Lady Washington (ship, 1989)`: 30 files, kept as a fallback.
- **Kept**, in `Tools/asset_builders/references/` and credited in `CREDITS.md`:
  - `sloop/providence_side.jpg`, public domain. Starboard side at a pier, sails furled, nearly square to the camera. The reference for the hull, the mast and the spars.
  - `sloop/providence_sail.jpg`, CC BY 2.0. Under sail, bow three-quarters to the camera. It shows which sails the rig carries; it is too foreshortened to measure.
  - `brig/niagara_sail.jpg`, CC BY 2.0. Broadside under full sail. The reference for everything on the brig.
- **Scale check.** On `niagara_sail.jpg`, taking 33.7 m as the stern-to-stem length at the rail (468 px, 13.9 px/m), the mast tops measure 35.2 m and 36.6 m above the water, against the published 34.5 m and 36.1 m. So the photograph is close to side-on and its ratios hold.

### The sloop: numbers

Published (classicsailboats.org): LOD 20.3 m, LWL 17.98 m, beam 6.16 m, draft 2.41 m, LOA 34 m with the spars.
Measured on `providence_side.jpg`, with the 20.3 m on deck at 308 px (15.2 px/m). Distances along the hull are from the stern.

| What | Measured |
| ---- | -------- |
| Freeboard to the rail | 2.5 m amidships, 2.9 m at the stern, 3.1 m at the bow |
| Mast | at 0.69 of the length from the stern (6.3 m behind the stem), raked aft 5.5° |
| Mast height above the water | 28.6 m to the truck; the lower mast's head (crosstrees) at 17.2 m |
| Boom | 16.5 m from the mast, overhanging the stern by 2.6 m |
| Bowsprit | 9.7 m beyond the stem, steeved up about 29° |
| Stern | a flat transom, raked aft |

Rig, from `providence_sail.jpg`:
- a gaff mainsail on the boom;
- a square topsail, and a topgallant above it, on yards on the topmast;
- two headsails, a jib and a staysail, from the bowsprit.

### The sloop: what right looks like

1. A long, low hull: length on deck 3.3 times the beam, freeboard about 2.5 m, a gentle sheer lowest a little aft of amidships and rising about 0.5 m to each end.
2. A transom stern, raked aft; a bow with a rounded stem, not a clipper.
3. One mast well forward, at about 0.7 of the length from the stern, raked aft, as tall as 1.4 times the hull's length.
4. A boom longer than half the hull, overhanging the stern.
5. A long bowsprit steeved up, half the hull's length beyond the stem.
6. Sails: a big gaff main filling the space aft of the mast; two square sails above it on the topmast; two triangular headsails ahead to the bowsprit.
7. Palette (side photograph): white topsides, a red band at the rail with red gun ports, a blue rail cap, a black bottom showing above the water, spars in varnished and black wood.

### The brig: numbers

Published (Wikipedia):
- LBP 33.7 m, beam 9.8 m, depth 2.7 m;
- foremast 34.5 m, mainmast 36.1 m;
- sail area 1,177 m².

Measured on `niagara_sail.jpg`, at 13.9 px/m, from the stern:

| What | Measured |
| ---- | -------- |
| Freeboard to the rail | 3.0 m, nearly even along the hull |
| Masts | mainmast at 0.40 of the length, foremast at 0.75; both raked aft, about 6° and 4° |
| Yards, foremast, from the bottom | course 13.7 m, topsail 13.3 m, topgallant 8.5 m, royal 7 m |
| Yards, mainmast | topsail 13 m, topgallant 8.3 m, royal 7.2 m; no main course is set |
| Spanker boom | 14.9 m from the mainmast, overhanging the stern by 1.3 m |
| Bowsprit and jib-boom | 17 m beyond the stem, steeved about 16° |

### The brig: what right looks like

1. A long, nearly straight hull: length 3.4 times the beam, freeboard about 3 m, a flat sheer rising a little at the bow.
2. A transom stern, near upright; a bluff bow under a long jib-boom.
3. Two masts at 0.40 and 0.75 of the length from the stern, both about as tall as the hull is long, raked a little aft.
4. Square sails on each mast, stacked, narrowing upward: three on the mainmast, four on the foremast with the course.
5. A gaff spanker aft of the mainmast; three triangular headsails from the foremast to the jib-boom.
6. Palette: a black hull with a broad ochre band along the gun deck, black gun ports in it, a black rail; off-white sails, brown spars.
7. Against the sloop: about 1.7 times its length, much taller and broader in sail.

### Notes for the builder

- No raised quarterdeck shows on `niagara_sail.jpg`: the rail runs flush from stem to stern. Phase 04's quarterdeck part is not needed for this brig; the generality is in its two masts and stacked square sails.
- Both photographs show cannon ports as painted or dark rectangles in a band. With guns out of scope, they are part of the palette: a `Trim` band and dark squares, not geometry.
- The sloop's mast rises 28.6 m above a 20.3 m hull, and the brig's 36 m: both are taller than they are long, which is a matter for phase 03's camera offset.
