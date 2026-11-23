# Cannons - implementation tracking

Ships' cannons fire on their own at an enemy on the beam; balls hit where they land, with flash, smoke, splinters and splashes. Analysis, research, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- Any ship can carry all of it: a component and settings on `PF_Ship` and `SO_ShipType`; a side per ship, fire at the nearest ship of another side.
- Balls are kinematic, a linecast per frame; a hit calls the existing `ShipDamage.Hit` at its point, scaled by the firer's `BallDamage` and the target's `HullStrength`.
- Effects are one ParticleSystem per kind, emitted at points; no object per shot.
- A ship that sails on its own steers its `ShipController` through `SetInput`.
- Future work, to the spec's Next list: sound, chain shot.

## Phases

| #  | Phase             | Plan | Status |
| -- | ----------------- | ---- | ------ |
| 01 | Guns and balls    | [`01_guns_and_balls.md`](01_guns_and_balls.md) | done |
| 02 | Effects           | [`02_effects.md`](02_effects.md) | done |
| 03 | Ships at sea      | [`03_ships_at_sea.md`](03_ships_at_sea.md) | done |
| 04 | The fight         | [`04_the_fight.md`](04_the_fight.md) | done |
| 05 | Barrels that aim  | [`05_barrels_that_aim.md`](05_barrels_that_aim.md) | done |
| 06 | Verify            | [`06_verify.md`](06_verify.md) | in progress |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-23 : bootstrapped the folder on `feat/12_cannons` from `main` at `a0e18d4`: the guns are modelled but baked in the hull; `ShipDamage.Hit` already places a hit's mark, list and mast break where it lands; no particles or audio clips yet. Research on age of sail guns, broadside games and mobile particles. Proposed kinematic balls with a linecast, volleys within 20 degrees of the beam, one ParticleSystem per kind of effect, aimable barrels as a last phase; eight extras; Q1 to Q6.
- 2026-11-23 : the user: Q1 a; Q2 a side per ship (allies may come); Q3 a; Q4 a, c, e, f, g yes, b no, d and h future work, plus a second brig sailing a circle; Q5 a; Q6 a. Folded in: six phases, ships at sea added; Q7 to Q10.
- 2026-11-23 : the user: Q7 to Q10 a (all recommended), proceed. Sub-plans 01 to 06 written. Review of the sub-plans: a muzzle amidships can sit inside its own hull's box, so a ball's query takes the nearest hit not its own (not a plain linecast); the lead needs each ship's velocity, kept by `Broadside` from its position (an anchored ship has no `ShipController`); `Instantiate` puts a respawned copy in the active scene, so it is moved to the spawner's; effects hear launches from `Cannonballs`, not from each ship. Corrected in place.
- 2026-11-23 : phase 01 - review before: the allocation check moved to the phone (only a player measures it). The builder reports each type's muzzles; `SO_ShipType` carries them with `BallDamage` and `HullStrength` (brig 1.6); `SO_Gunnery`; `Broadside` (side, arcs, volleys, the solved and led arc, scatter); `Cannonballs` (pool of 32, a ray per step skipping the firer, damage at the point, water splash and foam, ground); wired on `PF_Ship` and in the Game scene by a throwaway editor script. 9 new tests. Capture: a volley of six all striking the brig, marks along its side. Gates green (EditMode 242, PlayMode 24).
- 2026-11-23 : phase 02 - review before: one `Emit` call gives a whole burst one velocity, so bursts emit particle by particle. `textures.py particles` (`T_Particles`, puff, flash, splinter, drop); `M_Particles`; `CombatEffects` with five systems in the Game scene, hearing `Cannonballs`; `DamageSmoke` on `PF_Ship`. Found and fixed: the water drawing over the smoke (particles queued after it), damage smoke trapped in the hull (from the rail, longer life), a puff too soft to read. 3 new tests. Gates green (EditMode 245, PlayMode 24).
- 2026-11-23 : phase 03 - review before: tests that find the ship by type would find the brig (`GameManager.PlayerShip` added); the circle 350 m out is an island in some worlds (moved into the start clearing, Q7 corrected). `CircleHelm`, `ShipRespawn` (the scene's ship as an inactive template, copied, a new copy 60 s after sinking once the player is 300 m away), the ground for every ship that comes back; a second brig under sail circling south-east. 6 new tests. Gates green (EditMode 250, PlayMode 26). The sails' fixed trim looks wrong on half the circle: left.
- 2026-11-23 : phase 04 - review before: nothing to correct. `ShipBars` on the HUD (a pool of 4, ships within 180 m, red or green by side, fill at health), `CameraController.Shake` (tuning on `SO_CameraTuning`; the player's volleys and hits), wired by `GameSceneWiring`; a PlayMode fight (both hit, a bar over the brig, the sunk brig back). 3 new tests. Gates green (EditMode 252, PlayMode 27). Found: in the chase view the enemy on the beam is off the screen, so its bar never shows: Q11. The brig sinks the sloop in two volleys, as Q3 said: for the phone.
- 2026-11-23 : phase 05 - review before: the phone's verdict on the barrels moves to phase 06's run. Each barrel its own object turning about its trunnions (`make_object` takes an origin); `Broadside` aims each side's barrels at its target within the arc, fires from the mouth, runs them in by 0.5 m and out over 0.6 of the reload. 2 new tests. Gates green (EditMode 255, PlayMode 27). Capture: barrels off square and raised once the smoke clears.
- 2026-11-23 : phase 06 - review before: nothing to correct. `docs/systems/combat.md` written; the spec (the game, Scope, the HUD, Systems, a release check), `ship-controller.md`, `ui-system.md`, `camera-controller.md` and `building_assets.md` updated; spec check pass, links 0 broken, citations clean. Dev APK built for the phone run. Waiting on the phone (the baseline at `a0e18d4` and a fight on this branch, which also decides the barrels) and Q11.
