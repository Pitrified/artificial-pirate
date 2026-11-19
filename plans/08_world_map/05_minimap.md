---
status: done
---

# Phase 05 - The minimap

A minimap in the HUD's top right corner: north up, the cells around the ship, the unexplored ones dark, the ship's arrow turning with its heading.

## What the implementation found
- 2026-11-19. **Map tiles shared by both maps.**
  - `WorldMapTiles` paints a cell once into a 48 px tile coloured by height, and keeps it. Open sea shares one tile. An island's tile (9 KB) is allocated the first time it is painted: the one allocation in a frame here, once per island ever seen.
  - `WorldMap` (on `World`) composes any rectangle of cells, the unexplored ones dark, painting at most a budget of new tiles.
  - `SO_MapStyle` holds the colours and the tile size.
- **`Minimap`** (`Game.UI`):
  - the 3 x 3 cells around the ship in a 144 px texture, recomposed when the ship enters a cell or more is explored, one new tile a frame;
  - a window two cells wide, centred on the ship by the image's `uvRect`;
  - an arrow texture made in code, turned by the heading;
  - `Clicked` on a tap, for phase 06.
  
  The HUD is uGUI, so `Game.UI` now references `Game.Gameplay` and uGUI, and the EditMode tests reference uGUI.
- **The scene**, through throwaway editor scripts (deleted): `SO_MapStyle.asset`, a `WorldMap` on `World`, and the minimap on the HUD (220 px, 30 px from the top right corner).
  - Again a reference to the asset created in the same run was saved empty. A second run set it.
  - When an editor script creates an asset, set references to it in a separate run.
- **`CAPTURE_HUD`** in `SequenceCaptureTests` draws the HUD's overlay canvases through the capture camera, which a camera rendering to a texture would not otherwise show.
- **Capture**: the sloop under way with the HUD. The minimap shows the start island under the arrow and four islands around it on dark blue.
- **Gates**: compile 0 warnings, EditMode 179/179 (3 new in `WorldMapTests`), PlayMode 20/20.
