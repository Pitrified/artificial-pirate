---
status: done
priority: 0
description: |
  How the game looks from outside and from the deck: an app icon of a brig at sea, a minimap that
  shows boats (the player's and the other ships, an icon per kind of ship), three cameras (chase,
  bird's eye, helm) picked in a new settings screen, and a choice of the player's ship to test both.
---

# Views

Asked 2026-11-23 by the user, after `12_cannons` was closed:

> one new feat for cameras + minimap + icon
>
> 1. icon of the game should be a brig in the sea
> 2. minimap should show a boat not a triangle, and also the enemy ships - some icons . not the same icon for all ships
> 3. different cameras: chase , bird eye (a bit to the stern, quite high, looking at the bow, more or less), helm
>
> in the settings , add a button to change ships so i can test both brig and sloop views thanks

Worked on `feat/13_views`, cut from `feat/12_cannons` at `af3fd99` (not yet merged into `main`): the minimap's enemies are that branch's ships.

## What is there

- **No app icon.** `ProjectSettings` has no icons set, so the phone shows Unity's default.
- **The minimap** (`Minimap`, `docs/systems/world.md`): the 3 x 3 cells around the ship, a window two cells (800 m) across, north up; the ship is `MapArrow`, a white triangle made in code, turned with its heading. Other ships are not shown. The full map shows the same arrow.
- **One camera.** `CameraController` follows the ship from the type's `CameraOffset`, looking at `CameraLookAt` (the sloop's now (0, 18, -56) and (0, 9, 12)), smoothed, and shakes. The root carries the yaw only; the hull's pitch and roll are `WaveRider`'s on the `Hull` child.
- **No settings screen.** The screens are Menu, Pause, Map, GameOver, Diagnostics (`ScreenId`); Diagnostics is opened from the menu and the pause screen and closes back, the pattern a settings screen can follow. Volumes are kept in `PlayerPrefs` (`AudioManager`), with no screen for them.
- **One player ship.** The Game scene's `Ship` (the sloop) is the one `GameSceneWiring` wires and `GameManager` saves; the minimap's target, the respawns' player and the camera point at it. The save holds its place and damage, not its kind.

## Proposal

### Settings screen

A `Settings` screen (UI Toolkit, `ScreenId.Settings`), opened from the menu and the pause screen as Diagnostics is, closing back: the camera (Chase, Bird's eye, Helm) and the ship (Sloop, Brig), each a row of buttons with the chosen one lit. Kept in `PlayerPrefs`, as the volumes are, through a small `GameSettings` in `Game.Systems`.

### Cameras

- Each ship type holds a rig per camera (`SO_ShipType`: offset and look point in the ship's space), since a brig's helm is not a sloop's.
  - **Chase**: as now.
  - **Bird's eye**: high and a little astern, looking down toward the bow: for the sloop about (0, 45, -22) looking at (0, 0, 14).
  - **Helm**: an eye 1.7 m over the deck at the helm (the tiller or wheel, aft), looking ahead over the bow.
- `CameraController` takes the rig with the target; a switch eases from one rig to the next over half a second. The helm camera follows without the chase's lag.
- The shake stays in every camera.

### The player's ship

The Game scene keeps a player's ship of each kind (the sloop, a brig), both wired as `Ship` is now; `GameSceneWiring` turns on the chosen one when the scene loads and hands it to everything that points at the player's ship (the camera, the minimap, the respawns, the bars, the hull bar, the map). A change takes effect at the next New Game or Continue. The save keeps no kind: the damage and place go onto whichever ship sails (a broken mast's number beyond the ship's masts is ignored, as now).

### Minimap icons

- Each ship type has a map icon: its silhouette from above (the hull's plan from the builder's half-beam stations, the bowsprit, a dot per mast), made by `textures.py` from the builder's numbers, white on clear. The sloop's and the brig's differ in shape and length; drawn to scale against each other.
- The minimap shows the player's ship with its type's icon, and every other ship within the window with its own type's icon, tinted by side (enemy red, ally green), turned with its heading. A fixed pool of icons; the ships found as the bars find them (the ships' layer around the player's ship).
- The full map shows the player's icon in place of the arrow.

### App icon

A picture of a brig at sea from the game itself: a still from the Game scene (the brig under sail, the sea, the sky), cropped square, set as the app's icon on Android: the legacy icon, the round one, and the adaptive icon's background, its foreground clear, with the brig inside the adaptive icon's safe circle (the middle 66 %).

## Phases, proposed

| # | Phase | What |
| -- | -- | -- |
| 01 | Settings and cameras | `GameSettings`; the Settings screen from menu and pause; the three rigs per type; switching in `CameraController`; tests, captures of each camera on each ship |
| 02 | The player's ship | both player ships in the scene, the choice applied at load to everything that follows the player's ship; tests |
| 03 | Map icons | the icons by `textures.py`; the minimap's ships; the full map's icon; tests, a capture |
| 04 | App icon | the still, the crops, the Android icons; a dev APK |
| 05 | Verify | the spec (Screens: Settings; Controls: the camera; Scope; a decision row for settings in `PlayerPrefs`), the system pages; the phone |

## Open questions

- Q1: Where the camera and ship choices live.
  - a. A new Settings screen, opened from the menu and from the pause screen.
  - b. Buttons on the pause screen.
  - Recommended: a, because settings will grow (the volumes have no screen yet) and the pattern of Diagnostics is there to follow.
  - ANS: a.
- Q2: The ship choice.
  - a. In development builds only: a tester's switch; a release sails the sloop.
  - b. In every build.
  - Recommended: a, because it is for testing both views, and the game's progression (a bigger ship) is not decided.
  - ANS: a.
- Q3: Switching cameras while sailing.
  - a. In the settings and with a camera button on the HUD that cycles through the three.
  - b. In the settings only.
  - Recommended: a, because comparing views mid-fight is the point, and a pause to change a camera breaks it.
  - ANS: a.
- Q4: The helm camera's motion.
  - a. Rides the hull: it pitches and rolls with the waves and the list, as an eye on deck does.
  - b. Steady: follows the heading only, the horizon level.
  - Recommended: b, because a rolling camera on a phone tires the eyes, and the waves still show in the bow rising against the horizon; a can be tried on the phone with a field.
  - ANS: both, to try: the helm camera steady or riding the hull, a choice in the settings.
- Q5: The icons per ship.
  - a. Each type's silhouette from above, made from the builder's numbers, tinted by side (the player's white, enemies red, allies green).
  - b. A simple glyph per type drawn by hand in code.
  - Recommended: a, because the shape then says which kind of ship it is, and a new type gets its icon from its own numbers.
  - ANS: a, and super small: the minimap will be crowded, so no detail; the shape and size of each type is enough.
- Q6: Other ships on the full map.
  - a. On the minimap only; the full map shows the player's icon.
  - b. On both.
  - Recommended: a, because the minimap is where a fight is read, and the full map is the explored world; the full map's screen takes only a picture and the player's place now, and markers would widen it.
  - ANS: b, on both maps.
- Q7: The app icon's picture.
  - a. A still from the game: the brig under sail on the game's own sea.
  - b. A render by the ship builder (Blender, studio light, a painted sky).
  - Recommended: a, because it looks like the game a player then opens, with no second look to keep in step.
  - ANS: to be assessed: take some stills from the game and see, or hand-draw a flat SVG; the user picks.

## Decisions

Folded in from Q1 to Q7, 2026-11-23.

- **A Settings screen** (Q1), from the menu and the pause screen, closing back, its choices in `PlayerPrefs` through `GameSettings`: the camera (Chase, Bird's eye, Helm), the helm's motion (Steady, Riding: Q4, both to try), and in development builds the ship (Sloop, Brig: Q2).
- **A camera button on the HUD** (Q3) cycles the three cameras, and the settings remember the last.
- **Map icons, small** (Q5): a type's silhouette from above, its hull's plan and nothing else, white on clear, a few dozen pixels; tinted by side when drawn (the player's white, enemies red, allies green). Sloop and brig differ by shape and length.
- **Ships on both maps** (Q6): the minimap shows every ship in its window; the full map every ship in its picture, the moment it opens. `IUISystem.ShowMap` takes the ships as markers (where, heading, icon, tint) in place of the player's place alone.
- **The app icon is chosen by the user** (Q7): phase 04 makes candidates, stills from the game and a flat SVG drawn by hand, and the user picks before it is set.

## Phases

| # | Phase | What |
| -- | -- | -- |
| 01 | Settings and cameras | `GameSettings`; the Settings screen; three rigs per type, the helm steady or riding; switching in `CameraController`; the HUD's camera button |
| 02 | The player's ship | both player ships in the scene, the choice applied at load to everything that follows the player's ship |
| 03 | Map icons | the icons by `textures.py`; the minimap's ships; the full map's markers |
| 04 | App icon | candidates (stills, a flat SVG), the user's pick, the Android icons |
| 05 | Verify | the spec and the system pages; the phone |

## The phone read

2026-11-23, the user on a Pixel 7 Pro: the icon, the three cameras, both ships, the minimap and the full map work.
The helm view is sickening in both motions (the boat moving under a level view, or the world moving around a still boat); might suit a milder sea.
Sinking in the helm view breaks the view.
Diagnostics: GPU p50 10.3 ms, p95 13.3, mean 10.2 (the cannons run: 11.8, 12.8, 11.7); 6 slow frames in 180 s.

Why the sinking breaks the helm view: `WaveRider` carries the hull down and tips it bow or stern down (`ShipDamage.SinkDepth`, `SinkPitchDeg`) while the ship's root stays at the surface.
A riding helm is measured from the hull, so it goes under and tips with it; a steady helm is measured from the root, so it hangs at eye height over the water while the deck drops away beneath it.

- Q8: The helm view while the player's ship sinks.
  - a. Ease to the chase view once the player's ship is sinking, the setting left as it is (a new game starts in the chosen view again).
  - b. Keep the helm, held at the surface and turned to look down at the hull as it goes.
  - Recommended: a, because the chase already frames a sinking ship well and it reuses the views' ease; b is a fourth pose to tune for a few seconds.
  - ANS: a.
- Q9: The helm's motion at sea.
  - a. Keep both motions as they are; add "the helm view in a milder sea" to Next, for when the sea can change.
  - b. A third motion, riding a share of the hull's pitch and roll (a third, say), to try on the phone.
  - c. Drop the helm view.
  - Recommended: a, because both ends were rough, so a share between them is likely rough too, and the sea's height is the real cause.
  - ANS: a.

Folded in from Q8 and Q9, 2026-11-23.

- **A sinking player's ship is seen from the chase** (Q8): the scene's wiring shows the chase once the player's ship is sinking, whatever the setting, which stays as chosen. Phase 06.
- **The helm stays as it is** (Q9): both motions kept; "the helm view in a milder sea" joins the spec's Next.
