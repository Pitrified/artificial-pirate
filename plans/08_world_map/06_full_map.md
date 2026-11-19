---
status: done
---

# Phase 06 - The full map

Tapping the minimap opens a map of everything explored, over the paused game, with the ship on it and a Close button.

## What the implementation found
- 2026-11-19. **`WorldMap.ComposeExplored`** draws the smallest square of cells holding every explored one. When they spread wider, it caps at 21 cells a side (1,008 px), centred on the ship. The picture is kept and resized with `Reinitialize`.
- **`GameManager.OpenMap(worldMap, ship)`**:
  - composes the map and places the ship on it;
  - disables input and moves to `Paused`, as the pause screen does;
  - `CloseMap`, on `IUISystem.OnMapCloseRequested`, hides the map and resumes.
  
  Returning to the menu hides the map before the pause screen, so the pause screen stays the last one hidden, as an existing test expects.
- **`UISystem`** has a `Map` document (`UI/Map.uxml`, `Map.uss`) bound to `MapPanel`:
  - the picture is kept square in its frame on every geometry change;
  - the ship's arrow is placed in percent and turned;
  - Close.
  
  `IUISystem.ShowMap` takes only a texture, the ship's place and heading, so the UI's screens know nothing of the world. The arrow texture is shared with the minimap (`MapArrow`).
- **`GameSceneWiring`** opens the map on `Minimap.Clicked`.
- **The scenes**, through a throwaway editor script (deleted): a `Map` UIDocument under `UIRoot` with the pause screen's panel settings, and the wiring's world map and minimap references.
- **Tests**:
  - `MapScreenTests` (PlayMode): a tap on the minimap opens the map over the paused game, and Close (a UI Toolkit submit on the button) resumes;
  - `GameManagerTests`: open, close, and open while not playing;
  - `WorldMapTests`: the fit and the cap.
- **Picture**:
  - UI Toolkit screens do not render through a camera, so the sequence capture cannot show the screen.
  - `CAPTURE_MAP` writes the map's picture instead. After two minutes of sailing, 12 cells were explored: islands with sand rims, grass and grey rock tops, the start island small in the middle, the unexplored row dark.
  - The screen itself is for the phone.
- **Gates**: compile 0 warnings, EditMode 182/182, PlayMode 21/21.
