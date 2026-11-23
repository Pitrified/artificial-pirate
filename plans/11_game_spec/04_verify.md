---
status: done
---

# Phase 04 - Verify

## Plan

- The check made to fail on purpose, in a scratch copy of the docs: a systems link removed, a screen name removed, a wrong save version; each reported by name.
- The links in the changed docs resolve.
- `plans.py check --citations`: nothing this plan wrote points at a plan (9 older citations exist; Q7 in [`00_start.md`](00_start.md)).
- A read of the spec cold: does it say what is in the game, what comes next, and what to update when a feature lands.

## Done when

- All of the above hold; the user's read of the spec.

## Results

- 2026-11-23. **The check made to fail**, on a copy of `docs/`, the check and the two source files it reads: a system page added (`cannons.md`), `world.md`'s link removed, `GameOver`'s backticks removed, a `ScreenId` added (`Harbour`), `CurrentVersion` raised to 5. All five reported by name, exit 1; the untouched copy exit 0.
- **Links**: 99 files, 0 broken; the six anchors the changed docs use (`#scenes`, `#release-checks`, `#systems`, `#screens`, `#keeping-this-page-current`, `#schema`) match their headings.
- **Citations**: the same 9 older findings, none from this plan (Q7).
- **Gates** at `17eee05`: spec pass, compile 0 errors 0 warnings, EditMode 233/233, PlayMode 24/24.
- **Q7, a**: the 9 citations fixed. The three history pointers name unity-setup-project without the folder (the README says what moved: the naval slice and its runtime); `logging.md` names the guide; `ship-controller.md` points at the spec's locked decisions; `water.md` states the choice and its alternatives in place; `world.md`'s pointer dropped, since its layers and rules state the choices. `plans.py check --citations`: 11 folders agree. Spec check pass, links 0 broken.
- 2026-11-23. The user read the spec: ok.
