---
status: planned
---

# Phase 03 - Docs and instructions

## Overview

The docs come over written for the source, which was both the template and the game.
Here they describe the game; what is generic to setting up Unity stays in the guide and is linked.

## Plan

- A root `README.md`: what the game is, how to run the gates, where the docs and plans are, where it came from.
- `.github/copilot-instructions.md`, `CLAUDE.md` and `AGENTS.md` about the game; `CLAUDE.md` keeps importing the instructions file.
  The Plans section loses the line about `docs/tracking.md` and `scratch/`, which stay in the source.
- `docs/README.md` indexes what is here and links the guide's docs on GitHub.
- Links to the docs left in the source become absolute links to them on its `main`.
- The fresh-session note is `docs/headless-unity.md` as copied: how a session runs Unity headless and what stays with a person. Checked to be true here after the renames.

## Done when

- The link check passes (relative links resolve; absolute links into the source point at files that exist on its `main`).
- `git grep` finds no reference to `docs/tracking.md`, `scratch/` or a guide-only doc as a local path.

## What the implementation found
