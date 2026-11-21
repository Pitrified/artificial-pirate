---
status: done
---

# Phase 04 - Hatches, bulwark and rail

## Overview

The hatches are solid boxes and the bulwark a plain band. The hatches get coamings and gratings, one becomes the way below, and the rail gets timberheads and a moulding.
Context: [`00_start.md`](00_start.md) "Hatches and the companionway", "Bulwark and rail", Q4; depends on [`02_wood.md`](02_wood.md).

## Goals

1. Every hatch a coaming with a grating of bars over a dark recess.
2. One companionway per ship: a hood with a sloping top and an open front, a ladder going down into the dark.
3. Timberheads standing proud of the cap rail; a moulded cap rail.

## Plan

- `fittings.furniture`: hatches as a coaming (four boards) around a `Tar` recess, the grating as crossed bars in `ShipWood`; the type's `deck` keys gain which hatch is the companionway (`companionway`, the hatch's index), the rest gratings.
- Companionway: two side boards, a sloping roof, a back, the front open onto a `Tar` opening, three or four ladder steps visible going down.
- `fittings.rail`: the stanchions run up through the cap rail as timberheads every few stations; the cap's profile gains a step on the outboard edge.
- Rebuild, captures from the deck poses, counts logged.

## Out of scope

- Gun ports cut through the bulwark.
- A deck below to walk into.

## Done when

- Gates green; deck captures show gratings, a companionway and timberheads.
- Counts logged.

## Pitfalls

- The crew and cannons are placed on the deck; check none stands in the companionway or on a grating's recess.

## What the implementation found

- The companionway is named by the hatch's index rather than its `t`: matching floats is fragile. The sloop's is its main hatch (t 0.45): a crew member stands at the corner of the fore hatch. The brig's is the after hatch (t 0.3).
- No crew stands on a grating or in the companionway's opening.
- The cap rail's moulding is a chamfer on both top edges; the grating's fore and aft bars sit a bar lower than the athwartship ones so their tops do not meet flush.
- The sloop's companionway was hard to frame in the scene under the boom; a Blender probe of the deck showed it.
- Counts: sloop 7 052 triangles, brig 12 824; both ships in view 57 470 triangles, 48 material slots.
