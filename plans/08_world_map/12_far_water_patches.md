---
status: done
---

# Phase 12 - Patches on the far water

Asked 2026-11-20 by the user, on the phone: the far water is better but "a bit linear, there are just some long waves". They took the proposal: "ok try this 80m thing".

## Plan

- Cause: far out only the two long waves are left (the short ones fade so they do not shimmer), all straight-crested, and the 8 m detail normals blur to flat.
- One more sample of the detail normal map at a tenth of its tiling (about 80 m a repeat), scrolling slowly its own way, faded in with distance from the camera, so the far water gets broad uneven patches across the swell. Near the ship nothing changes.
- Check: the same two captures toward the horizon as phase 10; the gates; the phone.

## Done when

- The far water reads less linear in the captures and on the phone, with no visible tiling; the gates pass; a new dev APK.

## What the implementation found
- 2026-11-20. Done as planned: one more detail-map sample at a tenth of the tiling, drifting at 0.3 of the detail's speed, fading in from 60 to 200 m from the camera.
- Its strength was set from captures through the game's own camera: at 2 times the detail's strength the far water barely changed; at 4 the long lines past the start island break into uneven patches. 4 is in.
- Gates: compile 0 warnings, EditMode 195/195, PlayMode 22/22. A new dev APK: 74,173,379 bytes.
