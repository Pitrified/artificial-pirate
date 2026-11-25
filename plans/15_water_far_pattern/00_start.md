---
status: draft
priority: 0
description: |
  Far water seen from the chase camera shows a pattern: the small ripples line up into faint bands
  of lines in the distance. Find what repeats and break it up.
---

# Water far pattern

## The idea

The user's note, from the phone run of the fleet's giant ([`14_fleet`](../14_fleet/00_start.md)):
"in the chase camera (maybe now that it's more distant from the giant boat) we notice patterns in the waves.
the small ripples line up when looking far, and you see a faint bunch of lines or something".

## Leads, not yet checked

- The detail normal map repeats; past 60 m it is sampled again at a tenth of its tiling (about 80 m a repeat, docs/systems/water.md).
  At a grazing angle either repeat can line up into rows.
- The four Gerstner waves sum to a periodic pattern; their crests seen far off can read as parallel lines.
- Filtering of the detail map at a grazing angle (mips, anisotropy) can make a moire.
- First step: reproduce it in a capture from the chase camera's distance behind the giant, then switch each term off in turn.
