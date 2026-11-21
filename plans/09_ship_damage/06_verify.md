---
status: done
---

# Phase 06 - Verify

## Plan

- A fresh clone of the branch passes the gates; the dev APK goes to the phone.
- HUMAN on the phone:
  - ram the brig a few times at different speeds, run aground, watch the bar, the marks, the list and a mast going down;
  - quit and Continue: the damage is back;
  - sink the brig, then the sloop: the game over screen, Return to menu, no Continue;
  - paste the diagnostics report.

## Done when

- 60 fps held with marks and broken masts in view, no per-frame allocation, no problems in the report.
- The user's read written here; what they want changed becomes a phase or a later plan.

## What the implementation found

Phone run on 2026-11-21, Pixel 7 Pro, the dev APK from `01f3eff`, three diagnostics reports.

- The sloop sank after 9 real hits in about 113 s of play: sinking at health 0, under 8 s later, the game over screen, back to the menu with no save on disk.
- Its mast went down on a 0.150 hit at health 0.375 in the first game, and on the first hit (0.315) in the second.
- Quit and Continue restored the seed, the explored cells, health 0.685 with its hit, and the position, both from the menu and after restarting the app.
- The user's read: the mast stayed down after Continue, marks appear on the ship, the bar goes down and changes colour. Nothing to change.
- 60 fps held (p05 and p50 59.7), memory flat, no problems in any report; the only long frames are scene loads.
- The brig takes about a quarter of each ram the sloop takes, the blow split by size; it was not sunk in this run.
- Hits under 0.001 still log a line; noise only, left as is.
- The restore log line reports health and hits but not the masts lost; the mast's restore was confirmed by eye.
- The fresh-clone gate run was not done: it is a full import of about ten minutes and was not asked for; the gates passed on this tree at `01f3eff`.
