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
| 01 | References               | [`01_references.md`](01_references.md)     | planned |
| 02 | Ship builder and sloop   | [`02_builder.md`](02_builder.md)           | planned |
| 03 | The sloop in the game    | [`03_sloop_in_game.md`](03_sloop_in_game.md) | planned |
| 04 | The brig                 | [`04_brig.md`](04_brig.md)                 | planned |
| 05 | Verify                   | [`05_verify.md`](05_verify.md)             | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-19 : asked by the user. Branch `feat/06_ship_model` from `main` at `0ca6e7a`. Questions Q1 (scale), Q2 (one builder), Q3 (what the model has), Q4 (type data in Unity), Q5 (first types), Q6 (look), Q7 (phases) written.
- 2026-11-19 : the user asked for clarification, no decisions yet. What scale costs went into Q1 and what 3 k triangles is into Q3. The user also said: Q2 and Q4 "keep tidy code" with no hacks; Q5 two ships to prove the generalisation early; Q6 ok; few code comments, none that list current contents.
- 2026-11-19 : "all recommended, plan them". Five phases planned.
