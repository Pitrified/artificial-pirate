---
status: done
---

# Phase 01 - Diagnostics

## Plan

- Copy the guide's diagnostics: `Systems/Diagnostics/*`, `UI/DiagnosticsPanel.cs`, `Diagnostics.uxml` and `.uss`, the monospace font and licence, `Core/AllocationScan.cs`, `Editor/BuildLinuxDevPlayer.cs`, `Editor/ProfileCaptureReport.cs`, `SO_BuildInfo.asset`, the tests.
- Port the guide's hooks in the scripts both changed (`GameManager`, `UISystem`, `ScreenId`, `MenuController`, `Log`, `BuildPipeline`, ...), keeping this repo's ship and names.
- Scene and settings changes (the recorder in `Persistent.unity`, the Diagnostics document, the menu and pause buttons) through an editor script in batch mode.
- Docs: `logging.md`'s "Diagnostics report", the headless pitfalls.

## Done when

- Gates green; dev APK built; a capture of the panel; a report read through `REPORT_OUT`.

## What the implementation found

- 2026-11-18. Copied from the guide at `0716d6d`, `.meta` files included so every GUID holds: `Systems/Diagnostics/*`, `DiagnosticsPanel`, `UI/AssemblyInfo.cs`, `Diagnostics.uxml` and `.uss`, the font and its licence, `AllocationScan`, `BuildLinuxDevPlayer`, `ProfileCaptureReport`, `SO_BuildInfo.asset`, `DiagnosticsTests`, `DiagnosticsReportTests`, `DiagnosticsPanelTests`; and, as the guide has them, `Log`, `ScreenId`, `UISystem`, `MenuController`, the two `AssemblyInfo` files, `LogTests`, `Game.Core.asmdef` (the scan uses the Input System).
- **`Persistent.unity` taken whole from the guide**: this repo's copy was byte for byte the guide's at the split, and the guide changed it since only in its two diagnostics commits (the rename commit moved it unchanged), so its current file is what Unity wrote for exactly this change.
- **`ModelSetup.cs` came one phase early**: `CommandLine`, which the two editor tools read their arguments with, lives in it.
- Adapted: `BuildPipeline` (the guide's build stamp, this repo's names), `SO_BuildInfo`'s menu name, `ArtificialPirate.x86_64`, the scan's avatar to the ship. `GameBootstrap`'s quality lookup fixed as in the guide ("mobile" did not match the project's "Mobile"). The Diagnostics buttons added to `Menu.uxml` (this repo's title kept) and `Pause.uxml`.
- Docs: `logging.md`'s "Diagnostics report", the guide's headless pitfalls.
- Not yet: `AllocationTests` (it also measures `WaveField`), with the water in phase 03.
- Gates: compile 0 warnings, EditMode 120/120, PlayMode 6/6. The panel capture reads "Artificial Pirate 0.1.0", `quality Mobile`, aligned columns; dev APK 73,112,246 bytes, git unchanged by the build.
