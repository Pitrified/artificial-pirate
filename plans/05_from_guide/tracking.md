# From the guide - implementation tracking

The guide's diagnostics, asset pipeline and water, brought into the game, and the ship put on the water. Brainstorm and questions in [`00_start.md`](00_start.md).

## Key decisions

- **Q1 A**: the ship stays kinematic and rides the waves; a kinematic collider for the props.
- **Q2**: the island model and a few floating barrels and crates; no launcher, no blobs.
- **Ported as the guide has it**, renamed to this project's folder and names; the ship stays where the guide has the avatar.

## Phases

| #  | Phase              | Plan                                   | Status  |
| -- | ------------------ | -------------------------------------- | ------- |
| 01 | Diagnostics        | [`01_diagnostics.md`](01_diagnostics.md) | done    |
| 02 | Asset pipeline     | [`02_assets.md`](02_assets.md)         | done    |
| 03 | Water              | [`03_water.md`](03_water.md)           | done    |
| 04 | Ship on the water  | [`04_ship.md`](04_ship.md)             | done    |
| 05 | Verify             | [`05_verify.md`](05_verify.md)         | in progress |
| 06 | The ship's wake    | [`06_wake.md`](06_wake.md)             | in progress |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-18 : asked by the user; the two repos compared file by file; Q1 (ship on the water), Q2 (sample content), Q3 (phases) written. Branch `feat/05_from_guide` from `main` at `576f6e5`.
- 2026-11-18 : Q1 A, the rest as recommended. Five phases.
- 2026-11-18 : phase 01 - diagnostics ported (the Persistent scene taken whole, unchanged here since the split); boot's quality lookup fixed. Gates green, dev APK.
- 2026-11-18 : phase 02 - asset pipeline ported; the builders reproduce the barrel, crate and island byte for byte here. Gates green.
- 2026-11-18 : phase 03 - water, island and floating props in the Game scene (carried from the guide as prefabs), the ship's start moved into the bay; the depth bake no longer takes floating bodies for seabed. Gates green, dev APK.
- 2026-11-19 : phase 04 - the ship rides the waves kinematically (`WaveRider`, a `Hull` child, a kinematic collider); a start splash fixed in `RippleSource`. Gates green, dev APK.
- 2026-11-19 : phase 05 - fresh clone green; the APK goes to the phone. Waiting on the device run.
- 2026-11-19 : phone report: 60 fps held, 147 MB, no per-frame allocation, no problems. The look not yet reported.
- 2026-11-19 : the look on the phone: the ship bobs right, the barrels are pushed by it; the wake is a chain of circles. Phase 06 added: the ripple square follows the ship, the hull stamps every step, a foam trail in the ripple field.
- 2026-11-19 : phase 06 - the capture sails the ship; the ripple square follows it; `WakeSource` replaces the hull's rings with a dent and foam every frame; foam channel in the ripple field. Gates green, dev APK. Waiting on the phone.
