# Artificial Pirate

A 3D mobile game: you sail a ship on a stylized sea.
Unity `6000.4.4f1`, URP, Android ARM64, at v0.2: a vertical slice heading for a private alpha on Google Play.

## Layout

- `ArtificialPirate/` - the Unity project.
- `docs/` - the spec, the per-system specs, the project rules, build and release; start at [`docs/README.md`](docs/README.md).
- `plans/` - the work, one folder per feature (`python3 Tools/plans.py list`).
- `Tools/` - `setup.sh` once per machine, `build-android.sh dev|profile|release`.

## Running it

The gates every change closes on (compile with 0 warnings, EditMode and PlayMode tests, the `dev` APK) run in batch mode on a machine with no screen, as [`docs/headless-unity.md`](docs/headless-unity.md) describes.
At a workstation, open `ArtificialPirate/` in the Unity Hub at the version above and play from `Assets/Scenes/Boot.unity`.

Agents start from [`CLAUDE.md`](CLAUDE.md), which imports [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

## Where it came from

The game was built inside [unity-setup-project](https://github.com/Pitrified/unity-setup-project), which is now a guide to setting up a Unity mobile project.
It moved here without history at that repo's `8b521a7`; [`plans/01_bootstrap/`](plans/01_bootstrap/00_start.md) records what was copied and renamed.
The generic docs (the AI workflow, Unity MCP, a dev machine from zero) stay in the guide and are linked from here.
