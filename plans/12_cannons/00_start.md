---
status: in progress
priority: 1
description: |
  Combat: each ship's cannons fire on their own when an enemy is in range on the beam, the balls fly
  and hit or splash, a hit damages the hull where it lands. Flash, smoke, splinters and splashes from
  particles; the barrels possibly aimed. Moves "cannons and combat" from Next to In the game.
---

# Cannons

Asked 2026-11-23 by the user, after `11_game_spec` was merged:

> ok we now add some combat gameplay - cannons autofire when the enemy ship is in range
>
> 1a like the cannon points 90deg to the right of our ship, we can assume it can be aimed like 20 deg bow/aft
> 1b can we actually move the cannons in our ship to aim ? cool, not a must
>
> 2a one animation of explosion/fire out of the mouth of the cannon, ball fires toward other ship
>
> 3a if hit, another animation maybe? which?
> 3b for sure, when hit, does damage, lowers health. if possible, damage close to actual hit
>
> propose other cool things
> research online when needed

## What is there

- **The guns are modelled, baked in the hull.** `props.py cannons()` puts a carriage and a barrel at every gun port, both sides: the sloop 6 a side, the brig 9 (`gun_ports` in `sloop.py`, `brig.py`; centres from `fittings.py gun_port_centres`). They are part of the hull's mesh in the Palette slot, so they cannot move today.
- **A weapon's entry point exists.** `ShipDamage.Hit(worldPoint, amount)` ("what a weapon will call"). The hit is kept in the hull's space and drives everything already: the mark on the side and at the height it landed (`HullMarks` clamps it between the waterline and the rail), the list toward that side, the slower top speed, the settle, the nearest mast's chance to break, sinking at 0 and game over for the player. So 3b, damage where the ball lands, comes for free once a ball knows its hit point.
- **Balls and splashes, for the water's tests.** `CannonballLauncher` lobs pooled Rigidbody balls (iron `Buoyancy`, a `RippleSource` splash). Not in the Game scene. The splash ring through `WaterRipples.AddImpulse` is reusable as is.
- **The enemy.** `AnchoredBrig` in the Game scene: a `PF_Ship_Brig` with its `ShipController` removed, near the start island, not saved, whole on every load, gone once sunk.
- **No particles, no sound yet.** The ParticleSystem module is in the manifest (built in, no package). `AudioManager.PlaySfx` plays 2D one-shots, and the project has no audio clips at all.
- **Budgets** (`performance-budget.md`): judged by the diagnostics report's cpu and gpu p95 on the phone against a run from before; no realtime lights besides the sun, so a muzzle flash is an emissive particle, not a light.

## Research, 2026-11-23

- **Age of sail numbers** ([Naval artillery in the Age of Sail](https://en.wikipedia.org/wiki/Naval_artillery_in_the_Age_of_Sail), [Broadside](https://en.wikipedia.org/wiki/Broadside_(naval)), [32-pounder](https://en.wikipedia.org/wiki/32-pounder_gun), [Britannica](https://www.britannica.com/topic/naval-warfare/The-age-of-fighting-sail)): muzzle velocity about 490 m/s; accurate from a rolling deck to a few hundred metres at best, often fought much closer; a trained crew fired a broadside every 2 to 3 minutes. A ball at 490 m/s is invisible and a 3-minute reload is no game on a phone, so both are scaled: a ball slow enough to watch arc (about 60 m/s, under 2 s of flight) and a reload of seconds.
- **Games** ([AC Black Flag naval combat](https://allthings.how/how-naval-combat-works-in-assassin-s-creed-black-flag-resynced/), [Sunless Sea combat](https://sunlesssea.fandom.com/wiki/Combat)): broadsides fire only to the side, so you sail at an angle to the target; firing arcs per weapon; the reload shown on the HUD, the aim line dim while cooling down. Sunless Sea pauses a weapon's warm-up, without resetting it, while the target is outside its arc. With autofire the game is the same as Black Flag's minus the button: steer to put the enemy on the beam and keep it there.
- **Particles on mobile** ([Unity mobile optimization, part 2](https://unity.com/how-to/mobile-game-optimization-tips-part-2), [Unity 6 mobile graphics tips](https://discussions.unity.com/t/unity-6-mobile-graphics-optimization-tips-and-tricks/1593293)): the cost is overdraw from large transparent sprites; keep them few and small, URP's Particles Unlit shader, check with the Frame Debugger. VFX Graph needs compute and has a mobile history of issues: the built-in ParticleSystem is the fit.

## Proposal

### Guns and firing (1a)

- **Where the guns are** goes into `SO_ShipType` from the builder's `gun_ports` (count, from, to, height), the way the hull's half-beam curve went in, and `BuiltModelTests` checks it against the model. Each muzzle is then a point on the hull's side, from `HalfBeamAt`.
- **A `Broadside` component on `PF_Ship`'s root**, wired to its `ShipDamage` like the others: two batteries, port and starboard. A battery fires when an enemy is within range and its bearing is within 20 degrees of the beam, bow or aft (1a), and the battery is loaded.
- **A volley as a ripple**: the guns of the side go one after the other, about 0.12 s apart, then the side reloads for a few seconds. It reads better than single guns trickling, and gives a rhythm the reload indicator can show.
- **Aim**: each ball is solved ballistically for a fixed muzzle speed toward where the target will be (its drift and speed led), the yaw clamped to the 20 degrees, the low arc; then a spread grows with range, so near shots mostly hit and far ones splash around. The aim point is along the target's hull at mid-freeboard, so hits land along its length, bow to stern.
- **Enemies**: a ship is an enemy of another when its `Broadside` says a different side (the player's or not). The player's ship fires at any other ship; the others at the player's (Q2).

### Balls and hits (2a, 3b)

- **Kinematic balls, no Rigidbody**: a pool per scene, each a position from its start, velocity and gravity, moved each frame; a `Physics.Linecast` from the last position to the new against the `Ship` layer finds the hull and the exact point. Deterministic, as movement is; no physics stepping.
- **On a hull**: `ShipDamage.Hit(point, amount)`; the firing ship's own hull is skipped. The mark, list and mast are the existing ones. With 16 kept hits a long fight drops the oldest marks; that is the limit as it stands, kept unless the capture says otherwise.
- **On the water** (the ball under `WaterSurface.HeightAt`): a splash, the ripple ring, the ball gone. **On land** (under the world's `HeightAt`): a puff of dirt.
- **How much a ball does**: by the firing type's ball and the target type's hull strength (Q3).

### Effects (2a, 3a)

One ParticleSystem per kind of effect in the scene, emitted at a point with `Emit(EmitParams, n)`: no object per shot, no allocation, one draw per kind. Textures from `textures.py` (a soft puff, a flash, a splinter), one small atlas, URP Particles Unlit.

| Moment | Effect | Notes |
| -- | -- | -- |
| A gun fires | **flash**: two or three bright billboards, 0.08 s; **smoke**: 6 to 8 puffs that grow, rise a little, drift downwind and fade over 2 to 3 s | the smoke stays where it was fired, so a ship sailing on leaves a line of it behind |
| A ball flies | the ball, a small dark sphere | no trail: a trail renderer per ball is a draw each |
| A hull is hit (3a) | **splinters**: 10 to 15 wood-coloured chips thrown out from the side, falling under gravity into the water; **a dust puff**; the mark appears | the classic age-of-sail hit; no fire on a single hit |
| A ball misses | **a splash column**: white drops up and falling, plus the ripple ring | the splashes walking toward the target are how the player reads the range |
| A ball lands on an island | a dirt puff | |

### Barrels that aim (1b)

Possible, and the last phase so the rest does not wait on it: the builder exports each barrel as its own object (`<Name>Cannon<side><i>`, pivoted at its trunnions), and the battery turns each toward its ball's aim within the 20 degrees, raises it to the solved elevation, and runs it in on firing (a recoil of half a metre, out again over the reload). The carriages stay in the hull. The cost is a renderer per barrel, 12 on the sloop and 18 on the brig, all one material, so the SRP batcher keeps them cheap on the CPU; the phone decides (Q5).

### Other things that would add (for Q4)

- a. **The brig fires back**: the same `Broadside` on every ship; without it there is no danger and the hull bar never moves in a fight.
- b. **Reload on the HUD**: a small arc each side of the hull bar filling as the battery reloads, lit when it is loaded and something is in its arc.
- c. **The enemy's health**: a small bar over the enemy ship while it is in range.
- d. **Sound**: a boom per gun, a crunch on a hit, a splash on a miss, synthesized by a builder script into `.wav` (no clips exist and none are bought), played by distance.
- e. **Camera shake**: small, on the player's volley, larger when the player is hit.
- f. **A burning wreck**: below a quarter of its health a ship smokes from its worst hit, and the smoke thickens as it sinks.
- g. **The brig comes back**: after it sinks, another brig anchors somewhere else out of sight, so there is always something to fight. A brig that sails and hunts is "other ships under sail" in Next, its own feature.
- h. **Aim for the rig**: chain shot that brings masts down rather than holing the hull. A choice per battery needs a control; later.

## Phases, proposed

Superseded by [Decisions](#decisions) after Q1 to Q6; kept as first written.

| # | Phase | What |
| -- | -- | -- |
| 01 | Guns and balls | gun ports in `SO_ShipType`, `Broadside` with the arc, range, volleys and aim; the ball pool and its hits on hulls, water and land; plain balls, no effects; EditMode tests |
| 02 | Effects | the particle textures, the per-kind systems, flash, smoke, splinters, splash, dirt; sound if Q4 d |
| 03 | The fight | what Q4 picks of a, b, c, e, f, g |
| 04 | Barrels that aim | if Q5 a |
| 05 | Verify | PlayMode test of a fight, a capture, the spec (Scope, Systems: a new `docs/systems/combat.md`) and `ship-controller.md`; the phone run against a baseline in a fight |

## Open questions

- Q1: The numbers to start from, to be tuned on the phone.
  - a. Range 90 m (the brig is about 34 m long, so at 90 m it is a fair target and the splashes are seen), arc 20 degrees each side of the beam, muzzle speed 60 m/s (1.5 s to 90 m), 0.12 s between guns in a volley, reload 5 s for the side.
  - b. Other numbers: say which.
  - Recommended: a, because it gives a fight of about a minute at a visible scale, and every number is a field on a settings asset.
  - ANS: a.
- Q2: Who is an enemy.
  - a. The player against every other ship; the others never fire at each other.
  - b. Factions per ship, set on its prefab instance.
  - Recommended: a, because there is one other ship; factions wait for a second one that needs them.
  - ANS: b, a side per ship, set on the ship: allies may come.
- Q3: How much a ball does.
  - a. Per type: `BallDamage` of the firing type, divided by `HullStrength` of the target type. Start from a ball of 0.06 on both, the sloop's strength 1 and the brig's 1.6: the brig sinks the sloop in about 17 hits (two volleys that mostly hit), the sloop the brig in about 27 (five).
  - b. The same damage for every ball on every ship.
  - Recommended: a, because a bigger hull should take more, and it is two fields. 1.6 rather than the brig's 3.7 times the mass, and the brig anchored and unable to turn its broadside: a sloop that keeps off its beam can win against 9 guns.
  - ANS: a, tunable.
- Q4: Which of the other things (a to h above) are in this feature? Several letters.
  - Recommended: a, b, d, e, f. a and b make it a fight one can read; d and e are what makes a cannon feel like one; f is cheap with the smoke already made. c, g and h later: c needs a world-space UI, g changes the world's scope, h needs a control.
  - ANS: a yes, and the whole of it usable by any ship, nothing about the sloop in particular; b no; c yes; d not now, future work; e yes; f yes; g yes, and a second brig, under sail, sailing in a circle; h not now, future work.
- Q5: Barrels that aim (1b).
  - a. In this feature, as its own phase after the fight, kept only if the phone run shows no cost.
  - b. Later, its own feature.
  - Recommended: a, because it is what makes the 20 degrees visible, and its own phase keeps the rest independent of it.
  - ANS: a.
- Q6: Autofire only, or also a button.
  - a. Autofire only, as asked.
  - b. Autofire, plus a button to hold fire.
  - Recommended: a, because it keeps the controls one stick; a hold fire button can come with a reason (a ship that is not an enemy).
  - ANS: a, for now.

## Decisions

Folded in from Q1 to Q6, 2026-11-23.

- **Any ship.** Everything is a component and settings on `PF_Ship` and its `SO_ShipType`: the guns, their balls, the hull's strength, the side it is on, the smoke when badly damaged, the health bar. No code names the sloop or the player's ship except the camera shake, which follows what the camera follows.
- **A side per ship** (`Broadside`'s side, set on each ship): a ship fires at the nearest ship of another side in its arc and range, never at its own side. The player's ship and the two brigs start on two sides; an ally is a ship given the player's side.
- **The numbers** of Q1 a and Q3 a, as fields on a settings asset (the range, arc, muzzle speed, spread, volley gap and reload) and on `SO_ShipType` (`BallDamage`, `HullStrength`).
- **In:** the brig fires back (a), a health bar over other ships (c), camera shake (e), smoke from a badly damaged ship (f), sunk ships coming back and a second brig sailing a circle (g), barrels that aim (Q5 a). **Autofire only** (Q6).
- **Future work:** sound (d), chain shot at the masts (h), a reload indicator (b, declined). d and h go into the spec's Next list in the verify phase, not into folders of their own until they are picked up (Q10).
- **A ship that sails on its own** is a helm component that steers its `ShipController` through `SetInput`, the way the stick does for the player. `ShipController` stays the movement of every ship that moves; what changes is that it is no longer the player's only (the scene's wiring and the save pick the player's by reference, as now).

## Phases

| # | Phase | What |
| -- | -- | -- |
| 01 | Guns and balls | gun ports in `SO_ShipType` from the builder, checked by `BuiltModelTests`; `Broadside`: side, arc, range, volleys, aim; the ball pool, hits on hulls (`Hit` scaled by `BallDamage` and `HullStrength`), water and land; plain balls; on both ships in the Game scene; EditMode tests |
| 02 | Effects | the particle textures by `textures.py`; one ParticleSystem per kind; flash, smoke, splinters, splash, dirt; smoke from a badly damaged ship; a capture |
| 03 | Ships at sea | a circling helm; the second brig under sail; every moving ship wired to run aground; sunk ships coming back (Q8); tests |
| 04 | The fight | a health bar over other ships (Q9), camera shake; a PlayMode test of a fight |
| 05 | Barrels that aim | barrels exported as their own objects, turned and raised to the shot, recoiling; kept if the phone shows no cost |
| 06 | Verify | the spec (Scope, Systems: `docs/systems/combat.md`, Next gains d and h), `ship-controller.md`; the phone run in a fight against a baseline |

## Open questions, second batch

- Q7: The circling brig's circle.
  - a. A circle 120 m across around a point in open water about 350 m from the start island, where the first fight with the anchored brig is out of its range; at half throttle, steered to hold the circle (it heads for a point ahead on the circle, so a ram or a drift puts it back on it), turning to starboard.
  - b. Other: say where and how big.
  - Recommended: a, because steering to the circle survives being pushed, which a fixed rudder does not; the centre and radius are fields on the helm.
  - ANS: a.
  - Corrected in phase 03: the world's islands come from the seed, which changes with every new game, so open water 350 m out in one world is an island in another. The only water clear in every world is the start clearing, 160 m round the origin (`SO_WorldSettings`). The circle is inside it: 90 m across (radius 45), its centre 99 m south-east of the origin, about 130 m from the anchored brig and out of the guns' range of the player's start. The rest of the answer holds.
- Q8: Sunk ships coming back.
  - a. A sunk ship that is not the player's comes back whole at its own start (the anchored brig at its anchor, the circling one on its circle) once 60 s have passed and the player's ship is more than 300 m from that start, so it never appears in sight.
  - b. Somewhere new each time.
  - Recommended: a, because it needs no search for open water; somewhere new comes with ships that sail where they like.
  - ANS: a.
- Q9: The health bar over other ships.
  - a. Over every ship but the player's, within twice the guns' range, coloured by side (an enemy's red, an ally's green), a uGUI element on the HUD placed over the ship each frame.
  - b. Over enemies only.
  - Recommended: a, because with allies coming the colour says which is which; a HUD element placed from the camera needs no world-space canvas.
  - ANS: a.
- Q10: Future work (d sound, h chain shot).
  - a. Into the spec's Next list in the verify phase.
  - b. A draft folder each now, on its own branch.
  - Recommended: a, because the Next list is where wanted but unplanned work lives since `11_game_spec`; a folder when one is picked up.
  - ANS: a.
