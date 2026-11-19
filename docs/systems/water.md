# 📄 `docs/systems/water.md`

## Overview

Water for a mobile game: Gerstner waves that floating things ride, shore colour and foam from a baked depth map, ripples from splashes and bobbing, a wake behind the ship.
Decent and reactive, not photoreal; one transparent draw for the surface, no depth or opaque texture, a 256x256 ripple step per frame.
The choice and the alternatives are in the guide, unity-setup-project's `plans/04_water/00_start.md`.

---

## Parts

| Part | Where | What it does |
| ---- | ----- | ------------ |
| `SO_WaterSettings` | `Game.Gameplay.Water`, `Assets/Settings/SO_WaterSettings.asset` | Sea level, four Gerstner waves, colours, foam, detail normals, calm depth |
| `WaveField` | `Game.Gameplay.Water` | The Gerstner sum on the CPU: displacement, height at a world point, normal; no allocations |
| `WaterSurface` | on the `Water` object | Owns the `WaveField` and the clock; hands the shader the waves, clock, colours and depth map as globals every frame; runs in the Editor too |
| `Water.shader` | `Assets/Shaders/` | Vertex: the same Gerstner sum, calmed in the shallows. Fragment: shallow-to-deep colour, detail normals, Fresnel to the sky, sun glint, foam (waterline swash, shore bands, crests, ripples, the wake), no water over land |
| `WaterDepthBake` | `Game.Editor` | Casts rays down over the water's area, writes `T_WaterDepth.png` (0 at the surface, 1 at the depth range) |
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
- **Ripples and the wake live in the square.** In the Game scene it follows the ship (48 m, about 24 m each way), so its wake is always drawn; things farther away make no ripples. The square moves whole texels at a time and the step reads the field shifted by them, so ripples keep their place in the world.
- **The drawn waves calm in water shallower than `CalmDepth`; the CPU waves do not.** A floater in very shallow water rides a little high. Keep floaters in deeper water, or add the depth map to `WaveField` if it matters.

---

## Adding water to a scene

1. A ground with colliders under the water (`ModelSetup -colliders` for a model), and a water grid mesh (`Tools/asset_builders`: `build.py water_grid`) with the `M_Water` material, at the origin.
2. `WaterSurface` on the grid, with an `SO_WaterSettings`; set its depth rect to cover the area.
3. Bake: `-executeMethod Game.Editor.WaterDepthBake.BakeFromCommandLine` (`docs/headless-unity.md`); bake again when the shore changes. Colliders on a Rigidbody (floating props, a kinematic ship) are passed through, so only the ground counts.
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

Against `docs/performance-budget.md`: 150 k triangles and 80 draws. The water grid is 33.8 k of it (130 x 130 cells of 2 m, so the shortest wave should stay at 3 m or more). On the GPU per frame: one 256x256 ripple step (RGBA half: height, height a step ago, foam), the surface's fragment work (five texture reads, a handful of sin/cos in the vertex stage). The device run is what settles it.

---

## Checking it

- `WaveFieldTests` (EditMode), `BuoyancyTests`, `FloatingPropsTests`, `WaterRippleTests` (PlayMode).
- Look at it moving, not still: the sequence capture and `measure.py sheet / gif / motion / track` (`docs/headless-unity.md` §3).
