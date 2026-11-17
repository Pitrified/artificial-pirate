---
status: in progress
priority: 0
description: |
  Bootstrap this repo from unity-setup-project: the naval vertical slice with the generic runtime
  it runs on, renamed to artificial-pirate, its gates run headless from the first commit, and the
  game's open roadmap moved in as plan folders numbered from 02.
---

# Bootstrap artificial-pirate

Started 2026-11-17. Phases in [`tracking.md`](tracking.md).

## Where this came from

The game grew inside [unity-setup-project](https://github.com/Pitrified/unity-setup-project), a repo that began as a Unity 6 mobile setup and grew the naval slice as its sample.
That repo is being split: it becomes a guide to setting up a Unity mobile project, linking to projects that implement a pattern worth copying, and the game moves here.
The split is planned in that repo's `plans/01_repo_split/`, whose audit (`02.1_audit_table.md` there) gives every tracked file a destination: `both`, `pirate`, `guide` or `drop`.

There is no bootstrap script. A Claude session reads this plan and the source repo and writes this one.
The session that wrote it also wrote the audit, so this plan is the record of what it assumed.

## Source

- Repo: `Pitrified/unity-setup-project`, `main` at `8b521a7`.
  That commit already carries the cleanup made for this split (template leftovers dropped, MCP pinned, a generic versioned save envelope with the ship's section in `Game.Gameplay`, the debug overlay moved into `Persistent.unity`, the AI workflow docs rewritten for Claude Code running its own gates).
- Rows taken: those marked `pirate` or `both` in the audit table.
- No LFS object at that commit; `.gitattributes` keeps the LFS rules so the next binary lands in LFS.

## Scope

- **Kept**: the game as it runs at the source commit: Boot, Persistent, Menu and Game scenes, the ship and chase camera, the water shader, pause, save and restore, the debug overlay, the generic runtime under it, all tests.
- **Renamed** (source Q2, Q7, Q8): Unity project folder `UnitySetupPrpj/` to `ArtificialPirate/`, product name `Artificial Pirate`, Android id `com.pitrified.artificialpirate`, and `artificial-pi` to `artificial-pirate` in docs, scripts, menus, the shader name and `~/.config/artificial-pirate/build.env`.
  The guide keeps `com.pitrified.unitysetup`, so builds of the two install side by side.
- **Links instead of copies**: the docs the audit marks `guide` (AI playbook, MCP setup, dev environment from zero, the instructions template) are linked on the source's GitHub.
- **Gates from the first commit**, run headless on the box as `docs/headless-unity.md` describes: compile with 0 errors and 0 warnings, EditMode and PlayMode tests, the `dev` APK.

## Plans

Numbered from 01 here.
The source's open roadmap is phases 9 to 13 of its `docs/tracking.md`, a checklist rather than plan folders (source Q12). They move as:

| Here | Source |
| ---- | ------ |
| `02_mcp_smoke` | Phase 9, MCP integration. The server choice and pin are done at the source commit; what is left is the smoke test at the workstation Editor with Claude Code. |
| `03_performance` | Phase 10, performance pass. |
| `04_release` | Phases 11 to 13: upload key, Play Console, demo-ready. One folder, since each step gates the next and all three are short. |

The source's phases 0 to 8 and its `scratch/` diary stay there as history.

## Open points

- Q1: Unity Cloud link. `ProjectSettings.asset` carries the source's `cloudProjectId`, `organizationId` and `productGUID`, so both repos point at the same Unity Cloud project. `cloudEnabled` is 0 and nothing here uses Unity services (the spec rules out analytics and ads SDKs).
  - a. leave it; link a separate cloud project if a Unity service is ever needed.
  - b. unlink now, in the Editor at the workstation (Project Settings, Services).
  - Recommended: a. It has no effect today, and unlinking is an Editor action with no batch-mode API.

## Out of scope

- New gameplay. The game behaves as it does in the source.
- The asset-building method (the source's `plans/03_asset_method/`); it is copied here when it lands, with a findings doc for the ship.
- CI. The gates run on the box.
