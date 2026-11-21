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
- 2026-11-21. **Phone, this build** (`6646353`, Pixel 7 Pro, Mali-G710, Vulkan), 96.7 s sailing with both ships, last 60 s: frame ms p50 16.8, p95 16.8 (60 fps held); gpu ms p50 11.8, p95 12.3, max 16.3; cpu ms p50 16.8, p95 18.8 (the CPU time includes the wait for the next vsync, so it sits at the frame time; the GPU time is the one to compare). Session: 6 frames over 33 ms of 5 592, the first ones at load.
- The user saw a thin stripe of water at the stern. The cause was older than this plan: the hull's stern loop left the transom open from the deck to the rail between the bulwarks, and the deck stopped square short of the raked transom (0.73 m on the sloop, 0.30 m on the brig). The old solid taffrail board hid most of it from the chase camera; the open rail does not. Fixed in `0d14399`: a transom board (face aft, top, planking inside), the deck run aft to it, the stern windows centred on the board and laid on the rake. Sloop 9 554 triangles, brig 16 450; gates green (EditMode 233, PlayMode 24); captures from astern of both and from the sloop's deck.
- **Dev APK** from `0d14399`: 75 208 660 bytes, SHA256 `c24a78b67bdd9fef1ded96cec160de9b7d4691deaa3dc1ccdc209e3d6df24cfb`.
- 2026-11-22. **Against the baseline** (phase 01, `16e08ea`): gpu ms p50 11.8 and p95 12.3 in both; mean 11.4 before, 11.6 to 11.7 after, within what two different routes give. 60 fps held in both. The new ships cost nothing the phone can show under the cap; the frame keeps about 4.5 ms of GPU room.
- Waiting: the stern on the phone and the user's read of the six parts.
