---
status: done
---

# Phase 05 - Verify

## Plan

- A fresh clone of the branch passes the gates.
- The dev APK goes to the phone.
- HUMAN on the phone:
  - play for a few minutes: sail the sloop past the brig and around the island;
  - paste the diagnostics report here.
  - Check that 60 fps is held, and give your read of the ships' look, the camera, the waves at the new size and the wake.

## Done when

- The phone report: 60 fps held, no per-frame allocation, no problems.
- The user's read of the look written here; what they want changed becomes a phase or a later plan.

## What the implementation found
- 2026-11-19. **Fresh clone** of `feat/06_ship_model` at `660c93d`, in `~/repos/artificial-pirate-verify` (on disk, not `/tmp`):
  - LFS: 19 files, all real (the ship models identical to the working tree's).
  - `Tools/setup.sh` exit 0 (warns that `bundletool` is missing, needed only for AABs).
  - Gates from an empty `Library/`: compile 0 errors, 0 warnings; EditMode 146/146; PlayMode 19/19. The clone's tree unchanged.
  - `plans.py check` passes; 116 relative links in the markdown, none broken.
- **The dev APK** is the working tree's from `4c1626f` (74,104,019 bytes); `660c93d` changes plan notes only.
- **The phone**: three reports, all 60 fps held at 147.5 MB with 0.1 kB allocated a frame and no problems: the detail build ([`06_detail.md`](06_detail.md)), then the furls and the rig clearance ([`08_rig_clearance.md`](08_rig_clearance.md)).
- **The user's read of the look**: "Brillant results", "all lovely", "Works very well". Changes they asked for became phases 07 (furls) and 08 (rig clearance). Noted for later: the waves are large, a choppy sea, for references and scale.
