---
status: done
---

# Phase 11 - Pan and zoom the map

Asked 2026-11-20 by the user: "i expected a drag on the map to move it ... do it with pinch to zoom, with two buttons to a. recenter boat b. reset zoom".

## Plan

- The map opens at a set scale, `SO_MapStyle` 7 cells across, centred on the ship; when less has been explored, the whole of it, as now.
- One finger drags the map; two pinch it, zooming about their midpoint and following it. Zoom runs from the whole explored square to 2 cells across. The view stays inside the explored square.
- Buttons: Ship (centre on the ship, the zoom kept) and Reset zoom (back to the set scale, the centre kept), beside Close.
- `MapView` (`Game.UI`): the view's state and its math (pan, zoom about a point, clamp, the image's size and place in the frame), no Unity events, tested in EditMode. `MapPanel` routes the pointer events into it and lays the image out in a square viewport that clips.
- `IUISystem.ShowMap` takes the set view and the closest as shares of the map's side, so the UI still knows nothing of cells.
- Tests: `MapView` in EditMode (pan, pinch about a point, clamps, recentre, reset); `MapScreenTests` taps the two buttons.

## Known ceiling

- The picture is 48 px a cell (8 m a pixel), so 2 cells across the screen is soft. Sharper needs bigger tiles, painted at four times the cost.
- Past 21 cells explored the picture is the 21 around the ship, as now; the view stops at its edge.

## Done when

- Drag, pinch and both buttons work on the phone; the gates pass; a new dev APK.

## What the implementation found
- 2026-11-20. Done as planned. `MapView` keeps up to two pointers; when one of two lifts, the other drags on. `MapPanel` captures each pointer on the window, so a drag that leaves the window keeps going.
- The map's border went with the clip: a border on the window would shift the picture by its width.
- `IUISystem.ShowMap` takes the start and closest views as shares of the map's side; `SO_MapStyle` gains `ViewCells` (7) and `ClosestCells` (2), and the asset keeps the defaults.
- Tests: `MapViewTests` (9: open, edge clamp, drag, pinch about its midpoint, zoom limits, one finger lifted, the two buttons); `MapScreenTests` taps Ship and Reset zoom; `GameManagerTests` checks the start view's share.
- Gates: compile 0 warnings, EditMode 195/195, PlayMode 22/22. A new dev APK with phases 10 and 11: 74,172,603 bytes.
- Not checked here: multi-touch reaching UI Toolkit through the event system on the phone. The tests drive `MapView` directly.
