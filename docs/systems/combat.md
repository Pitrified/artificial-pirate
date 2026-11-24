# 📄 `docs/systems/combat.md`

## Overview

Ships fight with their broadsides, on their own: a ship's guns fire when a ship of another side is in range on its beam.
There is no fire button; the player fights by steering, putting the enemy on the beam and keeping it there.
Everything is per ship and per type, so any ship can fight, on any side.
Other ships sail on their own through a helm, and a sunk one comes back.

---

## Sides and targets (`Broadside`, `ShipSide`)

- Every `PF_Ship` carries a `Broadside`, with its side (`ShipSide`: `Player`, `Pirates`). A ship fires at ships of another side, never its own; an ally is a ship given the player's side.
- A ship's guns are in batteries (`Battery`): the broadsides, `Starboard` and `Port`, and on a type that carries them the bow chasers, `Bow`, and the stern guns, `Stern`. Each bears one way off the bow (90, 270, 0, 180 degrees). Every `LookSeconds` (0.25 s) each battery with guns looks for the nearest hull of another side, by `Physics.OverlapSphereNonAlloc` on the ships' layer, whose centre is within `Range` (90 m) and within its arc: `ArcDeg` (20 degrees) either side of the beam for a broadside, `ChaseArcDeg` (30 degrees) either side of dead ahead or astern for the chasers and stern guns.
- A loaded battery with a target fires a volley: its guns one after another, `VolleyGap` (0.12 s) apart, stern to bow, then reloads for `ReloadSeconds` (5 s). A target lost mid volley ends it, and the battery reloads.
- A sinking ship does not fire. `VolleyFired` (the battery) is raised as a volley starts; `TargetOf(battery)` is its target.
- The numbers every ship shares are in `SO_Gunnery` (`Assets/Settings/SO_Gunnery.asset`).

## Aim

- Each ball is aimed at a point along the target's hull at half its freeboard, picked at random within `AimAlongLength` (0.6) of its half length either side of the centre.
- `Broadside.Solve` gives the velocity at `MuzzleSpeed` (60 m/s) that carries the ball onto that point on the low arc under gravity, led by the target's velocity over the flight time (each ship keeps its own velocity from its root's movement, which covers a sailing ship and a drifting one).
- The heading is held within the arc, then scattered up to `SpreadDeg` (2.5 degrees) each way at full range, less nearer, across and up: near shots mostly hit, far ones splash around the target.
- The ball's speed is a game's, not a cannon's (about 490 m/s): slow enough to watch, about 1.5 s to 90 m.

## The guns (`SO_ShipType`)

- `Guns`: the starboard muzzles in the hull's space, stern to bow; the port guns mirror them. `BowGuns` and `SternGuns`: the chasers' and stern guns' muzzles, each listed (only the frigate and the giant have them). `Gun(battery, i)` and `GunCount(battery)` read them. They come from the ship builder (`build.py <type> --no-fbx` prints `guns`, `bow_guns`, `stern_guns`), and `BuiltModelTests` checks that a barrel's mouth is at each.
- `BallDamage` (0.06): the health one of the type's balls takes off a hull of strength 1. `HullStrength` (sloop 1, brig 1.6, frigate 2.4, giant 4): what a ball's damage is divided by on this hull. The brig sinks the sloop in about 17 hits, the sloop the brig in about 27; the frigate takes about 40, the giant about 67.
- **Barrels that aim.** Each barrel is its own object, turning about its trunnions (`<Name>GunStarboard<i>`, `<Name>GunPort<i>`, `<Name>GunBow<i>`, `<Name>GunStern<i>`, `docs/building_assets.md`), set on the variant's `Broadside` battery by battery (starboard stern to bow, port, bow, stern). A barrel at rest points from its trunnions to its mouth. While a battery has a target its barrels turn toward it at `_barrelTurnDeg` (30 degrees a second), held within the arc and raised to the solved arc; each ball leaves from its barrel's mouth; a barrel that fires runs in by `_recoil` (0.5 m) and out over `_runOutShare` (0.6) of the reload. With no target they ease back. A ship with no barrels set fires from the type's muzzles.

## Balls (`Cannonballs`)

- One pool of 32 in the Game scene, which every ship's `Broadside` fires through. A ball has no physics body: its position is its start, velocity and gravity over its age.
- Each step a ray from where it was to where it is (`Physics.RaycastNonAlloc` on the ships' layer) finds the nearest hull that is not its own ship's; a muzzle can sit inside its own hull's box.
- **A hull struck**: its `ShipDamage.Hit` at the point, for the firer's `BallDamage` over the target's `HullStrength`. The point is what the damage already uses: the mark on that side and at that height, the list toward that side, the end that goes down first, the nearest mast's chance to break (`docs/systems/ship-controller.md`).
- **The ground** (the world's `HeightAt` above the sea and above the ball) and **the water** (under `WaterSurface.HeightAt`; a ripple ring and foam) end a ball too. A ball still flying after `_maxSeconds` (6 s) is dropped; with every ball flying a shot is dropped and logged once.
- Each is an event (`Launched`, `HitHull`, `HitWater`, `HitGround`) for the effects.

## Effects (`CombatEffects`, `DamageSmoke`)

- One world-space particle system per kind in the Game scene, under `CombatEffects`: flash, smoke, splinters, splash, dirt. All five use `M_Particles` (URP Particles Unlit, `T_Particles` from `textures.py particles`: a puff, a flash, a splinter, a drop) and always simulate.
- Each burst is emitted particle by particle, each in its own direction within a cone at its own speed (one `Emit` call gives a whole burst one velocity). A burst may set its particles' colour and life over the system's.
  - a ball fired: a flash (3, 0.1 s) and smoke (8 puffs that grow and slow, 2 to 3 s); a broadside leaves a bank of smoke along the side;
  - a hull struck: splinters (16, falling) and wood-coloured dust;
  - the water: a splash (20 drops), with the ripple ring and foam;
  - the ground: dirt.
- **A wreck smokes.** `DamageSmoke` on `PF_Ship`: below a quarter of its health a ship sends dark smoke up from the rail above its heaviest kept hit every 0.3 s, living 6 s, more while it sinks.
- `M_Particles` is queued after the water (3010): both are transparent and neither writes depth, so whichever sorted later would cover the other. A particle that falls below the surface shows through until it fades.

## Ships at sea (`CircleHelm`, `ShipRespawn`)

- **A helm.** A ship other than the player's moves through the same `ShipController.SetInput` the stick uses. `CircleHelm` sails a circle: each step it heads for a point `_lookAheadDeg` (20 degrees) ahead along the circle from where the ship is, steering by the bearing off the bow (hard over at 30 degrees), so a ram or a drift puts it back on. A sinking ship no longer answers the helm.
- The Game scene's second brig sails a circle 90 m across, centred at (70, -70), at half throttle, turning to starboard, its sails set. The circle is inside the start clearing (340 m round the origin, `docs/systems/world.md`), the only water clear of islands in every world. Its sails follow the world's wind round the circle (`docs/systems/ship-controller.md`).
- **Coming back.** `ShipRespawn` keeps a ship in the scene: the scene's ship is its template, kept inactive, copied at its place when the scene starts. When the copy sinks, after `_delay` (60 s) and once the player's ship is more than `_clearance` (300 m) from that place, the sunk copy is removed and a new one made. A new copy is the only way back to a whole ship: fallen masts are destroyed as they sink. Copies keep the template's references to the scene; `GameSceneWiring` gives each respawn the world (`UseWorld`): its ground, so a moving ship stops at a shore, and its wind, for the sails.
- **The giant at anchor.** The pirate giant lies at anchor at (-150, 0, 200), inside the start clearing, which was widened for it so its whole hull floats in open water in every world; its own `ShipRespawn` brings it back.
- Neither brig is saved: both start whole on every load.

## In the player's view

- **Bars over other ships** (`ShipBars`, on the HUD under `HullBar`, `docs/systems/ui-system.md`): each ship within 180 m of the player's gets a bar `_overRail` (6 m) over its type's rail amidships (its freeboard: 9 m over the water on the brig, 16 m on the giant), its fill the health, red for another side and green for the player's.
- **Shake** (`CameraController.Shake`, `docs/systems/camera-controller.md`): small on the player's volleys (`VolleyShake`, 0.12 m), larger on the hits it takes (`HitShakePerHealth`, 10 m per share of health, at most `ShakeMost`, 0.8 m).
- The chase camera looks ahead of the ship, and a broadside fight puts the enemy on the beam, at the edge of the screen or beyond; its bar shows only when it is on the screen.

## Cost

- Balls: up to 32 renderers, one material, only while flying. Particles: five systems, five draws while any of their particles live; the smoke is the overdraw.
- Barrels: 12 objects on the sloop, 18 on the brig, 30 on the frigate and 34 on the giant (its lower row of ports is painted, with no guns), one material each. Each gun is about 390 triangles with its truck carriage (the carriages are in the hull; a covered row has none).
- No allocation per frame: fixed pools and buffers, non-allocating queries.

## Tests

- `BroadsideTests` (EditMode): a side bears on a ship within range and within the arc both ways, never on its own side, on the nearest; a volley's guns a gap apart then the reload; the solved arc through a still target and, led, onto a moving one; a ball's damage over the target's strength at the point struck, passing through its own hull; a ball that hits nothing dropped; barrels turning toward the target within the arc, raised, running in and out; chasers bearing ahead and stern guns astern and not on the beam, a type without them bearing on nothing ahead or astern, a chaser volley hitting the ship ahead.
- `CombatEffectsTests` (EditMode): a launch flashes and smokes, a hull hit throws splinters and dust, a wreck smokes from above its heaviest hit only below the threshold.
- `CircleHelmTests` (EditMode): onto the circle from inside and outside, round the way it is told, back after a push.
- `ShipBarsTests` (EditMode): a bar for each other ship in reach, none for the player's, beyond reach or behind the camera, coloured by side, filled to health; the shake dying away.
- `ShipsAtSeaTests` (PlayMode): a sunk ship back after the delay only when the player is clear; the Game scene's circling brig on its circle for a minute without running aground; the anchored giant in water deeper than its draft along its whole hull in three new worlds.
- `FightTests` (PlayMode): the sloop abeam of the anchored brig; both fire and both are hit, a bar over the brig, the sunk brig back.
- `BuiltModelTests`: a barrel's mouth at each of the type's guns, every battery; a barrel object per gun of each battery, a broadside's on its side.
