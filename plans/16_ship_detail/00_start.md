---
status: done
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

Read from the builder and the project, not yet captured.

### Ropes

- Every rope is `mesh.line`: a three-sided prism with no caps, flat shaded, through `rigging._rope`, which also records it in `parts.ropes` for the clearance check.
- One slot for all of them, `Tar`: `M_Tar.mat` is URP Lit with no texture and no normal map, base colour 0.08, 0.07, 0.06, smoothness 0.2.
  Standing and running rigging, ratlines and the boats' falls all draw the same near black.
- The UVs are the pole's: u in metres along the rope, v in metres around it (a 9 cm shroud spans 0.28 m of v).
  No texture reads them now.
- Thickness: shrouds and stays 0.09 m, running rigging 0.05 m, ratlines 0.05 m, on every type.
- What a pixel is: at the phone's 1872 px across and the game's field of view, a pixel is about 3 mm at 5 m (the helm camera's nearest shrouds: a shroud about 30 px wide) and about 5 cm at 80 m (the chase camera behind the giant: a rope one or two pixels wide).
  So a rope's lay can show only from the helm. Far off, its colour and how it takes the light are what is left.
- No rope ends in anything: a shroud runs straight from its chainplate point on the channel to the masthead, a brace from the yard's end to the mast ahead.

### Boats and davits

- Only the giant carries boats (`"boats": [[0.31, 7.5], [0.57, 7.5]]`): four 7.5 m boats, a pair a side.
- `fittings._boat` lofts a closed hull from nine stations with a five-point section, white (`Trim`), with a flat wooden lid (`ShipWood`) at the gunwale.
  It has no sheer (the gunwale is level), no thwarts and no oars, and it is pointed at both ends.
- `fittings.boats` hangs each boat 1.4 m above the rail and 0.4 m clear of the side.
  Two davits per boat are two straight six-sided poles in `Tar`: up from the rail, then square out over the boat, with one thin line down to the boat's centreline.
  That is the black right angle the user saw.

## Leads

### Ropes

- A rope texture, made by `textures.py` like the others: three strands laid in a helix (diagonal ridges and grooves) as a normal map, and a tone with the grooves darker.
  The UVs of `line` scaled by the rope's own diameter (u in lays along it, v once round it), so a 5 cm and a 9 cm rope show the same lay.
- Smooth shading on the three-sided prism, so it shades round instead of three flat strips.
  It costs no triangles.
- Two rope slots: the standing rigging stays tarred (`Tar`, a dark brown rather than near black), and the running rigging takes a hemp tan (`Hemp`: braces, lifts, sheets, the boats' falls).
  One more slot is one more draw call per mast object and per hull shown.
- Rope ends: deadeyes with their lanyards at the shrouds' feet on the channels, and blocks at the yards' ends where the braces and lifts are made fast.
  These are small parts in the per-mast objects, at most a few thousand triangles a ship.
- Sag (Q1): a rope laid as a short polyline hanging under the straight line between its ends, instead of one prism.
  - The drop at the middle is a fraction of the rope's horizontal span, a fraction per kind: the running rigging hangs visibly (braces, sheets, the boats' falls), the standing rigging only a little (stays), shrouds and ratlines straight, as they are set up taut.
    Parabolic along the rope, straight down; a lift, near vertical, barely sags.
    The fractions are starting values, tuned on the captures.
  - `line` becomes a tube through the points, its rings shared between segments, so a sagging rope is one smooth piece; about six segments, so a rope is 36 triangles where it is 6 now.
    On the giant's running rigging (about a hundred ropes in the state shown) that is under 4k triangles more.
  - `Rope` keeps its points instead of a start and an end.
    `clearance.py` is the only reader (checked: nothing else reads a rope's ends); it tests each segment against the cloth's triangles, and `END_GRACE` counts along the whole rope from either end, so a sheet still meets its clew.
  - A sagging sheet hangs under its straight line, toward the sail's foot, so a band that cleared may not; the eased trim is the same loop as now.

### Boats and davits

- An open boat: a shell with an inside (`ShipWood`), a gunwale rail, a sheer rising toward the ends, a transom stern (a cutter's), three or four thwarts, and oars laid along the thwarts.
  From the chase camera, behind and above, you look down into it.
- Davits as curved wooden arms (`Spar`, already textured with grain): standing on the hull's side below the rail, curving out over the boat in a few segments, each with a block at its head and two falls in hemp down to the boat's ends.
- Cost: an open boat is a few hundred triangles; the four boats and eight davits are under 4k on the giant.

### Cost and look

- Cost is judged by the phone's frame time (60 fps held near both giants at GPU p50 12.8 ms), not a triangle count.
- Thin new geometry (lanyards, falls) is at or below a pixel from the chase camera, where the shimmer was; MSAA 4x settled it for the ropes there now, and should cover the new parts the same way.
  A far mip of the rope texture averages to its mean tone and a flat normal, so the texture adds no shimmer of its own.
- Pitfalls:
  - Every ship's FBX gains the `Hemp` slot, so `ModelSetup` runs again on each, and `BuiltModelTests`' slot list changes.
  - Deadeyes move a shroud's lower end up from the channel, which moves where the ratlines start.
  - Blocks are not in `parts.ropes`, so the clearance check does not see them; they are small and sit at the yards' ends.
- Seen in passing: `docs/building_assets.md` still gives "150 k triangles and 80 draw calls" as a stop condition, which `docs/performance-budget.md` now calls unmeasured starting guesses.
  The verify phase brings it in line.

## Phases

In [`tracking.md`](tracking.md); as derived:


1. Rope look: the rope texture and its normal map, the UVs by diameter, smooth shading, the `Hemp` slot and material for the running rigging, `Tar` dark brown; every ship rebuilt and imported.
2. Sag and rope ends: ropes as hanging polylines, the clearance check along their segments; deadeyes and lanyards, blocks on the yards' ends; the clearance count still 0 on every type, band and the anchored state.
3. Boats and davits: the open boat with thwarts and oars, the curved wooden davits with blocks and falls.
4. Verify: the docs, a dev APK and the user's read on the phone.

## Open questions

- Q1: How far should the rope pass go?
  - a. The look only: the lay texture and normal map, the UVs by diameter, smooth shading, tar and hemp.
  - b. a, plus deadeyes and lanyards at the shrouds' feet and blocks at the yards' ends.
  - c. b, plus sag on the running rigging (several segments a rope, and a clearance check that follows them).
  - Recommended: b, because the ends are what make a line read as rigging and they cost little; sag needs a new check for a few centimetres of droop.
  - ANS: c. "the lack of sag is quite unnatural, perhaps more than the colour. ok to update the collision checker".
- Q2: The ropes' colours.
  - a. Two slots: standing rigging tarred dark brown (`Tar`, about 0.12, 0.09, 0.06), running rigging hemp tan (`Hemp`, about 0.55, 0.45, 0.30).
  - b. One slot for all, a dark brown instead of near black.
  - Recommended: a, because tar and hemp is how a ship's rig looks and the lighter running rigging is what breaks the "black cylinders"; it costs a draw call per mast object.
  - ANS: a.
- Q3: The boats.
  - a. Open boats with a transom stern, sheer, thwarts and oars; curved wooden davits (`Spar`) with blocks and hemp falls.
  - b. As a, but still covered over, with a canvas cover (`SailCloth`) instead of the wooden lid.
  - Recommended: a, because the chase camera looks down into them and thwarts and oars read as a boat where a cover reads as a box.
  - ANS: a.
- Q4: Which types carry boats?
  - a. The giant only, as now.
  - b. The giant, and the frigate a boat on stern davits (the Hermione carries one there).
  - Recommended: a, because your note was about the giant's boats; another type is a parameter set once the builder's boat is good.
  - ANS: a.
- Q5: How the look is checked before the phone.
  The Blender probe renders take under a minute each and write only in the session scratchpad.
  A PlayMode capture from the helm or the chase camera is about 3 to 5 minutes, also only in the scratchpad.
  - a. Probe renders while building each part; one helm and one chase capture at the end of phases 1 to 3 (six runs, about 25 minutes); then the dev APK and your read on the phone.
  - b. Probe renders only, then the dev APK and your read.
  - Recommended: a, because the helm view is where the rope's lay shows and the probe does not render the game's shading.
  - ANS: a. "we are not in a rush".
