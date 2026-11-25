---
status: draft
priority: 0
description: |
  Finer ship details the user asked for after the fleet: better ship's boats and wooden davits
  holding them, and rigging ropes that look like rope rather than black cylinders.
---

# Ship detail

## The idea

The user's notes, from the phone run of the fleet's giant ([`14_fleet`](../14_fleet/00_start.md)):
"better safety boats / wooden poles holding them up" and
"better ropes for the rigging, not just black cylinders".

## Where things stand

- Boats: `builders/ship/fittings.py` hangs a pair outboard on davits (`boats`: each [t, length]), a simple lofted hull.
- Ropes: `builders/ship/rigging.py` draws every rope as a thin cylinder in the `Tar` material (`line`).

## Leads, not yet checked

- Ropes: a twisted-lay normal map, tarred standing rigging dark and running rigging a lighter hemp, blocks and deadeyes where they end.
- Boats: thwarts, a sheer, oars; davits as curved wooden arms with their falls.
- Cost: judge it by the phone's frame time (60 fps held near both giants at GPU p50 12.8 ms), not by a triangle count.
  Thinner geometry may bring back the shimmer that MSAA 4x settled.
