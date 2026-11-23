---
status: in progress
priority: 1
description: |
  Rewrite docs/functional-specs.md as the spec of this game (it is still the sample project's), point the
  agent instructions at it truthfully, and make keeping it current part of every feature so it does not
  drift again as the game grows.
---

# Game spec

Asked 2026-11-22 by the user, after `10_ship_graphics` was merged:
"a. We also need to account for the game growing or in the next feature it will drift out of date again."
Option a was: rewrite `docs/functional-specs.md` as the game's spec, rather than only changing the pointer to it.

## The problem

`docs/functional-specs.md` came from the guide, `unity-setup-project`, where it was the sample project's spec.
It calls itself "Source of truth. Any feature not here is out of scope", and its §14 says combat must "not leak into v0.2 code".
`09_ship_damage/00_start.md` Q1 decided it does not bind this game, and touched only §9 (the save schema), because code points at it.
That decision lives only in a plan; the repo's entry points still send agents to the spec as binding:

- `.github/copilot-instructions.md` (imported by `CLAUDE.md`): "Architecture, scope, constraints: `docs/functional-specs.md`", and "If a request contradicts these, stop and ask".
- `AGENTS.md`: "Locked architecture and scope", fourth in the reading order.
- `docs/README.md`: "the **source of truth**", "Do not implement features not defined there. Update the spec **first**, then code", and step 1 of the workflow loop.

A fresh session that follows them reads damage, sinking and masts as out of scope, stops before any work toward cannons, or "fixes" the doc on its own.

## What is true and what is not (first read)

Read on 2026-11-22 at `1f83691`; phase 01 makes it complete.

| § | Section | State |
| -- | -- | -- |
| 1 | Overview | True in outline (a ship on a sea), says nothing of the world, damage, other ships |
| 2 | Core technical decisions | True; kept current (the water row is the game's) |
| 3 | Platform | True |
| 4 | Lifecycle | True, but no game over path (sinking leads to it) |
| 5 | Scenes | True; the Game row was updated |
| 6 | Systems | Missing [`world.md`](../../docs/systems/world.md), and the systems that live inside others' pages: damage and masts (`ship-controller.md`), diagnostics (`logging.md`), HUD, minimap and map (`ui-system.md`) |
| 7 | Controls | To check: the map button, the pause button |
| 8 | UI surfaces | Missing the HUD (hull bar, minimap), the map, game over, the diagnostics panel |
| 9 | Persistence | Version 4 is right, the example lacks `damage.brokenMasts` |
| 10 | Errors | True |
| 11 | Logging | Missing the diagnostics report |
| 12 | Demo-ready | The sample's checklist ("Island is reachable") |
| 13 | Milestones | The sample's M0 to M5, history now |
| 14 | Out of scope | Combat listed as never; it is the next thing planned |

## Why it drifted when the system pages did not

Each feature updated the `docs/systems/` page of the system it touched: the plan's phases named the page, and the code it described was in the same diff.
No feature's phases named the spec, and nothing failed when it went stale.
The spec also repeats what the system pages hold (each system's purpose, the save's JSON), so every change had two places to land.

So the proposal is three parts, each weaker alone:

1. **A thinner spec.** It holds what no system page owns: what the game is, what is in and out of scope now and what comes next, the locked technical decisions, the platform, the lifecycle and scenes, the controls and screens, and an index of the systems. Details live in the system pages and the spec links to them, so a change has one place to land. The save's JSON moves to `save-system.md` (where the save code already points, with the spec), leaving a link.
2. **A step in every feature.** The instructions say: a feature that changes what the game is (scope, a system, a screen, a control, the save) updates the spec in the same branch; its last phase's "Done when" names the spec. The tracked-development skill already reviews a plan before coding; that review checks the spec line is there.
3. **A check that fails.** A small script run by `Tools/gates.sh` (Python, standard library, under a second): every `docs/systems/*.md` is linked from the spec's systems index; every screen in `ScreenId` is named in the spec; the save version in the spec, if it states one, matches `GameSaveData.CurrentVersion`. It catches structural drift (a new system, screen or save version) though not prose that has gone stale; the step in part 2 covers that.

## Questions

- Q1: The spec's shape.
  - a. Thin, as above: scope, decisions, platform, lifecycle, scenes, controls, screens, a systems index; details in the system pages.
  - b. Full: the current sections, each brought up to date, the save's JSON kept here.
  - Recommended: a, because a fact in one place cannot disagree with itself; b is what drifted.
  - ANS: a.
- Q2: Scope words. The spec says "v0.2" throughout and §14 says "not later, not allowed".
  - a. Three lists with no version numbers: **in the game**, **next** (planned or wanted: cannons and combat), **not planned** (multiplayer, IAP, ads, analytics, cloud save, ...). Something moves between lists through a plan.
  - b. Keep versions: v0.2 as built, v0.3 with combat.
  - Recommended: a, because the versions have not been used to cut releases, and "next" says what an agent should not block.
  - ANS: a.
- Q3: The demo-ready checklist (§12) and the milestones (§13).
  - a. Milestones dropped (the plans are the history); demo-ready rewritten as the game's release checks (cold start, the menu loop, continue, sinking to game over, 60 fps held on the Pixel), linked from `build-and-release.md`.
  - b. Both dropped.
  - Recommended: a, because the device checks are still what a release needs.
  - ANS: a.
- Q4: Keeping it current.
  - a. All three parts above: thin spec, a step in every feature, a check in the gates.
  - b. The step only, no check.
  - c. The check only.
  - Recommended: a, because the step catches what the check cannot read and the check catches a forgotten step.
  - ANS: a.
- Q5: The "update the spec first, then code" rule in `docs/README.md`.
  - a. Replace it: a plan says in its start file what it changes in the spec; the spec changes in the same branch, by the last phase, as the system pages do.
  - b. Keep it: the spec changes before any code.
  - Recommended: a, because the spec should say what the game is, and a plan can change course before its code lands.
  - ANS: a.
- Q6: The plan's own scope.
  - a. The spec, the three entry points (`.github/copilot-instructions.md`, `AGENTS.md`, `docs/README.md`), the check, and gaps in system pages found on the way (fixed in place when small).
  - b. Also a pass over every system page for staleness.
  - Recommended: a, because the system pages have been updated by each feature; the audit in phase 01 will say if one is stale, and that becomes a finding.
  - ANS: a.

- Q7: Raised by the review of the sub-plans. `plans.py check --citations` finds 9 places outside `plans/` that point at a plan: `.github/copilot-instructions.md`, `README.md` and `docs/README.md` at `01_bootstrap` (where the game came from); `logging.md`, `ship-controller.md`, `water.md` and `world.md` at the plans that built them. None is from this plan.
  - a. Fix them here: the history pointers say "the guide, unity-setup-project" without the folder; each system page states the decision it cites in place.
  - b. Leave them; a later plan.
  - Recommended: a, because they are the same fault as the spec (entry points leaning on plans), each a line or two.
  - ANS: a.

## Phases (draft, settled after the answers)

1. **Audit**: each section of the spec against the code and the system pages, and every reference to the spec across the repo; the table above completed.
2. **The spec**: rewritten in the shape Q1 to Q3 choose.
3. **Keeping it current**: the instructions and the docs index point at it as it is, the step in every feature, the check in the gates.
4. **Verify**: gates green with the check; the check shown to fail on a removed systems link and a wrong save version; links resolve.
