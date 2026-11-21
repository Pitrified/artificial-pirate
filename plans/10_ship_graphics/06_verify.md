---
status: in progress
---

# Phase 06 - Verify

## Plan

- A fresh clone of the branch passes the gates; the dev APK goes to the phone.
- Before and after captures from the five poses, on one sheet.
- HUMAN on the phone: sail around both ships from the baseline's pose for a minute, paste the diagnostics report; say how the six parts look.

## Done when

- The phone's GPU and CPU times written here beside the baseline from phase 01; 60 fps held.
- The user's read written here; what they want changed becomes a phase or a later plan.

## Results

- 2026-11-21. **Fresh clone** of `feat/10_ship_graphics` at `6646353`, in `~/repos/artificial-pirate-verify` (deleted after): 22 LFS files, the models and textures identical to the working tree's; `Tools/setup.sh` exit 0; gates from an empty `Library/`: compile 0 errors, 0 warnings; EditMode 233/233; PlayMode 24/24; the clone's tree unchanged.
- **Dev APK** from `6646353`: 75 208 424 bytes, SHA256 `5fcfed52d9e87df4e03ce8bd917df4b5be0c0eee885ee846d2a34804bfd39847`. The baseline APK from phase 01 (`16e08ea`) is kept beside it as `Build/dev/artificial-pirate-baseline.apk`, so both runs can be taken on the same day.
- **Captures**: before and after from the brig's stern and deck and the sloop's aft deck, on one sheet (sent in chat). Counts with both ships in view: 63 278 triangles, 48 material slots, 15 renderers; the sloop 6 132 to 9 548 triangles, the brig 10 788 to 16 444.
- Waiting: the phone runs (baseline and this build) and the user's read.
