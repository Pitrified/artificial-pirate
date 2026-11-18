---
status: done
---

# Phase 02 - Asset pipeline

## Plan

- Copy `Tools/asset_builders/` (builders, probe, measure, references and credits), `.claude/skills/build-asset/`, `Editor/ModelSetup.cs`, `MaterialTuning.cs`, `SceneCapture.cs`, `docs/building_assets.md`.
- The island, barrel, crate and water grid models and their materials and prefabs, built here by the pipeline (deterministic FBX), so the build is checked as well as the files.
- `BuiltModelTests`.

## Done when

- Gates green; the builders reproduce the committed models byte for byte; a capture of the props.

## What the implementation found

- 2026-11-18. From the guide at `0716d6d`: `Tools/asset_builders/` (its tracked files, `git archive`), `.claude/skills/build-asset/`, `docs/building_assets.md` (linked from `docs/README.md`), `MaterialTuning`, `SceneCapture`, `BuiltModelTests`; the barrel, crate and island models with their `.meta` files and materials (`M_Iron`, `M_Wood`, `M_Cork`, `M_CrateWood`, `M_Sand`, `M_Grass`, URP Lit only), `PF_Barrel`, `PF_Crate`. The output folder in `build.py`, `textures.py` and the doc points at `ArtificialPirate/`; `.venv/` and `__pycache__/` ignored.
- `uv sync`, then `build.py barrel`, `crate`, `island` here: **each FBX byte for byte the guide's**, so the pipeline works in this repo and the committed models are its output.
- The water grid model and the normal texture come with the water, in phase 03; so does a capture with the props in the scene.
- `headless-unity.md`'s capture gate names `SceneCapture`.
- Gates: compile 0 warnings, EditMode 128/128, PlayMode 6/6.
