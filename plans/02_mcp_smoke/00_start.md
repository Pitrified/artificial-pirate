---
status: draft
priority: 0
description: |
  Confirm Unity MCP works for Claude Code at the workstation Editor: the pinned CoplayDev server
  starts, Claude Code connects, and a read-only smoke prompt matches the Hierarchy.
---

# MCP smoke test

Moved on 2026-11-17 from phase 9 of [unity-setup-project's `docs/tracking.md`](https://github.com/Pitrified/unity-setup-project/blob/main/docs/tracking.md#phase-9---mcp-integration-optional-but-recommended), when the game was split into this repo, and brought up to date.

## Where it stands

Batch mode is the default way into Unity ([`docs/headless-unity.md`](../../docs/headless-unity.md)); MCP is the interactive option at a workstation Editor (the source's `plans/01_repo_split/00_start.md` Q14).
Of the source's phase 9:

- Choose the server and pin it: done. `com.coplaydev.unity-mcp` at `b92c05a2` in `ArtificialPirate/Packages/manifest.json`.
- `.vscode/mcp.json` for Copilot: superseded. The agent is Claude Code (source Q15), configured with `claude mcp add` as the guide's [`mcp-setup.md` §2](https://github.com/Pitrified/unity-setup-project/blob/main/docs/mcp-setup.md#2-setup) says.
- Confirm the client connects, and the smoke prompt: open, below.

## Plan

- HUMAN, at the workstation: open `ArtificialPirate/`, **Window → MCP for Unity → Start Server**, then add the server to Claude Code as [`mcp-setup.md` §2](https://github.com/Pitrified/unity-setup-project/blob/main/docs/mcp-setup.md#2-setup) says.
- HUMAN: `/mcp` in Claude Code lists `unityMCP` as connected.
- Smoke prompt, read-only: list the root GameObjects of `Boot.unity`; they match the Hierarchy.
- Record the result, and any setup step that differed from the guide, in this folder's log; a difference is fixed in the guide.

## Done when

- The smoke prompt's answer matches the Hierarchy, recorded in the log.
