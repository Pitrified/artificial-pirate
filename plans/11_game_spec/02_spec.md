---
status: done
---

# Phase 02 - The spec

## Overview

`docs/functional-specs.md` rewritten as this game's spec, thin (Q1), with three scope lists (Q2), release checks instead of the demo checklist and no milestones (Q3).
Depends on [`01_audit.md`](01_audit.md).

## Plan

- Sections: what the game is; scope (in the game, next, not planned); locked technical decisions; platform; lifecycle (with game over); scenes; controls; screens; systems index (each system page, and the systems inside another's page); release checks; errors; how the spec is kept current (the rule, short).
- The save's JSON and its rules move to `docs/systems/save-system.md`; the spec links to it.
- References found in phase 01 repointed: `save-system.md`, `world.md`, `project-structure.md`, `GameSaveData.cs`, the draft `04_release` plan's link to the checklist.
- Small staleness found in a system page on the way fixed in place (Q6).

## Done when

- Every line of the spec is true at the commit; the links resolve; gates green (a comment in `GameSaveData.cs` changes).
