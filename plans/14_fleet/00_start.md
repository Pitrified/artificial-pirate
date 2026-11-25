---
status: done
priority: 0
description: |
  The fleet grows and looks the part: gun carriages in place of boxes, sails trimmed to a wind as the
  ship turns, chase and stern guns on bigger ships, a frigate, and a giant three-master with the
  Amerigo Vespucci's size and look.
---

# The fleet

## The idea

The user, 2026-11-24, picking from the spec's Next list:

1. Sails that follow the heading.
2. More ship types.
3. Chase and stern guns, on bigger ships.
4. A giant ship: a three-masted galleon the size of the Amerigo Vespucci.
5. Better cannons: from the helm view the base is a box.

## Where things stand

- **Cannons.** `props.cannons` puts a carriage per gun in the hull: one box, 0.8 by 1.0 by 0.4 m. Each barrel is its own object (`<Name>GunStarboard<i>`, `<Name>GunPort<i>`), a tapered eight-sided pole of 1.8 m turning about its trunnions, aimed by `Broadside`. From the helm the boxes are in plain view along both rails.
- **Sails.** The builder sets every sail for one wind, `sails.WIND_FROM_DEG` = 130 degrees off the bow (the starboard quarter): `Trim.for_wind` braces the yards, swings the boom and the jib sheets, bellies each sheet downwind, and leads the running rigging and the flag to match. Each mast has two states, `Mast<i>Sailing` and `Mast<i>Anchored` (furled), and the game shows one. The game has no wind: sailing is kinematic and the stick goes anywhere at the same speed. So a ship sailing a circle has its sails aback for half of it (`docs/systems/combat.md`).
- **Guns.** `Broadside` has two batteries, starboard and port, each bearing within `ArcDeg` (20 degrees) of its beam. The type's `Guns` are the starboard muzzles, the port ones mirrored, and its barrels are set starboard then port (`_barrels.Length == 2 * GunsPerSide`). Nothing fires ahead or astern.
- **Types.** Two: the sloop (one mast, gaff and jibs, 6 guns a side) and the brig (two masts, square rigged, 9 a side, 33.7 m, 16,450 triangles). A type is a parameter set (`builders/sloop.py`, `brig.py`) over a shared builder: masts in a list, square, gaff and jib sails, one flush deck with a sheer, one row of gun ports, a transom with windows. A new type that needs a part the builder lacks gets the part as a parameter (`docs/building_assets.md`). Each type has a prefab variant (`PF_Ship_Sloop`, `PF_Ship_Brig`), an `SO_ShipType` (handling, strength, guns, camera rigs, map icon) and references under `Tools/asset_builders/references/<type>/`.
- **The Amerigo Vespucci** (1931, the Italian navy's training ship), by its published particulars, to be checked in the phase: about 82 m on the hull and 101 m overall, 15.5 m beam, about 7 m draft, three masts ship-rigged, the main about 50 m over the water. That is about two and a half brigs long; the start clearing is 160 m round the origin (`docs/systems/world.md`), and the circling brig's circle is 90 m across.
- **Budgets** (`docs/performance-budget.md`): 150 k triangles and 80 draw calls on screen. Each barrel is a draw call per instance (the brig has 18); a big ship with two gun decks would have several times that.

## Proposal

- **Carriages.** A truck carriage per gun from the builder: two cheeks stepped down toward the breech, a front and a rear axle, four trucks, the barrel on the cheeks at its trunnions; the barrel itself with a breech knob and a ring or two. Still in the hull's mesh, one slot; a few hundred triangles a gun.
- **A wind and sails that follow it.** One wind per world, its bearing drawn from the world's seed (no save change), steady. The builder bakes each mast's sailing state for several trims, bands of the wind's angle off the bow (close-hauled, reaching, running, on each tack), and the game shows the band the ship's heading puts the wind in, with some hysteresis at the edges. Everything stays as the builder leads it: the braces, the sheets, the clearance check, the flag. The wind is only seen: the ship's speed does not depend on it (Q2).
- **Chase and stern guns.** `Broadside` takes batteries in place of two sides: each with its bearing off the bow (90, 270, 0, 180), its arc and its guns. The broadsides stay as now; the bow and stern batteries are new type data (their muzzles and arcs), fired through bow ports and stern ports.
- **Builder parts for bigger ships**, each a parameter: three masts ship-rigged (the list already takes any number), a raised quarterdeck and forecastle, a second row of gun ports, bow and stern ports for the chasers.
- **A middle type**, with those parts, between the brig and the giant (Q4).
- **The giant**, from the same parts at its size, with its own handling, strength, rigs and icon; where it sails is Q6.
- **The ship choice** in Settings (development builds) offers every type.

## Open questions

- Q1: The sails' trims at run time.
  - a. Baked bands: several sailing states per mast, one shown, swapped as the wind's angle crosses a band. The ropes, the clearance check and the flag stay right; a swap is a visible jump, and the meshes grow with the band count.
  - b. Turned at run time: the yards and square sails turned about the mast together, the fore-and-aft sails baked per tack. Smooth, but the braces and sheets would have to follow at run time or be dropped.
  - Recommended: a, five to seven bands, because it keeps everything the builder already checks, and a jump while turning is what a crew bracing round looks like at the game's distance; the phone decides whether the jumps read.
  - ANS: a; we test how bad it looks. The ropes are very important and look good now.
- Q2: What the wind does.
  - a. Only seen: the sails and the flag follow it; the speed is the same every way.
  - b. Sailed: the speed by the point of sail, slow close-hauled and none head to wind.
  - Recommended: a, because b changes how the game is played and how the other ships must sail (the spec's "ships that choose where to sail" is not built yet); b can follow on its own plan.
  - ANS: a; the sailed wind left as a note for later.
- Q3: Which types carry chase and stern guns.
  - a. The bigger ships only: the middle type and the giant.
  - b. The brig as well (brigs often carried bow chasers).
  - Recommended: a, as the spec says, so the sloop and the brig keep their broadside-only fights.
  - ANS: a.
- Q4: The middle type, or types.
  - a. A frigate: three masts ship-rigged, a quarterdeck and forecastle, one covered gun deck, about 45 m. It builds every part the giant needs at a size the game already handles.
  - b. A schooner: two masts, gaff rigged, about 30 m, fast. New to the fleet but needs no new part; the giant's parts would then first show on the giant.
  - c. Both.
  - Recommended: a, because it carries the parts toward the giant in a ship that still fits the start clearing and the circling helm.
  - ANS: a.
- Q5: The giant's look.
  - a. A galleon at the Vespucci's size: a high stern castle with galleries, a forecastle, a beakhead, a lateen mizzen, two gun decks.
  - b. The Vespucci's own look: a long black hull with two white bands of gun ports, a raised poop and forecastle, square sails on all three masts.
  - Recommended: a, because the spec and the roadmap note both say a galleon, the Vespucci giving its size; it needs two more parts (the castle's height, the lateen sail).
  - ANS: b, the Vespucci's own look.
- Q6: Who sails the giant, and where.
  - a. A pirate ship at anchor beyond the start clearing, in the Game scene from the start, and the player's in development builds through the ship choice.
  - b. The player's in development builds only, for now.
  - c. A pirate that sails a wide circle.
  - Recommended: a, because a giant at anchor is a set piece to sail up to and fight, and an anchor avoids its turning circle in a world of islands; a circle needs open water the world does not promise.
  - ANS: a. If a ship that big cannot get among the islands, maybe the islands are wrong: noted for the future.
- Q7: One plan or two.
  - a. One folder, six phases: carriages, the wind, chase guns, the builder's parts and the middle type, the giant, verify.
  - b. Two: the wind and the sails as their own folder, the ships and guns here.
  - Recommended: a, because the wind is a change to the builder that every type is rebuilt through; doing it before the new types builds them once.
  - ANS: a.

## Decisions

Folded in from Q1 to Q7, 2026-11-24.

- **Baked trim bands** (Q1): each mast's sailing state is baked for five to seven bands of the wind's angle off the bow, with the running rigging and the flag led for each; the game shows one. The ropes stay as the builder leads them. The jumps between bands are tested on the phone.
- **The wind is seen, not sailed** (Q2): one wind per world from its seed; the speed does not depend on it. The sailed wind goes to the spec's Next.
- **Chase and stern guns on the bigger ships only** (Q3): the frigate and the giant; the sloop and the brig keep their broadsides alone.
- **A frigate** (Q4) is the middle type: three masts ship-rigged, a quarterdeck and forecastle, about 45 m. It brings the builder's parts for bigger ships.
- **The giant has the Vespucci's own look** (Q5): a long black hull with two white bands at the gun ports, a raised poop and forecastle, square sails on all three masts and a spanker; no galleon castle or lateen. Its parts are the frigate's at a bigger size, with a second row of ports.
- **The giant is a pirate at anchor near the start**, in the start clearing widened for it, and the player's in development builds through the ship choice (Q6). Islands a ship that big cannot get among go to the spec's Next.
- **The islands move out for it** (the user, 2026-11-24): the start clearing is widened so the giant anchors in open water near the start; no anchorage is searched for in the world.
- **One folder, six phases** (Q7).

## Pitfalls

- **A giant's draw calls**: a barrel object per gun on two decks is several times the brig's 18. The lower deck's barrels may stay part of the hull, or its ports closed, with only the upper deck's turning.
- **A giant in the world**: about 7 m of draft and 100 m of length against islands that are generated; the start clearing is 160 m round the origin, and a grounding stops a ship dead.
- **Its camera rigs and bars**: the bars stand 9 m over the waterline, below the giant's rail; the chase and bird's eye distances, the map icon's length.
- **Baked trims and the triangles in memory**: five to seven sailing states per mast, of which one is drawn; the counts are measured before choosing the number.
- **Wind and the circling brig**: its trims now follow the wind, so the bands are seen changing as it circles; that is the check that the swaps read.

## Phases

| # | Phase | What |
| -- | -- | -- |
| 01 | Carriages | truck carriages and a better barrel, every type rebuilt |
| 02 | The wind | a wind per world; sailing states per band; shown by the heading; the flag |
| 03 | Chase and stern guns | batteries by bearing in `Broadside`; bow and stern ports and their muzzles in the builder |
| 04 | The frigate | the builder's parts for bigger ships; the middle type, its prefab, type and icon |
| 05 | The giant | the Vespucci's look at its size, the type, its place at anchor |
| 06 | Verify | the spec and pages; the phone |
