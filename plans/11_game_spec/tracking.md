# Game spec - implementation tracking

Rewrite `docs/functional-specs.md` as the spec of this game, point the agent instructions at it truthfully, and keep it current as the game grows.
Analysis, proposals and questions in [`00_start.md`](00_start.md).

## Key decisions

- The spec is thin: what the game is, scope in three lists (in the game, next, not planned), decisions, platform, lifecycle, scenes, controls, screens, a systems index, release checks. Details live in the system pages; the save's JSON moves to `save-system.md`.
- Kept current three ways: the spec changes in the feature's branch by its last phase; the entry points say so; `Tools/spec_check.py` in the gates fails on a system page, screen or save version the spec misses.
- Scope of this plan: the spec, the entry points, the check, small fixes to system pages found on the way.

## Phases

| #  | Phase                | Plan | Status |
| -- | -------------------- | ---- | ------ |
| 01 | Audit                | [`01_audit.md`](01_audit.md) | done |
| 02 | The spec             | [`02_spec.md`](02_spec.md) | done |
| 03 | Keeping it current   | [`03_keep_current.md`](03_keep_current.md) | done |
| 04 | Verify               | [`04_verify.md`](04_verify.md) | in progress |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-22 : bootstrapped the folder on `feat/11_game_spec` from `main` at `1f83691`: the spec read section by section against the code and the system pages; why it drifted while the system pages did not; a thin spec, a step in every feature and a check in the gates proposed; Q1 to Q6.
- 2026-11-22 : the user: all a. Folded in; phases 01 to 04 planned.
- 2026-11-22 : review of the sub-plans: the save version check narrowed to `save-system.md`'s JSON (world.md states one too, to go); screens matched by `ScreenId` names in backticks; gates get a `spec` gate; 9 older citations of plans outside `plans/` found (Q7), so phase 04 checks only that this plan adds none.
- 2026-11-22 : phase 01 - review before: nothing to correct. Every section and every reference to the spec in [`01.1_findings.md`](01.1_findings.md): 2, 3, 5, 10 true; 1, 4, 6, 7, 8, 9, 11 stale; 12 to 14 the sample's; 8 references to repoint; `ui-system.md` lacks the diagnostics screen.
- 2026-11-22 : phase 02 - review before: nothing to correct. The spec rewritten thin (`docs/functional-specs.md`); the save's schema moved to `save-system.md` with `brokenMasts`; `world.md`, `project-structure.md`, `GameSaveData.cs` and the draft `04_release` repointed; `ui-system.md` gained the diagnostics screen. Gates green (EditMode 233, PlayMode 24); links resolve.
- 2026-11-23 : phase 03 - review before: in `all`, a spec failure must not skip the tests as a compile failure does (written that way). `Tools/spec_check.py` and the `spec` gate; the instructions, `AGENTS.md`, `docs/README.md` (the spec-first rule replaced by Q5's) and `headless-unity.md` §3 updated. Gates green with the spec line.
- 2026-11-23 : phase 04 - review before: nothing to correct. The check caught each of five breakages on a scratch copy; links and anchors resolve; no new citations of plans. Waiting on the user's read and Q7.
- 2026-11-23 : Q7, the user: a. The 9 older citations of plans fixed in place; `check --citations` clean. Waiting on the user's read of the spec.
