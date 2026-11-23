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
| 01 | Guns and balls    | [`01_guns_and_balls.md`](01_guns_and_balls.md) | in progress |
| 02 | Effects           | [`02_effects.md`](02_effects.md) | planned |
| 03 | Ships at sea      | [`03_ships_at_sea.md`](03_ships_at_sea.md) | planned |
| 04 | The fight         | [`04_the_fight.md`](04_the_fight.md) | planned |
| 05 | Barrels that aim  | [`05_barrels_that_aim.md`](05_barrels_that_aim.md) | planned |
| 06 | Verify            | [`06_verify.md`](06_verify.md) | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-23 : bootstrapped the folder on `feat/12_cannons` from `main` at `a0e18d4`: the guns are modelled but baked in the hull; `ShipDamage.Hit` already places a hit's mark, list and mast break where it lands; no particles or audio clips yet. Research on age of sail guns, broadside games and mobile particles. Proposed kinematic balls with a linecast, volleys within 20 degrees of the beam, one ParticleSystem per kind of effect, aimable barrels as a last phase; eight extras; Q1 to Q6.
- 2026-11-23 : the user: Q1 a; Q2 a side per ship (allies may come); Q3 a; Q4 a, c, e, f, g yes, b no, d and h future work, plus a second brig sailing a circle; Q5 a; Q6 a. Folded in: six phases, ships at sea added; Q7 to Q10.
- 2026-11-23 : the user: Q7 to Q10 a (all recommended), proceed. Sub-plans 01 to 06 written. Review of the sub-plans: a muzzle amidships can sit inside its own hull's box, so a ball's query takes the nearest hit not its own (not a plain linecast); the lead needs each ship's velocity, kept by `Broadside` from its position (an anchored ship has no `ShipController`); `Instantiate` puts a respawned copy in the active scene, so it is moved to the spawner's; effects hear launches from `Cannonballs`, not from each ship. Corrected in place.
