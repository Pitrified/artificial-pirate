# 📄 `docs/systems/water.md`

## Overview

Water for a mobile game: Gerstner waves that floating things ride, shore colour and foam from a depth map the world makes at runtime, ripples from splashes and bobbing, a wake behind the ship.
Decent and reactive, not photoreal; one transparent draw for the surface, no depth or opaque texture, a 256x256 ripple step per frame.
The middle ground, with no package: a Gerstner sum in the vertex shader with the same sum in C#, rather than scrolling textures (no shape) or an FFT ocean (compute shaders, large textures); the shore from a depth map rather than URP's depth texture, an extra depth copy each frame on a phone; no planar reflections, which render the scene twice.

---

## Parts

| Part | Where | What it does |
| ---- | ----- | ------------ |
| `SO_WaterSettings` | `Game.Gameplay.Water`, `Assets/Settings/SO_WaterSettings.asset` | Sea level, four Gerstner waves, colours, foam, detail normals, calm depth |
| `WaveField` | `Game.Gameplay.Water` | The Gerstner sum on the CPU: displacement, height at a world point, normal; no allocations |
| `WaterSurface` | on the `Water` object | Owns the `WaveField` and the clock; moves the grid under the ship in whole 2 m quads; hands the shader the waves, clock, colours and depth map as globals every frame; runs in the Editor too |
| `Water.shader` | `Assets/Shaders/` | Vertex: the same Gerstner sum, calmed in the shallows and far out. Fragment: the waves' normal and crest again per pixel, shallow-to-deep colour, detail normals, Fresnel to the sky, sun glint, foam (waterline swash, shore bands, crests, ripples, the wake), no water over land |
| `WorldDepthMap` | `Game.Gameplay.World`, on the `World` object | Draws the seabed depth around the ship from the world's ground into a 256 x 256 map over 512 m (0 at the surface, 1 at the depth range), a few rows a frame, and hands it to `WaterSurface`; see `docs/systems/world.md` |
| `Buoyancy` | on a Rigidbody | Floats it on a `WaterSurface` by its volume and density |
| `WaterRipples` | on the `Water` object | A wave-equation height field and a foam channel over a square, stepped by blitting between two render textures (`WaterRippleSim.shader`); the square stays put or follows a target |
| `RippleSource` | on anything that touches water | A splash on a fast entry, small rings while it bobs or moves |
| `WakeSource` | on the ship's root | Every frame it moves: a small dent in the ripples and foam at the stern and bow shoulders, by speed and frame time |
| `CannonballLauncher` | tests only, not in the Game scene | Lobs iron balls at the floaters every few seconds (`WaterRippleTests` uses it) |

---

## Rules

- **One wave function.** The shader's `GerstnerSum` and `WaveField.Displacement` must stay the same sum; a change to one is a change to both, and `WaveFieldTests` checks the C# side against the formula.
- **One clock.** Everything reads `WaterSurface.Now`; a capture or a test can pin it (`PinClock`).
- **The shader reads globals**, so a scene has one body of water at a time.
- **Ripples are visual.** Floaters ride `WaveField` only; a splash does not push them (the ball hitting them does, through physics).
- **The water receives the sun's shadows**: in shadow it loses part of its sun-lit light and its glint, not the sky's reflection (`ShadowDarkening`). The mobile renderer must let transparent surfaces receive shadows (`Mobile_Renderer`, on).
- **Ripples and the wake live in the square.** In the Game scene it follows the ship (48 m, about 24 m each way), so its wake is always drawn; things farther away make no ripples. The square moves whole texels at a time and the step reads the field shifted by them, so ripples keep their place in the world.
- **The drawn waves calm in water shallower than `CalmDepth`; the CPU waves do not.** A floater in very shallow water rides a little high. Keep floaters in deeper water, or add the depth map to `WaveField` if it matters.
- **The grid follows the ship.** A fine core of 256 m (2 m quads) and coarse flat quads of 64 m out to 2.3 km, past the fog's end. It moves in whole fine quads, so its waves do not swim. The drawn waves' height calms to nothing from 100 to 124 m from the grid's centre (`_WaterFarCalm`), so the core's edge is flat and the coarse quads cannot crack against it; the CPU waves do not calm there, which is fine as long as floaters stay near the ship.
- **The waves' light goes out to the fog.** The fragment works out the waves' normal and crest height itself, so water past the calm is still lit as waves. A wave fades as its wavelength nears four pixels of water, so the short ones do not shimmer far out. It costs the four-wave sum again in each water pixel. Past 60 m from the camera, fading in to 200 m, the detail map is sampled once more at a tenth of its tiling (about 80 m a repeat), so the far water shows broad uneven patches across the long waves rather than only their lines.
  That layer is turned 37 degrees off the near layers, so the map's grain does not line up across them.
  Far out only the long waves are resolved, and two plane waves alone draw straight, even lines to the horizon; so over the same 60 to 200 m each wave's strength in the light swings up and down in patches (the detail map read at about 140 m a repeat, a different reading for each pair of waves).
  The vertex waves, the CPU's `WaveField` and buoyancy are untouched: only the far water's light changes.

---

## Adding water to a scene

1. A water grid mesh (`Tools/asset_builders`: `build.py water_grid`) with the `M_Water` material.
2. `WaterSurface` on the grid, with an `SO_WaterSettings` and the ship to follow.
3. The seabed depth: a `WorldDepthMap` with the water, the `WorldStreamer` and the ship (`docs/systems/world.md`). Without one, the shader reads whatever depth map was set last.
4. Optional: `WaterRipples` on the same object with `M_WaterRippleSim`, centred where things happen or following the player's ship.
5. A procedural sky whose ground colour is the far water's, or the horizon shows a band past the grid's edge.

## Making something float

- A Rigidbody, a collider, `Buoyancy` with the body's **volume**, its **density relative to water** (under 1 floats) and **sample points**.
- Each point is the centre of an equal slice of the volume, `columnHeight` tall. **Spread them through the whole body, top and bottom**: points only on the lower half make it roll over and sink once they end up on top. A box: its eight octant centres. A barrel: two rings at the centres of its halves. A sphere: the corners of its inner cube.
- The draft follows from the density: a 1 m cube of density 0.5 floats with its centre at the waterline (`BuoyancyTests`).
- Do not start a body overlapping a one-sided mesh collider (a beach): it can be pushed through to the underside.
- A `RippleSource` with the water and the ripples gives it splashes and rings.

## A wake

A `WakeSource` on whatever moves the hull on the plane (the ship's root), with the ripples and the ship's `SO_ShipType` for the hull's half length and beam.
The dent's radius (`_dentRadiusHalfBeams`) and the foam's are in half beams, so one setting suits every size of ship.
It replaces a `RippleSource` on a hull: rings dropped at intervals read as a chain of circles behind a moving ship.
Stamped every frame, the dents merge; ripples in the field travel at about 4 m/s (0.71 texel a step, 0.1875 m texels, 30 steps/s), so a hull faster than that leaves a V of them, about 30 degrees each side at 8 m/s.
The foam it stamps fades (`_foamFade`, halved in about 1.5 s) and spreads (`_foamSpread`), so the trail widens and breaks up behind the stern.
Its strengths (`_dentStrength`, `_foamRate`) and the shader's `_WakeFoam` are the tuning knobs.

---

## Cost

Measured in the sample scene with `SceneCapture -captureBudget` (renderers in view, before batching), 2026-11-18:

| View | Triangles | Material slots |
| ---- | --------- | -------------- |
| The game's start | 56.7 k | 30 |
| Over the bay | 49.0 k | 16 |
| Above the island, nearly everything | 63.7 k | 39 |

Against `docs/performance-budget.md`: 150 k triangles and 80 draws. The water grid is 35.3 k of it (a core of 128 x 128 quads of 2 m, so the shortest wave should stay at 3 m or more, and 1 280 coarse quads around it). On the GPU per frame: one 256x256 ripple step (RGBA half: height, height a step ago, foam), the surface's fragment work (five texture reads, a handful of sin/cos in the vertex stage). The device run is what settles it.

---

## Checking it

- `WaveFieldTests` (EditMode), `BuoyancyTests`, `FloatingPropsTests`, `WaterRippleTests` (PlayMode).
- Look at it moving, not still: the sequence capture and `measure.py sheet / gif / motion / track` (`docs/headless-unity.md` §3).
