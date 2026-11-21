---
status: in progress
priority: 0
description: |
  Better-looking ship fittings: the lantern, the helm, the hatches, the bulwark and its rail, the masts
  and the taffrail. More detail in the shapes from the builder, and wood textures where the parts are
  now flat colour, within the draw call budget.
---

# Ship graphics

Asked 2026-11-21 by the user, after merging `09_ship_damage`:
"plan better graphics: Lamp, Helm, The port to go down into the ship, The parapect, The mast, The back parapet.
Either more details in the shapes or some textures or both. Plan, take some screenshots, propose improvements."

Read here as: the stern lantern, the helm (the brig's wheel, the sloop's tiller), the hatches (a companionway to go below), the bulwark and its cap rail, the masts, and the taffrail across the stern.

## Where the ship stands

Captured 2026-11-21 in `Game.unity` with `SceneCapture` at 1280x720, close views of the sloop and the brig (sent in chat, not kept in the repo).

- **The parts are flat colour.** The mast, spars, the bulwark's inner face, the cap rail, stanchions, hatches, capstan and helm are all the `ShipWood` slot, `M_ShipWood`: URP Lit, a plain brown, no texture.
  The builder gives poles and boxes UV (0, 0) everywhere; only the hull loft (the bulwark's inner face included) has UVs, in metres.
- **Lantern** (`props.lantern`): an iron post on the taffrail, a 0.35 x 0.35 x 0.5 m box of the palette's `lamp` yellow, a six-sided brass cone on top. It reads as a yellow block; no frame, no glass, no glow.
- **Helm** (`fittings.furniture`): the brig's wheel is a solid 12-sided disc 1.4 m across on one square post, so it reads as a millstone. The sloop's tiller is a six-sided pole from the stern.
- **Hatches** (`fittings.furniture`): solid `ShipWood` boxes, 0.35 m high, no coaming, no grating, no way down.
- **Bulwark and rail** (`hull` and `fittings.rail`): the bulwark's inner face is a smooth brown band; the cap rail is a lofted strip; square stanchions every 1.2 m.
  From the deck view the bulwark is the biggest flat area in frame.
- **Masts** (`rig.standing`): an 8-sided tapered pole, flat shaded, so its facets show; tops and crosstrees are boxes; no bands, no partners at the deck.
- **Taffrail** (`fittings.stern`): one `Gilt` box across the stern, 1.8 half-beams wide and 0.45 m high, flat yellow. The stern windows below it are framed and read well.
- **Cost now.** Sloop 6 132 triangles (hull object 3 298, mast 484), brig 10 788. Both ships in view: 57 draw calls (`09_ship_damage`).
- **What decides the cost.** `docs/performance-budget.md` sets 80 draw calls and 150 k triangles; these are guides, not limits (the user, 2026-11-21). The frame rate on the phone is what counts, and the numbers are proxies for it.
  - On the Pixel 7 Pro the game holds 60 fps with p95 16.8 ms, but the frame rate is capped at 60, so that says the frame fits, not how much room is left.
  - `DebugOverlayController` reads CPU and GPU frame times from `FrameTimingManager`, but `enableFrameTimingStats` is 0 in the player settings, so on the device they are most likely 0, and the diagnostics report does not carry them.
  - Every material here is URP Lit under the SRP batcher, so a new material of the same shader costs much less than a new shader would; the draw call count overstates it.

## Proposals

Each part, with the shape change and the texture change. The shape work is the builder (`Tools/asset_builders/builders/ship`); textures come from `textures.py`, the same way as the planks and sail canvas.

### Measure first

- Turn on frame timing stats and put CPU and GPU frame times (p50, p95) in the diagnostics report, so each phase is judged on the phone by how much of the 16.7 ms the GPU uses, not by counts.
- A baseline run on the phone before any graphics change, from the same spot as later runs.

### Shared: wood

- `ShipWood` split in two slots: `Spar` (masts, yards, booms, the bowsprit, the tiller, the helm) with grain running along, and `ShipWood` (bulwark, rail, hatches, deck furniture) with boards and seams.
  Two textures, each grey and multiplied by the colour like `T_HullPlanks`, and a normal map each so grain and seams catch the sun. One more material, one more draw a ship.
- UVs in metres on poles (u along the axis, v around) and per face on boxes (boxes then need their own vertices per face: 24 instead of 8, triangles unchanged).
- The bulwark's inner face already has metre UVs, so it gets planks with no geometry change.
- Smooth normals on the sides of round poles (masts, spars, lantern post, capstan, tiller), flat on the caps, and more sides on the masts (12 to 16): the masts stop looking faceted.

### Lantern

- Shape: a frame of four corner posts and a top and bottom plate in iron, the glass a lighter pane inset on each side, a ring on the cap; about 150 triangles.
- Glow: the glass in its own emissive material, `M_LanternGlass`, rather than an emission mask on the shared `M_Palette`. Simpler, one draw.
- A real light is not proposed: there is no night for it to show in, and an extra realtime light in URP's forward path is lighting work on every object it touches, which is a GPU cost to weigh when a night comes.

### Helm

- Brig's wheel: a rim (a 16-sided ring), a hub, 8 spokes with handles past the rim, on a pedestal of two posts and a cross piece; about 400 triangles.
- Sloop's tiller: kept a tiller, with a rudder head on the transom and a slight upward curve; or the sloop gets a small wheel too (Q2).

### Hatches and the companionway

- Every hatch gets a coaming: a raised frame around it, and a grating inside, bars in `ShipWood` over a dark recess in `Tar` (both slots already on the hull, no new draw).
- One hatch per ship becomes a companionway, the way below: a hood with a sloping top and an open front, a ladder going down into a dark opening.
- Bars as geometry rather than an alpha-cut texture: a few hundred triangles each, and alpha clipping is the expensive case on a tile GPU like the Mali (it defeats the early depth test), which matters more than the draw.

### Bulwark and rail

- Planks on the inner face from the shared texture.
- Stanchions become timberheads that stand just proud of the cap rail at intervals, and the cap rail gets a small moulding (one more step in its profile).
- Cut gun ports through the bulwark's inner face is not proposed: the bulwark is one loft and the cannons already sit at it; the gain does not show from the game camera.

### Masts

- Smooth shading and the wood texture (shared, above).
- Iron hoops every 1.5 to 2 m up the lower mast, thin `Tar` rings; partners, a collar of wedges where the mast meets the deck; a cap at the masthead.
- Tops and crosstrees: a thin rim on the top's platform so it reads as a platform, not a slab.
- Each mast's parts stay in its own object (`<Name>Mast<i>`), so breaking is unchanged; the stump gets the same texture.

### Taffrail

- Shape: a rail on turned balusters (posts) instead of a solid box, with a carved board at the middle and scroll ends at the corners; quarter rails along the side of the stern deck back to the bulwark.
- Colour: keep `Gilt` for the carving and the cap, the balusters in `ShipWood` or `Trim` (both slots there already).

## Approach

- Shapes first or textures first: textures first. One texture and UVs lift every one of the six parts at once and set the look the shapes are judged against.
- Everything in the builder, both ships from one change; FBX rebuilt; probes with `build.py --probe` and captures in `Game.unity` from the poses below.
- Each phase ends with captures from the same poses as now, side by side, and the counts with both ships in view, logged as a record rather than a gate; the phone's GPU time is the gate.
- Triangles are spent where they show: rounder masts, a full wheel, turned balusters. The ships could grow from 6 k and 11 k to two or three times that; the phone says whether it matters.
- Capture poses used 2026-11-21 (fov, then eye, then target): sloop stern `40 -7,7,76 0,4,61`, sloop aft deck `45 2,9,72 0,3,58`, brig stern and wheel `40 -56,10,51 -46,4.5,63`, brig deck `45 -28,12,58 -35,4,70`, sloop mast `55 10,6,40 0,12,46`.

Phases, in [`tracking.md`](tracking.md):

1. CPU and GPU frame times in the diagnostics; a baseline run on the phone.
2. Wood: two slots and textures, UVs on poles and boxes, smooth rounder poles; planks on the bulwark.
3. Lantern and helm.
4. Hatches, the companionway, bulwark timberheads and rail moulding.
5. Masts and tops, taffrail.
6. Verify: gates, a dev APK, the phone's GPU time against the baseline.

## Open questions

- Q1: Shapes, textures, or both?
  - a. Both: the wood texture first, then the shapes, as above.
  - b. Shapes only: no new texture, more geometry and the existing slots.
  - c. Textures only: wood and the lantern's glow, the shapes as they are.
  - Recommended: a, because the texture is cheap and lifts every part, while the wheel, the hatches and the taffrail need shapes to read as what they are.
  - ANS: a, both: the wood first, then the shapes (the user, 2026-11-21, accepting every recommendation).
- Q2: The sloop's helm.
  - a. Keep the tiller, made better: a rudder head, a curve, a grip.
  - b. Give the sloop a small wheel like the brig's.
  - Recommended: a, because a tiller is right for a sloop of this size and keeps the two ships different.
  - ANS: a, the tiller kept and made better.
- Q3: The lantern's glow.
  - a. Emissive glass in its own material.
  - b. No glow until there is a night.
  - Recommended: a, because it is cheap and makes the lantern read as a lamp in daylight too.
  - ANS: a, emissive glass in its own material.
- Q4: The way below. Is "the port to go down into the ship" a companionway (a hood with a ladder going down) on one hatch, with the other hatches as gratings?
  - a. Yes, one companionway per ship, gratings on the rest.
  - b. Every hatch a companionway.
  - c. Something else (say what).
  - Recommended: a.
  - ANS: a, one companionway per ship, gratings on the rest.
- Q5: The taffrail's style.
  - a. Balusters under a gilt cap rail, a carved board in the middle.
  - b. A solid board kept, but shaped: a curved top and carved scroll ends.
  - Recommended: a, because the open rail reads from the chase camera and matches period sloops and brigs.
  - ANS: a, balusters under a gilt cap rail, a carved board in the middle.
- Q6: `docs/performance-budget.md` states 80 draw calls and 150 k triangles as targets, with the frame times beside them. Change how it reads?
  - a. Mark the counts as guides that point at a cost, and the phone's frame times (CPU and GPU, from the diagnostics) as the measure; keep the numbers as where to start looking.
  - b. Leave the doc as it is.
  - Recommended: a, because otherwise the next plan reads 80 as a limit again; done in phase 1 with the GPU times.
  - ANS: a, yes: the counts become guides and the phone's frame times the measure; done in phase 01.
