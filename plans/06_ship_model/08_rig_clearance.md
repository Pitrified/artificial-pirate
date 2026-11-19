---
status: done
---

# Phase 08 - Rigging clear of the sails

Asked 2026-11-19 by the user: "rigging intersects sails sometimes. How could we fix that? Can rig lines and cloth actually interact or is that overkill?"
Then: "do it as p8": route the lines, let the cloth give way to the rope, add a check.
Not now: runtime cloth for a hero sail, nor a vertex shader push (that comes with dynamic sails).

## Plan

All at build time, so it costs nothing in the game.

1. **The check.** The builder keeps every rope it lays as a segment. After a sail state is built, each rope is tested against that state's sail and furl triangles; a crossing is reported by rope and sail (`build.py` prints them). A rope that a sail hangs on or is sheeted by is not a crossing where it meets that sail. Run on both ships, both states: how many, and which kind.
2. **Route the lines as a ship does**, by what the check finds. Likely:
   - lifts and braces led clear of the sail below, behind it;
   - the lee backstay slack or left off where the boom swings out;
   - braces and boom stopped where the lee shrouds would stop them.
3. **The cloth gives way.** Where a rope still grazes a set sail, the cloth near it is pushed to one side of the rope, fading over about a metre, as a sail pressed on a shroud.
4. The check reports none left, or each one left is named and explained. Probe screenshots before and after; triangles unchanged or close; the FBX rebuilt; gates; a commit.

## What the implementation found
- 2026-11-19. **The check** (`builders/ship/clearance.py`):
  - The rigging records each rope it lays (kind, ends, thickness); each sail and furl records its faces.
  - A rope's core and both its faces are tested against the cloth's triangles, away from its ends (0.6 m: a sheet or a brace made fast at a corner).
  - A rope a sail hangs on or is sheeted by belongs to it and is skipped for it.
  - `build.py <ship>` prints it per state. It takes well under a second.
- **First count**: sloop 6 sailing and 3 anchored; brig 9 and 22. Almost all were routing mistakes:
  - **Yards inside the mast.** They hung 0.3 m before its centre; the brig's mast has a 0.4 m radius. Braced, the yard crossed the lee shrouds. Now a yard hangs a mast radius plus a spar radius forward.
  - **Lifts.** The lower yards' lifts ran almost flat to the top, the highest yards' lifts ran down to the crosstrees. Now a lift rises half the yard's half-length to the mast.
  - **Braces** on the mainmast led aft through the gaff sail. On a brig they lead forward to the foremast ([Steel, snow-brig](https://navalmarinearchive.com/research/docs/snow_brig_steel.html)); now each mast's braces lead to the mast ahead, and the foremost mast's lead aft to the deck. Leading them forward to the deck instead crossed the square sails below.
  - **Backstays** on a mast with a boom are running backstays, in the sail states: the lee one is let go so the boom can swing out.
  - **Jib stays** were made fast below the yard of the sail behind them, through it. Now they are made fast above it, high enough to clear its head and its furl: sloop outer 28.0; brig 27.2, 31.6 and 34.5. The next sail's foot is cut hollow, so the stay passes under it.
  - **The forestay** doubled the stay of the jib tacked at the stem; that jib sets on it, so it is drawn only when no jib does.
  - **The outer jib's sheet** led inboard through the middle jib. Jib sheets now lead no further forward than the foremast.
  - **Furls.** Square furls rest on top of and before their yard, clear of the mast and the topmast shrouds. The jibs' furls overlapped on the bowsprit, and one had no length (its tack at the bowsprit's start); now each jib has its own stretch of it. Hauled down on its stay, a jib read as a fat diagonal sausage, so that was dropped.
- **The sloop's gaff sail and its lee braces.** The braces run from yardarms outboard of the sail to the rail inboard of it, so they cross its whole plane, and no dent can clear them. On a real ship the boom swings out only as far as the lee rigging lets it. The build now brings the boom in a degree at a time from the wind's 25° until no rope passes through a gaff sail: the sloop's settles at 14°, the brig's stays at 25°.
- **The cloth giving way** was built: free cloth pressed to the side of a rope most of it is on, held within a grid cell, easing over the next, with the edges on spars and stays pinned.
  - On the 6 x 6 grids (2.7 m cells on the sloop's mainsail) it could not clear the long crossings.
  - Once the routing was right it changed nothing: with it switched off the check still read 0 everywhere. So it was taken out.
  - It belongs with dynamic sails, as the planned vertex-shader push, and the check will show if a later wind needs it.
- **Result**: 0 ropes through sails, both ships, both states. Captures before and after: the brig sailing and anchored, the sloop sailing and anchored.
- **Cost**: triangles about the same: sloop 3,782 / 916 / 1,310, brig 5,660 / 2,018 / 2,962 (hull, sailing, anchored), from 3,800 / 910 / 1,254 and 5,678 / 2,012 / 2,994. No new slot.
- **Gates**: compile 0 warnings, EditMode 146/146, PlayMode 19/19. No new APK: the change is rope routing and a few degrees of boom, the same triangles and slots.
- 2026-11-19, **the phone report** (Pixel 7 Pro, build `dev 4c1626f` of 16:54:45Z, this phase and the furls):
  - 117.4 s, about 111 s of it playing (4.2 to 115.3 s).
  - Frame time 16.8 ms at p05, median and p95, the same over the last 60 s. **60 fps held.** Three frames of 33 ms in play (75.4, 103.7, 106.7 s); the 300 ms and 133 ms frames are the scene load and the pause.
  - Memory 147.5 MB, as before. Heap 9.7 to 13.0 MB. `gc alloc kB` 0.1 a frame at p05, median and p95. 8 collections.
  - No problems.
  - The user: "Works very well".
