---
status: done
---

# Phase 09 - The HUD only while sailing

Asked 2026-11-20 by the user, on the phone: the joystick covers the buttons that close the map and the diagnostics screen, and a tap on the open map reached the minimap.

## Plan

- Cause: the HUD is a uGUI overlay canvas sorted at 10, above the UI Toolkit screens at 0, so it draws over them and takes their taps.
- `GameSceneWiring` keeps the HUD object and shows it only in `Playing`, on `GameManager.StateChanged`. Turning it off also releases the joystick.
- The scene reference through a throwaway editor script.
- Test (PlayMode): the HUD is on while playing, off while paused, on again after resuming.

## Done when

- The HUD is hidden on every paused screen; the gates pass; a new dev APK.

## What the implementation found
- 2026-11-20. The HUD object goes off and on with the state, in `GameSceneWiring`; the scene reference set by a throwaway editor script (deleted).
- `PauseResumeTests.PauseResume_HudShownOnlyWhilePlaying`: on while playing, off while paused, on after resuming.
- Gates: compile 0 warnings, EditMode 186/186, PlayMode 22/22. A new dev APK: 74,158,439 bytes.
