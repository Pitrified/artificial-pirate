# A ship model - implementation tracking

A ship model in place of the box hull, general enough for several ship types. Brainstorm and questions in [`00_start.md`](00_start.md).

## Key decisions

- **Real size**: a sloop of about 12 m. The camera offset and the wake's size go per type, and the waves get longer.
- **One builder**: `builders/ship.py` and a module of numbers per type, origin on the waterline amidships.
- **The model**:
  - hull, deck, bulwark, masts, spars, bowsprit, rudder and fixed sails;
  - no rigging, no guns, nothing moving;
  - at most 3 k triangles and 5 slots per ship.
- **`SO_ShipType`** holds a type's size, movement tuning and camera offset, in place of `SO_ShipTuning`; a prefab variant per type.
- **Two types**: the sloop for the player and the brig anchored in the bay.
- **Flat colours** in five shared slots, matched to a replica's photograph.
- Tidy code, no hacks; few comments, none listing current contents.

## Phases

| #  | Phase                    | Plan                                       | Status  |
| -- | ------------------------ | ------------------------------------------ | ------- |
| 01 | References               | [`01_references.md`](01_references.md)     | done    |
| 02 | Ship builder and sloop   | [`02_builder.md`](02_builder.md)           | done    |
| 03 | The sloop in the game    | [`03_sloop_in_game.md`](03_sloop_in_game.md) | done    |
| 04 | The brig                 | [`04_brig.md`](04_brig.md)                 | done    |
| 05 | Verify                   | [`05_verify.md`](05_verify.md)             | planned |
| 06 | Detail and look          | [`06_detail.md`](06_detail.md)             | done    |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-19 : asked by the user. Branch `feat/06_ship_model` from `main` at `0ca6e7a`. Questions Q1 (scale), Q2 (one builder), Q3 (what the model has), Q4 (type data in Unity), Q5 (first types), Q6 (look), Q7 (phases) written.
- 2026-11-19 : the user asked for clarification, no decisions yet. What scale costs went into Q1 and what 3 k triangles is into Q3. The user also said: Q2 and Q4 "keep tidy code" with no hacks; Q5 two ships to prove the generalisation early; Q6 ok; few code comments, none that list current contents.
- 2026-11-19 : "all recommended, plan them". Five phases planned.
- 2026-11-19 : phase 01 - Commons rate-limits a generic User-Agent; the user chose a descriptive one. Sizes: the sloop *Providence* (20.3 m) and the snow-brig *Niagara* (33.7 m), at real size.
- 2026-11-19 : phase 01 - references kept and credited: the *Providence* side-on (public domain) and under sail, the *Niagara* broadside under full sail (both CC BY 2.0). Numbers published and measured; the two "what right looks like" lists written. The *Niagara* has a flush deck, so phase 04's quarterdeck is not needed.
- 2026-11-19 : phase 02 - `ship.py` and `sloop.py`; the sloop reads as the Providence in one probe round; 1,358 triangles, 5 slots; `Sloop.fbx` identical on a rebuild.
- 2026-11-19 : phase 03 - `SO_ShipType` in place of `SO_ShipTuning`, `PF_Ship_Sloop` in the scene, longer waves, the camera framing the rig; the bow direction and the hull caps fixed after the first Unity checks. Gates green, dev APK.
- 2026-11-19 : phase 04 - `brig.py` after the Niagara; `brace_deg` added to the builder (square sails show from the side); `PF_Ship_Brig` with its own paint, anchored in the bay without the player's controller. 56.8 k triangles and 36 slots in view. Gates green, dev APK.
- 2026-11-19 : the user on the ships: correct but they read as toys. Phase 06 added: rigging and tops, sails for one wind and furled, sail sag and texture, hull detail, props, flags, shadows, a wet band; each benchmarked with a screenshot. Phase 05 (verify) runs after it.
- 2026-11-19 : phase 06 - sail trim and furling, sail sag, canvas, rigging and tops, hull detail, crew and cannons from a palette texture, a waving flag shader, the sun's shadows on decks and water, a wet band. Scene 64.7 k triangles and 48 slots in view. Gates green, dev APK; waiting on the phone.
- 2026-11-19 : phone report of the detail build: 60 fps held, 147.5 MB, 0.1 kB allocated a frame, no problems. The user: "Brillant results". Phase 06 closed.
