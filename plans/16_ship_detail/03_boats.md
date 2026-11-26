---
status: done
---

# Phase 3 - Boats and davits

## Overview

The giant's four boats as open boats, hung from wooden davits.
Last of the building phases: it uses the hemp ropes, sag and blocks of phases 1 and 2.
Context: [`00_start.md`](00_start.md), depends on [`02_sag_and_ends.md`](02_sag_and_ends.md).

## Goals

1. `fittings._boat`: an open boat with a shell (white outside, `ShipWood` inside), a gunwale, a sheer rising toward the ends, a transom stern, three or four thwarts and oars laid along them.
2. Davits in `Spar`: standing on the hull's side below the rail, curving out over the boat in a few segments, a block at each head, two hemp falls down to the boat's ends.
3. The giant only; the `boats` parameter keeps its shape.

## Plan

- The davit's upright stands outside the cap rail at the widest the side reaches between its foot and the rail (the giant's tumblehome), then bends out over the boat in a quarter circle of a few segments.
- The falls hang straight down from a block under each davit's head; with no span across the water they do not sag.
- Boat proportions from a cutter's (a beam about a quarter of the length, the sheer rising a few tenths of a metre at the ends), by eye on the probe, not measured.
- Probe renders from above and from the side; one helm and one chase capture of the giant.

## Out of scope

- Boats on other types.

## Done when

- Gates green; the giant's triangles reported; the captures shown to the user.

## What the implementation found
The probe frames the whole model, so zoomed in it showed only sky; the boats were judged on Unity captures from a fixed eye beside the giant instead.
From above: the open boat, white outside and wood inside, three thwarts, two oars, the curved davits with blocks and falls. From the helm the davits read as wooden arms.
The giant: 49.7k triangles shown (+1.3k), clearance 0.
One side capture came out in strange colours (red masts, dark sails, pale decks), the same models normal in the capture before and after it: the scene's light at that moment, not the models; not followed up.
The gates failed once on `FightTests`: it took the last anchored ship the search returned, and with the giant at anchor a reimport changed the order, so the sloop fought the giant and sank.
The test now takes the anchored ship nearest the start, the brig (its own commit); gates green after it.
