# Bootstrap - implementation tracking

This repo written from unity-setup-project with the naval game and the runtime it needs. Scope and source commit in [`00_start.md`](00_start.md).

## Key decisions

- **Copied, not extracted.** `git archive` of one commit; no history carried over, the source keeps its own. `.meta` GUIDs are file content, so every asset reference survives.
- **Renamed once, during the copy**: folder, product name, Android id, `artificial-pi` names.
- **Settings change through Unity.** `ProjectSettings.asset` is edited by an editor method in batch mode, not by hand.
- **Plans numbered from 01**; the source's open roadmap moves in from 02.

## Phases

| #  | Phase                          | Plan                                       | Status  |
| -- | ------------------------------ | ------------------------------------------ | ------- |
| 01 | Copy the baseline              | [`01_baseline.md`](01_baseline.md)         | done    |
| 02 | Renames                        | [`02_renames.md`](02_renames.md)           | in progress |
| 03 | Docs and instructions          | [`03_docs.md`](03_docs.md)                 | done    |
| 04 | Move the open roadmap          | [`04_plans.md`](04_plans.md)               | planned |
| 05 | Verify                         | [`05_verify.md`](05_verify.md)             | planned |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-17 : repo created empty on GitHub by the user; this plan written by the session that ran the source's audit.
- 2026-11-17 : phase 01 - source tree at `8b521a7` copied with `git archive`, 262 files matching the audit, byte-identical. Compiles with 0 warnings from a fresh `Library/`.
- 2026-11-17 : phase 02 - renamed to `ArtificialPirate/`, `Artificial Pirate`, `com.pitrified.artificialpirate`, `artificial-pirate`. EditMode 114/114, PlayMode 4/4, 0 warnings, capture unchanged. `dev` APK not yet built: the build was stopped for low memory.
- 2026-11-17 : phase 03 - root README, docs index and instructions about the game; guide-only docs linked on the source's GitHub; the template package id in the release doc replaced. Link check clean.
