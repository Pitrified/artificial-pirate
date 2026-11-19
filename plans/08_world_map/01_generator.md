---
status: done
---

# Phase 01 - The generator

Seed, cells, islands and the height function, in plain C#, with tests and a look at a few seeds.

## What the implementation found
- 2026-11-19. In `Scripts/Gameplay/World/`:
  - `WorldHash`: a seeded integer hash (Murmur3's finaliser), value noise and its fractal sum. Our own, so a world is the same on every platform and runtime.
  - `SO_WorldSettings`;
  - `CellCoord` and `IslandSite`;
  - `WorldGenerator`.
- **Cells** are centred on the origin, so the start island's cell (0, 0) spans -200 to 200 m and the island is not cut by a corner.
- **Each draw has its own stream** (presence, radius, peak, offset x and z, the island's seed), so a later draw moves none of the others.
- **Fitting islands in their cells.** The reach is the radius with its wobble, plus a seabed slope of a fixed 25 m. With the slope a share of the radius instead, the largest islands (140 m) would not fit a 400 m cell; at 25 m they fit exactly.
- **The start island**:
  - radius 26 m, peak 8 m, a fixed seed;
  - at the floating props (38 to 43 m out) the water is 2.5 to 5 m deep;
  - the sloop's start and the brig are on the open seabed.
- **A look** at seeds 1 and 2 over 4 km, and the start island over 200 m, through a throwaway editor dump (deleted): scattered blob islands, about half the cells. The coastlines read too round at a wobble of 0.15, so it is 0.25.
- A compile error on the way: a constant stream x salt overflowed at compile time; the multiplication is now `unchecked` in one helper.
- `docs/systems/world.md` is new.
- **Gates**: compile 0 warnings, EditMode 163/163 (7 new in `WorldGeneratorTests`, one with 3 seeds), PlayMode 19/19.
