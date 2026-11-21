"""Masts and spars. The masts and bowsprit are fixed; yards, booms and gaffs turn with the sails' trim.
Each mast's parts go in its own parts, so it can be shown, broken and hidden on its own; a broken
mast leaves a stump."""

from __future__ import annotations

import math
from dataclasses import dataclass

from mathutils import Vector

from builders.ship.hull import deck_height
from builders.ship.mesh import MeshParts, frame, pole

MAST_SIDES = 16
SPAR_SIDES = 10
SPAR_OF_MAST = 0.45
STUMP_HEIGHT = 1.4  # above the deck
STUMP_SPLINTER = 0.6  # how far the broken top's points rise and fall


@dataclass
class Mast:
    """A mast's foot on deck and its direction, so spars and sails can find a point at any height."""

    foot: Vector
    direction: Vector
    spec: dict

    def at(self, z: float) -> Vector:
        return self.foot + self.direction * ((z - self.foot.z) / self.direction.z)

    @property
    def head(self) -> Vector:
        return self.at(self.spec["height"])

    @property
    def spar_diameter(self) -> float:
        return self.spec["diameter"] * SPAR_OF_MAST


def masts(p: dict) -> list[Mast]:
    result = []
    for spec in p["masts"]:
        t = spec["at"]
        foot = Vector((0.0, (t - 0.5) * p["length"], deck_height(p, t)))
        rake = math.radians(spec["rake_deg"])
        result.append(Mast(foot, Vector((0.0, -math.sin(rake), math.cos(rake))), spec))
    return result


def bowsprit(p: dict) -> tuple[Vector, Vector]:
    spec = p["bowsprit"]
    steeve = math.radians(spec["steeve_deg"])
    start = Vector((0.0, p["length"] / 2 - 1.0, deck_height(p, 1.0) + 0.3))
    end = Vector((0.0, p["length"] / 2, start.z)) + Vector((0.0, math.cos(steeve), math.sin(steeve))) * spec["length"]
    return start, end


def yard_forward(brace_deg: float) -> Vector:
    """Square to the yards, braced round by brace_deg (positive toward starboard)."""
    brace = math.radians(brace_deg)
    return Vector((math.sin(brace), math.cos(brace), 0.0))


def yard_ends(mast: Mast, yard: dict, brace_deg: float) -> tuple[Vector, Vector]:
    forward = yard_forward(brace_deg)
    centre = mast.at(yard["z"]) + forward * ((mast.spec["diameter"] + mast.spar_diameter) / 2)
    half = Vector((forward.y, -forward.x, 0.0)) * (yard["length"] / 2)
    return centre - half, centre + half


def _swung(swing_deg: float, lift_deg: float) -> Vector:
    """Aft along the centreline, swung out by swing_deg (positive to starboard) and lifted by lift_deg."""
    swing, lift = math.radians(swing_deg), math.radians(lift_deg)
    return Vector((math.sin(swing) * math.cos(lift), -math.cos(swing) * math.cos(lift), math.sin(lift)))


def boom_ends(mast: Mast, swing_deg: float) -> tuple[Vector, Vector]:
    boom = mast.spec["boom"]
    start = mast.at(boom["z"])
    return start, start + _swung(swing_deg, boom["rise_deg"]) * boom["length"]


def gaff_ends(mast: Mast, swing_deg: float) -> tuple[Vector, Vector]:
    gaff = mast.spec["gaff"]
    start = mast.at(gaff["z"])
    return start, start + _swung(swing_deg, gaff["peak_deg"]) * gaff["length"]


def standing(per_mast: list[MeshParts], all_masts: list[Mast]) -> None:
    """The masts, each in its own parts."""
    for parts, mast in zip(per_mast, all_masts, strict=True):
        d = mast.spec["diameter"]
        pole(parts, mast.foot, mast.head, d, d * 0.5, MAST_SIDES, "Spar", smooth=True)


def bowsprit_pole(parts: MeshParts, p: dict) -> None:
    d = p["bowsprit"]["diameter"]
    pole(parts, *bowsprit(p), d, d * 0.5, SPAR_SIDES, "Spar", smooth=True)


def stump(parts: MeshParts, mast: Mast) -> None:
    """What a broken mast leaves: its foot standing a little above the deck, its top splintered."""
    d = mast.spec["diameter"]
    top = mast.at(mast.foot.z + STUMP_HEIGHT)
    side, up = frame(mast.direction)
    base = parts.add_verts([mast.foot + (side * math.cos(a) + up * math.sin(a)) * d / 2 for a in _angles()])
    jagged = [top + mast.direction * (STUMP_SPLINTER * d * (1 if k % 2 else -1)) + (side * math.cos(a) + up * math.sin(a)) * d / 2
              for k, a in enumerate(_angles())]
    crown = parts.add_verts(jagged)
    centre = parts.add_verts([top])
    step = math.pi * d / MAST_SIDES
    for k in range(MAST_SIDES):
        n = (k + 1) % MAST_SIDES
        uvs = ((0.0, k * step), ((jagged[k] - mast.foot).length, k * step),
               ((jagged[n] - mast.foot).length, (k + 1) * step), (0.0, (k + 1) * step))
        parts.add_face((base + k, crown + k, crown + n, base + n), "Spar", smooth=True, uvs=uvs)
        parts.add_planar_face((crown + n, crown + k, centre), "Spar")
        parts.mark_sharp(crown + k, crown + n)


def _angles() -> list[float]:
    return [2 * math.pi * k / MAST_SIDES for k in range(MAST_SIDES)]


def spars(per_mast: list[MeshParts], all_masts: list[Mast], brace_deg: float, swing_deg: float, gaff_twist_deg: float) -> None:
    """Yards, booms and gaffs, trimmed, each mast's in its parts."""
    for parts, mast in zip(per_mast, all_masts, strict=True):
        d = mast.spar_diameter
        for yard in mast.spec["yards"]:
            pole(parts, *yard_ends(mast, yard, brace_deg), d, d, SPAR_SIDES, "Spar", smooth=True)
        if "boom" in mast.spec:
            pole(parts, *boom_ends(mast, swing_deg), d, d * 0.8, SPAR_SIDES, "Spar", smooth=True)
        if "gaff" in mast.spec:
            pole(parts, *gaff_ends(mast, swing_deg + gaff_twist_deg), d, d * 0.8, SPAR_SIDES, "Spar", smooth=True)
