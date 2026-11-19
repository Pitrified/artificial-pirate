---
status: done
---

# Phase 06 - Detail and look

The ships are correct but read as toys: real proportions with almost none of what makes a ship read as one.
Asked 2026-11-19 by the user to probe both ways out, more realism and stylization, to learn what each element costs and how much it gives.

## The user's direction

Asked 2026-11-19 by the user:
- **Rigging.** Add some, so the sails do not float. Add the platforms on the masts and on the horizontal elements.
- **Sails.**
  - One configuration with every sail consistent for one wind direction. How they move comes later.
  - A version with no sails set, for a ship at anchor.
  - Sag, and a worn colour or texture.
- **Hull.**
  - Planks, gun ports, a figurehead, windows, decorations, a better rail and a better deck.
  - People, cannons, lanterns, flags (better still, flags flapping in the wind) and railings.
- **Shadows**, at least on the deck, better on the water too.
- **A wet band** at the waterline, static.
- **Not now**: the waves (already large), heeling (it comes with dynamic sails), the masts' and spars' diameters.
- **How to work**: iterate on the elements, benchmark each quickly, and attach a screenshot of each new element. Keep the code tidy even where it is temporary. Do research online on how to model each element.

## Research

- **Sails**: camber about 1:10 of the chord, deepest about 40% back from the leading edge ([Sail design](https://canadianboating.ca/sail-tech/how-sails-are-designed-from-measurements-to-build-ready-project/), [Draft stripes](https://practical-sailor.com/?p=836590)). A flat grid displaced along a parabola by chord position is the usual way ([sailing simulator](https://gh.r-mj.com/flyinggorilla/simulator.atterwind.info)). Billow needs enough grid along the chord ([Unity discussion](https://discussions.unity.com/t/making-a-sail-boat-sail/923442)).
- **Rigging** on game ships is either chunky geometry that reads at a distance, an alpha-blended secondary mesh, or ratlines baked into a texture ([stylized brigantine](https://www.cgtrader.com/3d-models/watercraft/historic-watercraft/stylized-pirate-brigantine-ship-3d-model-3d-model), [sailing ship pack](https://assetstore-fallback.unity.com/packages/3d/vehicles/sea/sailing-ship-pack-140769)). Ratlines do not run to the masthead ([LPC ship](https://opengameart.org/node/136432)).
- **Flags**:
  - A vertex shader of a few lines: a time-driven wave, growing from the pole outward ([vertex shader fun](https://www.moddb.com/news/vertex-shader-fun-august-15-2019), [Shader Graph example](https://unity.com/vn/blog/engine-platform/creating-animated-materials-with-shader-graph)).
  - The shadow caster pass needs the same displacement, or the shadow does not wave.
- **Shadows in a custom URP shader**: `#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE`, then `MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS))` ([Unity 6 manual](https://docs.unity3d.com/Manual/urp/use-built-in-shader-methods-shadows.html)).
- The mobile URP asset already supports main light shadows: 1024 map, 50 m, one cascade, hard.

## Plan

One element at a time. For each: build it, a probe or a capture as a screenshot to the user, the triangles and slots (`build.py`) or the scene's count (`SceneCapture -captureBudget`), and a commit when it holds.

1. **Sail configurations.** Sails become their own objects in the model, `<Name>Sails` (set for one wind) and `<Name>Furled` (sails rolled on the yards, boom and bowsprit). The prefab shows one of them: the player's ship sailing, the anchored brig furled. One wind, from the starboard quarter: the square yards braced to it, the booms and the gaff swung out to leeward, every sail bellied downwind.
2. **Sail look.** A finer grid, with camber deepest at 40% of the chord, and edges that sag between their corners. A procedural canvas texture: panel seams, edge tapes, a little dirt.
3. **Rigging and tops.**
   - Thin prisms for the lines: shrouds from the rails to the mast tops with ratline rungs, stays the jibs hang on, sheets from the clews.
   - Tops (platforms) at the lower mastheads, crosstrees and caps above them.
   - A `Rigging` slot.
4. **Hull detail.**
   - Planks and the wet band in a hull texture, mapped in metres. Wales as raised bands.
   - Gun ports, a figurehead, stern windows and a decorated taffrail.
   - A rail on posts, and a planked deck with hatches, a capstan and a wheel.
5. **Props.** Cannons at the ports, a few crew figures, stern lanterns, and flags on a waving vertex shader with a matching shadow pass.
6. **Shadows.** The sun casts them, on the deck (URP Lit) and on the water (`Water.shader` samples the main light's shadow map).

Each step adds what it costs: triangles, slots (draw calls), a texture or a shader. At the end, a table of what each element costs and gives, to choose what to keep. The performance budget (`docs/performance-budget.md`, "Real-time shadows: Off in v0.2") changes only once the phone has measured shadows.

## What the implementation found
- 2026-11-19. **The builder became a package**, `builders/ship/`. Its modules:
  - `mesh` (parts with UVs in metres, slots by name, primitives, ropes as 3-sided prisms);
  - `hull`, `rig`, `sails`, `rigging`, `fittings` and `props`.
  
  A ship is four objects:
  - `<Name>`: the hull with the standing rig, fittings and people;
  - `<Name>Flag`;
  - `<Name>Sailing` and `<Name>Anchored`: the sail states with their yards, booms, gaffs and running rigging.
  
  `build.py` prints triangles per object and can `--hide` an object from the probe. Slots are names now: a ship has as many as it uses.
- **Sails** (step 1 and 2):
  - One wind, from the starboard quarter (`WIND_FROM_DEG` 130). The yards are braced 25° to it and the boom swung 25° to leeward, with the gaff twisted a fifth more and the jibs sheeted at 60% of that. Every sail bellies downwind.
  - At anchor, the spars are squared and the sails rolled on their yards, boom and bowsprit.
  - Shape: a 6 x 6 grid, camber 0.12 of the chord, deepest at 40% on fore-and-aft sails and in the middle of square ones. The free edges are cut hollow (the foot 6%, the leech 5%).
  - `brace_deg` per mast is gone: the trim sets it.
- **Rigging and tops** (step 3):
  - shrouds from channels on the hull's sides to the lower masthead, with ratline rungs every 0.6 m; topmast shrouds from the top's edge;
  - backstays; stays between the masts and to the bowsprit, the jibs hanging on theirs;
  - braces and lifts on every yard, and sheets from the clews when set;
  - tops and crosstrees at `top_z` and `crosstrees_z`.
  
  Ropes are 3-sided prisms, 9 cm standing and 5 cm running, in a `Tar` slot.
- **Hull detail** (step 4), `fittings.py`:
  - a deck as its own surface mapped along its length;
  - a cap rail and stanchions;
  - wales, gun ports with lids, stern windows in gilt frames, and a carved taffrail;
  - a cutwater with a leaning figurehead, and head rails;
  - hatches, a capstan, a wheel or a tiller.
  
  All of it is set per type: `wales`, `gun_ports`, `stern_windows`, `deck`.
- **Props** (step 5), `props.py`:
  - cannons run out at the ports, crew figures (about 1.75 m, three shirt colours, two skin tones), and a stern lantern;
  - all of it from **one palette texture** (`builders/palette.py`, painted by `textures.py palette`, point-filtered), so many colours cost one slot.
  - The flag streams downwind from the tallest masthead on `Game/Flag`, a new shader. It is a vertex wave from hoist to fly with a droop, cheap fold shading, two-sided, and the same displacement in its shadow and depth passes. It waves in the capture (`u7_flag`).
- **Textures**, `textures.py`, all deterministic:
  - `T_SailCanvas`: 4 m tile, seams every 0.5 m, dirt;
  - `T_HullPlanks`: 0.25 m strakes over 8 m of height, clamped vertically, with the **wet band** darkening the lowest 0.9 m;
  - `T_DeckPlanks`;
  - `T_Flag`;
  - `T_Palette`.
  
  The hull's paints multiply the grey planks, so one texture serves the white sloop and the black brig.
- **Shadows** (step 6):
  - The sun casts hard shadows (it cast none before). The Lit materials receive them on the decks.
  - `Water.shader` now samples them: in shadow the water loses 45% of its sun-lit light and its glint, not the sky's reflection.
  - The mobile renderer had transparent surfaces receiving no shadows, so the water would not have shown any there; it is on now.
  - Seen in `u5_deck`: crew, rigging and masts on the deck, and a mast's shadow on the water.
  - On the way: the sloop's shadow captures looked unshadowed because its sails' shadows fell out of view. A throwaway check showed the light, the pipeline (PC level, PC asset) and the camera all set for shadows.
- **Unity**, through a throwaway editor script, deleted:
  - `ModelSetup` on both models: new materials `M_Tar`, `M_Deck`, `M_Gilt`, `M_Palette`, `M_Flag`;
  - textures and tiling on the materials (UVs are in metres: a 4 m tile is 0.25);
  - the palette point-filtered, the hull planks clamped vertically;
  - each variant shows its sailing state; the scene's `AnchoredBrig` its anchored one;
  - the brig's paint overrides re-applied by material name. The new model reorders the slots, so the old overrides, by index, would have hit the wrong slots.
- `BuiltModelTests` checks the union of all of a model's renderers' materials.

### What each element costs

Triangles per object from `build.py`:

| | Sloop | Brig |
| - | - | - |
| Hull and masts, as in phase 04 | 938 | 966 |
| + standing rigging and tops | 1,436 | 2,034 |
| + hull detail | 2,756 | 4,138 |
| + props | 3,800 | 5,678 |
| Flag | 100 | 100 |
| Sailing state | 910 | 2,012 |
| Anchored state | 318 | 706 |
| **In view, sailing** | **4,810** | **7,790** |

- The ships were 1,358 and 1,870 triangles before this phase.
- Material slots per ship: 8 on the hull object, plus the flag and 3 on the shown state, so about 12 draws, and again for the shadow casters.
- Scene, by `SceneCapture -captureBudget` with nearly everything in view: **64.7 k triangles and 48 material slots**, from 56.8 k and 36. The budget is 150 k and 80.
- Textures: five small ones, under 1 MB in all.
- Shadows: one more pass, a 1024 shadow map on mobile, one cascade, hard. The phone has to say what it costs.

What gives the most for the least, so far, by eye:
- **The sails' trim, sag and furling cost almost nothing** and change the read the most: the ship looks like it sails.
- **Rigging** is cheap in triangles (prisms) and slots (one), and it gives the ship its silhouette. The ratlines are most of its triangles.
- **Textures** (planks, canvas) cost no geometry.
- **Props through the palette** cost triangles (cannons and crew: 1,540 on the brig) but one slot. The crew give the scale.
- **Draw calls are where it would hurt**: the slots grew from 5 to 12 a ship, and the shadows double the casters. Folding the flat-coloured slots (`Tar`, `Gilt`, `ShipWood`, `HullBottom`) into the palette, and the textured ones into an atlas, would bring a ship to about 3 draws.
- **Gates**: compile 0 warnings, EditMode 146/146, PlayMode 19/19. **Dev APK** 74,043,547 bytes, 330 kB more than before the detail; the build left git unchanged. The frame rate with shadows and the detail is the phone's to measure.
- 2026-11-19, **the phone report** (Pixel 7 Pro, build `dev 04533e7+dirty` of 14:22:38Z: the detail tree, built before its commit `14de774`):
  - 137.6 s, about 131 s of it playing (4.8 to 135.5 s).
  - Frame time 16.8 ms at p05, median and p95; over the last 60 s at most 16.8 ms at p95 (one 133 ms frame at the pause). **60 fps held** with shadows, both detailed ships and the waving flag.
  - Memory 147.5 MB, as before the detail. Heap 9.7 to 10.5 MB. `gc alloc kB` 0.1 a frame at p05, median and p95. 8 collections.
  - 5 slow frames (0.06%): the boot and the Game scene's load among them.
  - No problems.
  - The user: "Brillant results".
- So on this phone the detail and the shadows fit at 60 fps. The draw-call fold (palette and atlas) is not needed for now; it is the first thing to do if more ships or a weaker phone push the frame time.
