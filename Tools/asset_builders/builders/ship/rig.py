"""Masts and spars. The masts and bowsprit are fixed; yards, booms and gaffs turn with the sails' trim."""

from __future__ import annotations

import math
from dataclasses import dataclass

from mathutils import Vector

from builders.ship.hull import deck_height
from builders.ship.mesh import MeshParts, pole

MAST_SIDES = 8
SPAR_SIDES = 6
SPAR_OF_MAST = 0.45
SAIL_CLEARANCE = 0.3


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
    centre = mast.at(yard["z"]) + forward * SAIL_CLEARANCE
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


def standing(parts: MeshParts, p: dict, all_masts: list[Mast]) -> None:
    """The masts and the bowsprit."""
    for mast in all_masts:
        d = mast.spec["diameter"]
        pole(parts, mast.foot, mast.head, d, d * 0.5, MAST_SIDES, "ShipWood")
    d = p["bowsprit"]["diameter"]
    pole(parts, *bowsprit(p), d, d * 0.5, SPAR_SIDES, "ShipWood")


def spars(parts: MeshParts, all_masts: list[Mast], brace_deg: float, swing_deg: float, gaff_twist_deg: float) -> None:
    """Yards, booms and gaffs, trimmed."""
    for mast in all_masts:
        d = mast.spar_diameter
        for yard in mast.spec["yards"]:
            pole(parts, *yard_ends(mast, yard, brace_deg), d, d, SPAR_SIDES, "ShipWood")
        if "boom" in mast.spec:
            pole(parts, *boom_ends(mast, swing_deg), d, d * 0.8, SPAR_SIDES, "ShipWood")
        if "gaff" in mast.spec:
            pole(parts, *gaff_ends(mast, swing_deg + gaff_twist_deg), d, d * 0.8, SPAR_SIDES, "ShipWood")
