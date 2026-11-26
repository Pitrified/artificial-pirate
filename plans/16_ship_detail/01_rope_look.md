---
status: done
---

# Phase 1 - Rope look

## Overview

The rope's surface: what reads from the helm (the lay) and from the chase camera (the colour and round shading).
First because it touches every rope without changing where any rope runs, so the clearance count cannot move.
Context: [`00_start.md`](00_start.md).

## Goals

1. A rope texture and its normal map from `textures.py`: three strands laid in a helix, the grooves darker, tiling once round the rope and once per lay along it.
2. A rope primitive in `mesh.py` beside `line`: u in lays along the rope and v once round it, by the rope's diameter, shaded smooth.
   `line` stays as it is for the bow's head rails (`Trim`, textured in metres); the rigging, the footropes of the manned yards and the boats' falls take the rope.
3. Running rigging (braces, lifts, sheets, the main sheet, the boats' falls) in a new `Hemp` slot; the standing rigging and ratlines stay `Tar`, dark brown.
4. `M_Hemp.mat` and `M_Tar.mat` take the rope texture and normal map; every ship rebuilt, its FBX set up again for the new slot, `BuiltModelTests` lists `M_Hemp`.

## Plan

- `textures.py rope`: `T_Rope.png` and `T_Rope_N.png`, a small tile (128 px), the helix as diagonal ridges with an integer count round and along so it tiles.
- The rope's UVs: u = distance / (lay length), lay length about three diameters; v = angle / 2 pi, closing at 1 on the last face.
  Three sides kept: the giant shows about 1180 ropes, most of them ratlines, 7.2k triangles; the helm capture judges the silhouette.
- Tangents import as MikkTSpace (`tangentImportMode: 3` in the models' meta), so the normal map needs nothing more.
- The davits are `Tar` poles until phase 3 makes them `Spar`; they take the rope texture in metres meanwhile.
- `rigging._rope` takes the slot by kind; the boats' fall in `fittings.boats` uses `Hemp`.
- `Hemp` in every type's `preview_rgb`.
- Materials set with a throwaway editor script (textures, tiling 1, colours: `Tar` about 0.12, 0.09, 0.06, `Hemp` about 0.55, 0.45, 0.30, low smoothness), deleted before the commit; `ModelSetup` on the four ships.
- Probe renders of a mast close up; then one helm and one chase capture of the giant.

## Out of scope

- Where ropes run, sag, rope ends (phase 2).

## Done when

- The clearance count unchanged (0) on every type, band and the anchored state.
- Gates green; the captures in the scratchpad, shown to the user.

## What the implementation found
The first helm capture showed the lay on the near shrouds, but the tar read blue-black: on so dark a colour the sky's reflection and ambient won over the brown.
`Tar` raised to 0.24, 0.16, 0.09 at smoothness 0.1 (with `MaterialTuning`); the second capture reads dark brown, the lay on the lit side, dark against the sky.
From the chase camera behind the giant a rope is one or two pixels: only its colour shows there, as expected.
Triangles unchanged (the rope is the same three-sided tube); clearance 0 on every type, band and the anchored state.
