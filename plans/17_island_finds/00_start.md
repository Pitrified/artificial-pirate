---
status: draft
priority: 0
description: |
  Things to find on islands, starting with a small village and a dock for about five brigs,
  and the world scale that has to come first: how big things are, how dense islands are, how far apart.
comment: |
  Crosses the spec's "Next" item "islands made for ships the giant's size": a village needs a larger island.
---

# Island finds

## The idea

The user's notes, 2026-11-26, picking "things to find on islands" from the spec's Next list:

- "we do this, which might cross over `Islands made for ships the giant's size`, as a village might need some larger island."
- "one prerequisite question is how far we want to push natural dimensions and proportions. a town is 500m? a dock is another 200? i have no ideas on actual sizes. let's start with a small village and a dock that can hold like 5 brigs."
- "how dense should islands be? in reality we have archipelagos with several settlements probably, but then also stretches of sea. so they are fundamentally different than our grid approach."
- "in these large oceans, it is not fun to just sail ahead at ship speed for months, in a game. so. some distances should be tweaked."

This changes what the game is: the world generation (`docs/systems/world.md`), the spec's scope (two Next items move in), the save's version only if its schema changes.

## Where things stand

Read from the code and the settings assets on 2026-11-26.

### The world

- A grid of 400 m cells (`SO_WorldSettings.asset`, `_cellSize`); each cell holds an island with chance 0.5, its centre anywhere in the cell.
  Every draw comes from the seed and the cell, so a cell is the same whenever it is rebuilt.
- Islands: radius 30 to 110 m at the waterline, peak 4 to 40 m, stretched up to 2x, up to two lobes, a 25 m shelf down to a -20 m seabed.
  An island's bounds stay within one cell of its centre; neighbours can meet and merge (`HeightAt` takes the highest of the 3x3 cells).
- No island within 340 m of the origin; the start island there is 24 m in radius.
- Streaming loads the 5x5 cells around the ship (about 2 km square, 10 to 16 islands), one island built per frame.
- The camera's far plane is 1000 m, the fog runs from 400 to 900 m: an island is seen from about 900 m.
- One wind bearing for the whole world; it trims sails, it does not change speed.
- Nothing stands on any island: no trees, buildings or piers.

### Ships and time

| Type    | Length | Draft | Top speed | One 400 m cell |
| ------- | ------ | ----- | --------- | -------------- |
| Sloop   | 20 m   | 2.4 m | 8 m/s     | 50 s           |
| Brig    | 34 m   | 3.4 m | 7 m/s     | 57 s           |
| Frigate | 44 m   | 5.4 m | 7.5 m/s   | 53 s           |
| Giant   | 83 m   | 7 m   | 6.5 m/s   | 62 s           |

7 m/s is about 13.6 knots, already about twice a real brig's cruising speed.
Gun range is 90 m.
From one island to the next today is about 400 to 800 m, a minute or two, with an island almost always inside the 900 m of sight.

### Budget

`docs/performance-budget.md`: draw calls at most 80, triangles on screen at most 150k (about 148k measured with both giants in view held 60 fps).
An island is about 2 draws; the giant alone is 49.7k triangles shown.
A village has to be a few draws and a few thousand triangles, not tens.

## Research

Two passes on 2026-11-26; numbers with a source are marked so, the rest are estimates.

### Real sizes

- **A small settlement is small.** Ferryland, Newfoundland, about 100 people in 1625; its mansion house 4.5 x 13.3 m ([Wikipedia](https://en.wikipedia.org/wiki/Ferryland), [heritage.nf.ca](https://www.heritage.nf.ca/articles/exploration/calvert-mansion-house.php)).
  Nassau about 1713: about 100 residents, cabins of pine and palmetto, storehouses and two or three taverns on the shore, no real wharves, ships careened on the beach ([reconstruction notes](https://bobmarshall.co.uk/image/125/Nassau-1715-Digital-Reconstructions)).
- **A large port town** for scale: Port Royal 1692, about 21 ha (about 460 x 460 m), about 2000 buildings, 6500 to 8000 people ([ICOMOS](https://www.icomos.org/public/risk/2006/17hamilton2006an.pdf)).
  Its excavated houses: a brick one 16 x 14 m, a timber one 11.6 x 8.2 m ([jamaicaportroyal.com](https://jamaicaportroyal.com/archaeology.html)).
  So the user's "a town is 500 m" is a real port town; a village is a tenth of that.
- **Streets** 2.4 to 2.8 m in an early 1700s Barcelona fishing quarter; Port Royal's only described as broad. Estimate 3 to 5 m for a village track.
- **Wharves.** Boston's Long Wharf (1710-21), 483 m long, up to 50 vessels alongside, unloaded straight into warehouses ([NPS](https://www.nps.gov/places/long-wharf-boston.htm)): about 10 m of wharf per ship one side, so ships were packed close, bow to stern and abreast.
  Small places often had a short jetty or none, ships anchored off and used boats.
  Construction, deck height and berth length per ship: not found.
- **Forts** come as gun counts, not footprints: Fort Montagu, Nassau, 4 guns; the Old Fort of Nassau, 22 to 26 guns in 1697; Fort Charles, Port Royal, 36 guns by 1667.
- **Archipelagos.** The Cyclades: about 220 islands in 2572 km2; Naxos to Paros about 20 km port to port ([Wikipedia](https://en.wikipedia.org/wiki/Cyclades), ferry sites).
  The Grenadines: legs of 8 to 15 nm between anchorages, none over 32 nm.
  The Exumas: 365 cays along 145 to 209 km.
  At a real 5 knots one hop is two to four hours.

### Other games

- **Sea of Thieves**: about 19 km2 (a fan estimate), crossed in 15 to 20 minutes; the designers sized it for a ship encounter every 15 to 30 minutes ([wccftech](https://wccftech.com/sea-of-thieves-map-size-linked-frequency)).
- **Assassin's Creed IV**: about 15 x 15 km, about 30 minutes to sail across, mostly water, with fast travel ([PC Gamer](https://pcgamer.com/assassins-creed-4-black-flags-huge-open-world-will-take-half-an-hour-to-sail-across)).
- **Skull and Bones**: about 625 km2.
- **Wind Waker**: a 7 x 7 grid, one island or cluster per square; the sailing was criticised as slow, and the HD version's fix was a sail that is faster and turns the wind to always blow behind ([Kotaku](https://kotaku.com/heres-why-sailing-took-so-damn-long-in-legend-of-zelda-513171965)).
- **Sunless Sea**: ships kept near real size (25 to 50 m), the geography "heavily scaled down" ([Failbetter](https://www.failbettergames.com/mapping-the-sunless-sea-geography-spoilers/)).
- **Port Royale 4**: a map of 4500 x 2700 km with about 60 towns, time compressed rather than space.
- **Raft**: islands spawn around the raft, one "less than 30 seconds away" (forum, unverified).
- Open-world density: a designer of Ghost of Tsushima put the spacing between "not every 10 seconds" and "not every three minutes" (from a search snippet, speaker unconfirmed).
- Not found: Sid Meier's Pirates! map scale, Atlas cell size, Windward sizes.

### What the research says, in one line each

- Every sailing game keeps the ships near real size and shrinks the gaps; none keeps true distances.
- The settlements stay near real size; the land around them shrinks.
- The common target is something new within a minute or two, with something on the horizon at all times; the open stretches are where a game puts its events or its speed-up.
- Our grid is Wind Waker's shape: one island per square, evenly spread, which is exactly the "no archipelagos, no open sea" the user noticed.

## Analysis

### Proportions: true size up close, compressed between

The player sees ships, piers and houses from 20 to 100 m, next to a 34 m brig; anything there that is not near true size reads wrong at once.
What the player does not measure is the distance between islands and an island's own size.
So the proposed rule: ships, buildings and piers at true size; islands and the gaps between them compressed.

The compression that falls out of the numbers:

- An archipelago hop of 20 km (Naxos to Paros) at 7 m/s and one to two minutes is 400 to 800 m: about 1:25 to 1:50.
- Today's islands (30 to 110 m in radius) are already about 1:20 to 1:50 of a small real one.
- A settlement on such an island takes a large share of it; that is the price of the rule, and the reason the giant's-size islands come into this folder.

### A village and a dock, sized

Estimates from the research, for a "small village":

- 8 to 12 houses of about 5 x 8 m, two or three larger buildings (a storehouse, a tavern) of 10 to 16 m, in a loose cluster of about 80 x 60 m behind the shore.
- Tracks 3 to 5 m wide, between the houses and down to the dock.
- Optionally a small battery of 4 to 6 guns, about 25 to 30 m across, on a point above the bay.
- A dock for five brigs: a brig is 34 m, so a berth with room to come alongside is about 40 m.
  - a. One pier straight out, berths on both faces: three and two, about 120 m long, 8 m wide. Compact, reads as a pier.
  - b. A quay along the shore, five in a row: about 200 m. Close to the user's "another 200", but it needs a long straight shore.
  - c. An L or T pier: about 80 m out and 80 m along. More shelter, more geometry.
- Water depth is not a problem at the pier's end: the brig draws 3.4 m and the shelf reaches -20 m within 25 m.
  The giant (7 m) would lie at anchor off, not alongside, which is also what a small place did with a big ship.
- The whole settlement, village and pier, is about 150 to 200 m along the shore.
  An island holding it needs a flat, low strip on a bay, on the side away from the wind (the world has one bearing, so "lee" is well defined), and a radius of about 150 to 250 m: past today's largest (110 m).

### Density: archipelagos and open sea

The current grid gives an even scatter. Ways to get clusters and open stretches, keeping every draw local to the seed and the cell so streaming still works:

- a. Keep the grid and tune the chance: still even, only sparser or denser.
- b. Modulate each cell's island chance (and size) with a low-frequency noise of the world position: clusters where the noise is high, open water where it is low.
  Cheap, smooth, but there is no unit to hang "one village per archipelago" on.
- c. A second, coarser grid of regions (say 2.4 km, 6 x 6 cells), each region drawn from the seed as open sea, an archipelago, or a settled archipelago; the region sets its cells' chance and sizes, and a settled one places one large island with the village.
  Counts are controllable (one settlement per settled region), still local, and the map can name regions later.
- With today's sight of 900 m: inside an archipelago the next island is always in view; in open sea nothing is, so an open stretch longer than about 1 km is sailed blind unless something is put there.

### Distances and long crossings

Targets proposed, at about 7 m/s (420 m a minute):

- Inside an archipelago: islands 300 to 600 m apart, 45 to 90 s.
- Open stretches between archipelagos: 1 to 2 km, 2.5 to 5 minutes, with the next archipelago's tallest island showing through the fog before the last one is lost, or an event in between (a wreck, a ship).
- A settlement: one per settled archipelago, so roughly one every 3 to 6 minutes of sailing.

The ways other games keep crossings short, none of them for this folder unless chosen:

- Shorter gaps (the compression above): free, and this folder's core.
- A speed-up with the wind astern: belongs to the spec's "a wind that is sailed".
- Fast travel between docks found: needs docks first, so a later folder.
- Events on open water: belongs to the ships that choose where to sail.

### What "find" means

Two readings, and the folder's size depends on which:

- Something seen and marked: the village shows from afar, enters the map with its own icon once seen; nothing to do there yet.
- Something used: sail into the dock and something happens (a repair, a save, later trade).
  An economy is in the spec's "Not planned" list; a repair or save is not.

## Phases, a first cut

To be set once the questions are answered.

1. Scale and density: the region grid (or the chosen density model), the gap targets, the settled island's shape (a flat lee strip, a bay, 150 to 250 m). Measured with the probe and a map capture.
2. The village and pier models: builders in `Tools/asset_builders/` as for the ship parts, a few draws, triangles counted.
3. Placement: the village on its island's lee strip, the pier into the bay, colliders, the map icon.
4. Verify: gates, the phone at 60 fps with a village and both giants in view, the user's read.

## Pitfalls

- A pier in the sea is something ships hit: it needs a collider, and the ship sailing a circle on its own will hit it unless villages keep out of its way.
- The 1.8k triangles per island and the 2 draws per island grow with larger islands and a village; the budget is measured on the phone, not on counts.
- Larger islands may push the mesh grid (56 x 56 per island) to show its facets at 250 m; the probe judges it.
- The 340 m start clearing and the anchored brig and giant near the start may need a village moved off them, or the start moved.
- A region grid changes every seed's world: saves hold the seed, so an old save loads into a different world.
  Not a problem before the first release (`docs/systems/save-system.md`, storage rules); the save's version changes only if its schema does.

## Open questions

- Q1: How strict is the proportion rule: true size for ships, buildings and piers, compressed islands and gaps?
  - a. Yes, as the analysis proposes.
  - b. Compress the settlement too (fewer, smaller houses, a shorter pier), to keep islands small.
  - c. Bring the gaps closer to real (several km), and add a speed-up to cross them.
  - Recommended: a, because it is what every sailing game in the research does and the player measures only what is next to the ship.
  - NEW_ANS:
- Q2: The village's size.
  - a. Small: 8 to 12 houses, a storehouse, a tavern, about 80 x 60 m.
  - b. Smaller: 4 to 6 houses and a storehouse, a fishing hamlet.
  - c. Larger: 20 to 30 houses, a church, a square, about 150 x 120 m, a small town.
  - Recommended: a, as you asked, with b and c as later variants from the same builder.
  - NEW_ANS:
- Q3: The dock's form, for five brigs.
  - a. One pier out from the shore, berths on both faces, about 120 x 8 m.
  - b. A quay along the shore, about 200 m.
  - c. An L or T pier, about 80 m out and 80 m along.
  - Recommended: a, the most compact, needs no long straight shore, and reads as a pier from the chase camera.
  - NEW_ANS:
- Q4: A battery above the village?
  - a. Yes, a small one of 4 to 6 guns, as scenery.
  - b. Yes, and it fires on ships, as the guns on ships do.
  - c. Not in this folder.
  - Recommended: c, a battery that fires is a fight design question; scenery alone can come with the village models if wanted.
  - NEW_ANS:
- Q5: The density model.
  - a. Tune the grid's chance only.
  - b. A low-frequency noise on each cell's chance and size.
  - c. A coarser grid of regions: open sea, archipelago, settled archipelago.
  - Recommended: c, because it gives open stretches and clusters and a place to put exactly one village per settled archipelago, and stays local to the seed for streaming.
  - NEW_ANS:
- Q6: The distance targets: islands 300 to 600 m apart in an archipelago, open stretches of 1 to 2 km, a village every 3 to 6 minutes of sailing?
  - a. Yes.
  - b. Shorter, everything about half.
  - c. Longer open stretches (3 km and more), with the speed-up from Q7.
  - Recommended: a, tuned on the phone after phase 1.
  - NEW_ANS:
- Q7: Anything to shorten long crossings in this folder?
  - a. No; the gaps are short enough, and the wind's speed-up stays with "a wind that is sailed", fast travel with a later folder once docks exist.
  - b. Fast travel between villages found, here.
  - c. A speed-up out of combat, here.
  - Recommended: a, keeps this folder to the world and the village.
  - NEW_ANS:
- Q8: What does finding a village do?
  - a. Seen and marked: it shows from afar and goes on the map with its icon once seen; nothing to do there yet.
  - b. As a, and the dock repairs the ship (and saves) when the ship lies alongside.
  - c. As b, with moored ships at the pier.
  - Recommended: a, so this folder is the world and the look; using the dock is its own feature, raised as a spin-off.
  - NEW_ANS:
- Q9: Where is the first village?
  - a. Only where the world generates one, in a settled archipelago.
  - b. Also one fixed near the start, for testing and for the first minutes of play.
  - Recommended: b, it gives the tests and the phone a known village to look at, the way the start clearing holds the anchored ships.
  - NEW_ANS:
- Q10: One folder or two?
  - a. One folder: the scale and density, then the village.
  - b. Two: this one for scale, density and the larger islands; a sibling for the village and dock models.
  - Recommended: a, about four phases, and the village is what tells whether the scale is right.
  - NEW_ANS:
