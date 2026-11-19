---
status: planned
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
