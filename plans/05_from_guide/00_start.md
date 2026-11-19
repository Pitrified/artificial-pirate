---
status: done
priority: 0
description: |
  Bring the guide's work since the split into the game: diagnostics report, asset pipeline,
  water (waves, shore foam, ripples, buoyancy), and the tooling fixes; then put the ship on the
  new water.
---

# From the guide

Asked 2026-11-18 by the user: "Bring the guide's new systems into artificial-pirate".

## Where the two repos stand

This repo was copied from unity-setup-project at `8b521a7` ([`01_bootstrap`](../01_bootstrap/00_start.md)); [`f6c487e`](https://github.com/Pitrified/artificial-pirate/commit/f6c487e) and `576f6e5` took the template fixes (gates, hooks, setup and build scripts).
The guide has 38 commits since (`8b521a7..0716d6d`), in its plans `02_guide_rebuild` to `06_load_and_preview`. Compared file by file (2026-11-18):

- **Only in the guide, to port**:
  - Diagnostics: `Systems/Diagnostics/*` (recorder, report, export, histogram, problem log, build info), `UI/DiagnosticsPanel.cs`, `UI/Diagnostics.uxml` and `.uss`, the monospace font, `Core/AllocationScan.cs`, `Editor/BuildLinuxDevPlayer.cs`, `Editor/ProfileCaptureReport.cs`, `SO_BuildInfo.asset`, tests (`DiagnosticsTests`, `DiagnosticsReportTests`, `DiagnosticsPanelTests`, `AllocationTests`).
  - Water: `Gameplay/Water/*` (`WaveField`, `WaterSurface`, `SO_WaterSettings`, `Buoyancy`, `WaterRipples`, `RippleSource`, `CannonballLauncher`), `Water.shader`, `WaterRippleSim.shader`, the depth and normal textures, `Editor/WaterDepthBake.cs`, the water grid model, tests (`WaveFieldTests`, `BuoyancyTests`, `FloatingPropsTests`, `WaterRippleTests`, `SequenceCaptureTests`), `docs/systems/water.md`.
  - Asset pipeline: `Tools/asset_builders/` (Blender builders, probe, measure), `.claude/skills/build-asset/`, `Editor/ModelSetup.cs`, `MaterialTuning.cs`, `SceneCapture.cs`, `docs/building_assets.md`, the barrel, crate and island models and materials, `BuiltModelTests`.
  - Docs: the headless pitfalls, `logging.md`'s "Diagnostics report", the guide-rebuild doc changes that apply to a game.
- **Changed in both**: `GameBootstrap`, `GameManager`, `GameSceneWiring`, `MenuController`, `BuildPipeline`, `CameraController`, `GameSaveData`, `SO_CameraTuning`, `Log`, `ScreenId`, `UISystem` (2 to 67 lines each). Mostly the guide's diagnostics hooks and the rebuild's renames from ship to avatar; the port takes the hooks and keeps the ship.
- **Guide-only, not ported**: the avatar sample (`AvatarController`, `AvatarState`, `SO_AvatarTuning`, `PF_Avatar`, its tests), the green blobs (`PF_Blob`, `M_Blob`).
- **Replaced here**: `SG_Water.shader` (a stylised flat sea, 113 lines) gives way to the guide's `Water.shader`; the `Sea` object to the guide's `Water` object. `PF_Island` (two primitives) to the guide's island model, if Q2 says so.

The functional spec's water row ("Custom URP shader (stylized)", "Avoid simulation cost") still holds: the guide's water is a custom shader with a 256x256 ripple step, measured at 60 fps on the Pixel 7 Pro. Its out-of-scope "Realistic water physics" is untouched by Q1's recommendation.

## Questions

### Q1 - How the ship meets the water

The ship is kinematic (`ShipController` moves the Transform, `docs/systems/ship-controller.md`, the spec's stack); the guide's floaters are Rigidbodies with `Buoyancy`.

- **A (recommended): the ship rides the waves kinematically.** Movement stays as it is (thrust and yaw on the plane, the tests unchanged). Each frame the hull's height, pitch and roll follow the water: the `WaveField` sampled at bow, stern, port and starboard, smoothed so a short wave does not shake it. A `RippleSource` on the hull leaves rings as it moves. No physics cost, deterministic, the same as now for the camera and saves.
  Limit: the ship does not push floating props or get pushed by them; they bounce off it only if it gets a kinematic collider (cheap, one `Rigidbody` with `isKinematic`), which this plan would add.
- **B: the ship becomes a Rigidbody with `Buoyancy`**, thrust and rudder as forces. The ship rolls and pitches from real buoyancy and shoves props, but `ShipController` and its tests are rewritten, tuning becomes physics tuning, and the spec's kinematic choice changes.

### Q2 - The guide's sample content

The guide's Game scene has an island model (sand and grass), floating barrels and crates, a cannonball launcher lobbing balls at them, and the blobs.

- **Recommended: the water, the island model and a few barrels and crates as floating set dressing**; the island replaces `PF_Island` (the depth bake needs real terrain to make the shore foam). Not the automatic cannonball launcher (combat is the game's own later work, and out of scope in the spec for now) and not the blobs.
- Or: only the systems, no sample content; the shore foam then needs the island rebaked against `PF_Island`'s primitives.

### Q3 - Phases

Proposed, each closing on the gates and a commit:

1. **Diagnostics**: the report, export, panel, buttons on the menu and pause screen, the allocation scan and capture reader. No gameplay change; the dev APK to the phone carries it.
2. **Asset pipeline**: `Tools/asset_builders`, the skill, the editor tools and docs; the models it builds (island, barrel, crate, water grid).
3. **Water**: shaders, scripts, settings, the `Water` object in place of `Sea`, the depth bake against the island, ripples; the guide's water tests.
4. **The ship on the water**: Q1's answer; floating props per Q2; a short capture sequence of the ship sailing.
5. **Verify**: fresh clone green, dev APK, HUMAN on the phone: 60 fps held, a diagnostics report pasted here.

## Decisions

Answered 2026-11-18 by the user: "Q1 a / The rest as REC".

- **Q1: A.** The ship stays kinematic and rides the waves: height, pitch and roll from the `WaveField` at four hull points, smoothed; a `RippleSource` on the hull; a kinematic collider so floating props bounce off it.
- **Q2: the island model and a few barrels and crates**; no cannonball launcher, no blobs.
- **Q3: the five phases** as proposed.
