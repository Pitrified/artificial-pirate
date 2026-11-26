# 📄 `docs/systems/save-system.md`

## Overview

Single-file JSON persistence for the player session. The file's schema is [below](#schema).

---

## Responsibilities

- Read/write `save.json` from `Application.persistentDataPath`
- Validate the caller's schema version
- Provide a clean `HasSave` check for the menu's Continue button

---

## Ownership & Lifetime

- Lives in Persistent scene.
- Owned by GameManager.

---

## Public interface

```
bool HasSave { get; }
T Load<T>(int version)           // new T() if no file, corrupt, or another version
void Save<T>(T payload, int version)  // atomic
void DeleteSave()                // for debug + future "new game" flow
```

The file is an envelope the save system owns, `{ version, lastSavedUtc, payload }`, around a payload type the game owns.
The save system knows nothing about the payload: the game passes its type and its schema version on every call.
In this project the payload is `Game.Gameplay.GameSaveData`.

---

## Storage rules

- File path: `Path.Combine(Application.persistentDataPath, "save.json")`.
- Atomic write: write to `save.json.tmp`, `File.Replace` to `save.json`.
- On corrupt read: rename to `save.json.bak`, log warning, return defaults.
- Version mismatch: same as corrupt (no migration).
- Until the first release, keeping old saves working is not a goal: a change to the world generation that turns a saved seed into a different world, or a schema change that resets the save, needs no migration and no compatibility work.

---

## Schema

Stored as `save.json` in `Application.persistentDataPath`:

```json
{
  "version": 4,
  "lastSavedUtc": "2026-04-28T12:00:00Z",
  "payload": {
    "ship": { "x": 0.0, "y": 0.0, "z": 0.0, "yawDeg": 0.0 },
    "world": { "seed": 1234567, "explored": [ { "x": 0, "z": 0 }, { "x": 1, "z": 0 } ] },
    "damage": { "lost": 0.25, "starboard": 0.2, "port": 0.05,
                "hits": [ { "x": 3.1, "y": 0.4, "z": 6.0, "amount": 0.2 } ], "brokenMasts": [ 0 ] }
  }
}
```

`version` and `lastSavedUtc` are written by the save system; `payload` is `GameSaveData` and `version` is its `CurrentVersion`.
This example is the one place the docs state the version; `Tools/spec_check.py` checks it against the code.
Version 1 had `ship` at the top level; version 2 had no `world`; version 3 had no `damage`. Any of them is reset.
`world.seed` 0 means no world was saved, and a new one begins ([world](world.md)).
`damage` is the player's hull: the health lost, in all and on each side, the latest hits in the hull's local space, oldest first, and the masts brought down by the builder's number ([ship-controller](ship-controller.md)); none means a whole hull.

---

## Serialization

- `System.Text.Json` if available; otherwise `JsonUtility`.
- No reflection-heavy options. No external JSON libraries.
- Floats serialized with invariant culture.

---

## Interactions

### Uses
- File system

### Used by
- GameManager (save on pause/quit, load on Continue)
- The payload type and its version are the game's (`GameSaveData` in `Game.Gameplay`, filled from the ship's state, its hull's damage and the world by GameManager; the schema is [above](#schema))

---

## Constraints

- No PlayerPrefs (except audio volumes via AudioManager).
- No async I/O: the file is small (a few KB), a sync write is fine.
- No encryption; the file is intentionally inspectable.

---

## Failure handling

- Disk full / write fails → log error, keep in-memory state, do not crash.
- Read fails → see "corrupt" rule above.
