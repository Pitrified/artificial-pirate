#!/usr/bin/env python3
"""Check that docs/functional-specs.md has kept up with the game.

Fails when a page under docs/systems/ is not linked from the spec's Systems section, when a
ScreenId member is not named in backticks in its Screens section, or when the save's version in
docs/systems/save-system.md's schema differs from GameSaveData.CurrentVersion.
It reads structure only; whether the prose is still true is the feature's own step.

Usage: python3 Tools/spec_check.py
Exit code: 0 when the spec agrees, 1 otherwise.
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SPEC = REPO / "docs" / "functional-specs.md"
SYSTEMS = REPO / "docs" / "systems"
SAVE_PAGE = SYSTEMS / "save-system.md"
SCRIPTS = REPO / "ArtificialPirate" / "Assets" / "Scripts"
SCREEN_ID = SCRIPTS / "UI" / "ScreenId.cs"
SAVE_DATA = SCRIPTS / "Gameplay" / "GameSaveData.cs"


def section(markdown: str, title: str) -> str:
    """The text under a level-two heading, up to the next one; empty when there is no such heading."""
    match = re.search(rf"^## {re.escape(title)}\s*$(.*?)(?=^## |\Z)", markdown, re.MULTILINE | re.DOTALL)
    return match.group(1) if match else ""


def screen_ids(source: str) -> list[str]:
    body = re.search(r"enum\s+ScreenId\s*\{(.*?)\}", source, re.DOTALL)
    if not body:
        return []
    without_comments = re.sub(r"//.*", "", body.group(1))
    return re.findall(r"\b([A-Z]\w*)\b", without_comments)


def problems() -> list[str]:
    found = []
    spec = SPEC.read_text(encoding="utf-8")

    systems = section(spec, "Systems")
    for page in sorted(SYSTEMS.glob("*.md")):
        if f"(systems/{page.name})" not in systems:
            found.append(f"{SPEC.name}: Systems does not link systems/{page.name}")

    screens = section(spec, "Screens")
    ids = screen_ids(SCREEN_ID.read_text(encoding="utf-8"))
    if not ids:
        found.append(f"{SCREEN_ID.name}: no ScreenId members found")
    for name in ids:
        if f"`{name}`" not in screens:
            found.append(f"{SPEC.name}: Screens does not name `{name}` (ScreenId)")

    code = re.search(r"CurrentVersion\s*=\s*(\d+)", SAVE_DATA.read_text(encoding="utf-8"))
    doc = re.search(r'"version":\s*(\d+)', section(SAVE_PAGE.read_text(encoding="utf-8"), "Schema"))
    if not code or not doc:
        found.append(f"save version not found: {SAVE_DATA.name} {'ok' if code else 'missing'}, "
                     f"{SAVE_PAGE.name} Schema {'ok' if doc else 'missing'}")
    elif code.group(1) != doc.group(1):
        found.append(f"{SAVE_PAGE.name}: Schema says version {doc.group(1)}, GameSaveData.CurrentVersion is {code.group(1)}")
    return found


def main() -> int:
    found = problems()
    for line in found:
        print(f"spec: {line}")
    return 1 if found else 0


if __name__ == "__main__":
    sys.exit(main())
