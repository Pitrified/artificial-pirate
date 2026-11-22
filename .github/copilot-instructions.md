# Project: artificial-pirate (Unity 6, mobile)

Canonical instructions for agents working in this repo, imported by `CLAUDE.md`.
The file keeps its Copilot name for consistency with the other repos; it is written for Claude Code.

## Source of truth
- What the game is, its scope (in the game, next, not planned) and locked decisions: `docs/functional-specs.md`
- Folder layout & naming: `docs/project-structure.md`
- Per-system specs: `docs/systems/*.md`
- Coding rules: `docs/coding-standards.md`
- How the agent works (loop, gates, what it must not do): the guide's [`ai-development-playbook.md`](https://github.com/Pitrified/unity-setup-project/blob/main/docs/ai-development-playbook.md)
- Running Unity headless (install, commands, pitfalls): `docs/headless-unity.md`
- Unity MCP, the interactive option at the Editor: the guide's [`mcp-setup.md`](https://github.com/Pitrified/unity-setup-project/blob/main/docs/mcp-setup.md), and `docs/unity-mcp-best-practices.md`

If a request contradicts these, stop and ask. If a doc contradicts the code, the code is the fact and the doc is the bug: say so and fix the doc.

## Plans

New work is planned in `plans/NN_name/`, in the shape the `tracked-development` skill describes:
`00_start.md` for the brainstorm and questions, `tracking.md` for the phases and log, one file per phase.
A plan that changes what the game is (scope, a decision, a screen, a control, a system, the save) says so in its `00_start.md`, and updates `docs/functional-specs.md` in its own branch by its last phase, whose "Done when" names it beside the system pages it changes.
`python3 Tools/plans.py list` shows the folders and their status; `python3 Tools/plans.py check` checks them.
`Tools/plans.py` is a vendored copy of the skill's script; to update it, copy the skill's file over it.
The game's history from before this repo (setup checklist, diary) stays in [unity-setup-project](https://github.com/Pitrified/unity-setup-project), where it was built; see `plans/01_bootstrap/00_start.md`.

## Gates

Every change closes on the gates in `docs/headless-unity.md` §3, run by the agent (`Tools/gates.sh all` for the first four):
the spec check (`Tools/spec_check.py`), compile with 0 errors and 0 warnings, EditMode and PlayMode tests green, the `dev` APK for build-affecting changes, a capture for visual ones.
Results go in the commit message. The device run is the human's.

## Stack
Unity `6000.4.4f1` (exact version in `ArtificialPirate/ProjectSettings/ProjectVersion.txt`) · URP · IL2CPP · ARM64 ·
Unity Input System · UI Toolkit · kinematic movement · JSON save in `Application.persistentDataPath`.

## Hard rules
- Match existing asmdef boundaries (`Game.Core`, `Game.Systems`, `Game.Gameplay`,
  `Game.UI`, `Game.Editor`).
- No `FindObjectOfType`/`FindAnyObjectByType`, `GameObject.Find`, `Resources.Load`, `OnGUI`,
  static mutable state outside designated singletons, magic numbers.
  The one exception is `GameBootstrap`, the composition root: it finds the Persistent scene's systems once at boot, since references cannot cross scenes.
- No new packages, no analytics, no ads, no third-party SDKs, unless the plan says so.
- No allocations in `Update` (no `new`, no LINQ, no boxing).
- Logging via `Game.Systems.Log`, never `Debug.Log` directly.
- Stay inside the current plan phase. No "while I'm here" refactors.
- Scenes, prefabs and settings change through Unity (an editor script in batch mode, or MCP at the Editor), not by hand-editing YAML.

## Output style
- Small atomic commits. One concern per commit.
- Public types/members get `///` summary.
- Tests for non-trivial logic in matching `Game.Tests.*` asmdef; a failing test before its fix where possible.
- Stop and ask on a decision that is not obvious from the plan, the spec or the code.
- No em dashes `--`, `---`, curly quotes, or other fancy punctuation.
