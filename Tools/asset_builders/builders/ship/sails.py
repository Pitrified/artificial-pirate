"""Sails, set for one wind or furled, with the yards, booms and gaffs that carry them.

A set sail is a two-sided grid bellied downwind: camber about a tenth of the chord, deepest 40% back
from the luff on a fore-and-aft sail and in the middle of a square one, and its free edges cut
hollow between their corners.
"""

from __future__ import annotations

import math
from dataclasses import dataclass

from mathutils import Vector

from builders.ship.mesh import MeshParts, pole
from builders.ship.rig import Mast, boom_ends, bowsprit, gaff_ends, spars, yard_ends

WIND_FROM_DEG = 130.0  # the one wind for now: from the starboard quarter, degrees clockwise from the bow
MAX_BRACE_DEG = 40.0
GAFF_TWIST = 1.2
JIB_SHEET = 0.6

ROWS = 6
COLUMNS = 6
CAMBER = 0.12
FORE_AND_AFT_DRAFT = 0.4
FOOT_SAG = 0.06
LEECH_SAG = 0.05
SQUARE_SIDE_SAG = 0.03

FURL_SIDES = 8
FURL_OF_HEIGHT = 0.08
FURL_MIN_DIAMETER = 0.35
FURL_OF_SPAR = 0.9


@dataclass(frozen=True)
class Trim:
    """How the yards, booms, gaffs and jib sheets stand, in degrees (positive toward starboard)."""

    brace_deg: float
    boom_deg: float
    gaff_deg: float
    jib_deg: float
    downwind: Vector

    @classmethod
    def for_wind(cls, wind_from_deg: float) -> Trim:
        to = math.radians(((wind_from_deg + 360.0) % 360.0) - 180.0)  # where the wind blows, from the bow
        half = math.degrees(to) / 2
        return cls(
            brace_deg=max(-MAX_BRACE_DEG, min(MAX_BRACE_DEG, half)),
            boom_deg=half,
            gaff_deg=half * GAFF_TWIST,
            jib_deg=half * JIB_SHEET,
            downwind=Vector((math.sin(to), math.cos(to), 0.0)),
        )

    @classmethod
    def at_anchor(cls) -> Trim:
        return cls(0.0, 0.0, 0.0, 0.0, Vector((0.0, 1.0, 0.0)))


# ---------------------------------------------------------------- set


@dataclass(frozen=True)
class _Cut:
    """Where a sail is deepest along its chord, and which of its edges are free to sag."""

    draft_at: float
    foot_free: bool
    foot_sag: float
    leech_sag: float
    side_sag: float


_SQUARE = _Cut(0.5, True, FOOT_SAG, 0.0, SQUARE_SIDE_SAG)
_GAFF = _Cut(FORE_AND_AFT_DRAFT, False, 0.0, LEECH_SAG, 0.0)
_JIB = _Cut(FORE_AND_AFT_DRAFT, True, FOOT_SAG, LEECH_SAG, 0.0)


def _sheet(parts: MeshParts, corners: tuple[Vector, Vector, Vector, Vector], cut: _Cut, downwind: Vector) -> None:
    """A two-sided sail over (tack or clew, clew, head end, head start): u runs luff to leech, v foot to head."""
    a, b, c, d = corners
    normal = (b - a).cross(d - a).normalized()
    if normal.dot(downwind) < 0:
        normal = -normal
    chord = (b - a).length
    height = ((c + d) / 2 - (a + b) / 2).length
    sharpness = math.log(0.5) / math.log(cut.draft_at)

    grid, uvs = [], []
    for row in range(ROWS + 1):
        v0 = row / ROWS
        for col in range(COLUMNS + 1):
            u0 = col / COLUMNS
            v = v0 + cut.foot_sag * math.sin(math.pi * u0) * (1 - v0)
            u = u0 - cut.leech_sag * math.sin(math.pi * v0) * u0
            u = 0.5 + (u - 0.5) * (1 - cut.side_sag * math.sin(math.pi * v0))
            point = a.lerp(b, u).lerp(d.lerp(c, u), v)
            envelope = math.sin(math.pi * (0.25 + 0.75 * v)) if cut.foot_free else math.sin(math.pi * v)
            depth = CAMBER * chord * math.sin(math.pi * u**sharpness) * envelope
            grid.append(point + normal * depth)
            uvs.append((u * chord, v * height))

    width = COLUMNS + 1
    for side in (0, 1):
        s = parts.add_verts(grid, uvs)
        for row in range(ROWS):
            for col in range(COLUMNS):
                q = (s + row * width + col, s + row * width + col + 1,
                     s + (row + 1) * width + col + 1, s + (row + 1) * width + col)
                parts.add_face(q if side == 0 else tuple(reversed(q)), "SailCloth", smooth=True)


def jib_corners(p: dict, mast: Mast, spec: dict, sheet_deg: float) -> tuple[Vector, Vector, Vector]:
    start, end = bowsprit(p)
    tack = start.lerp(end, spec["tack"])
    head = mast.at(spec["head_z"]) + Vector((0.0, 0.3, 0.0))
    sheet = math.radians(sheet_deg)
    aft = spec["clew_aft"]
    clew = Vector((math.sin(sheet) * aft, tack.y - math.cos(sheet) * aft, spec["clew_z"]))
    return tack, clew, head


def square_corners(mast: Mast, spec: dict, brace_deg: float) -> tuple[Vector, Vector, Vector, Vector]:
    yards = mast.spec["yards"]
    top_l, top_r = yard_ends(mast, yards[spec["top"]], brace_deg)
    bottom = spec["bottom"]
    foot_l, foot_r = yard_ends(mast, yards[bottom] if isinstance(bottom, int) else bottom, brace_deg)
    return foot_l, foot_r, top_r, top_l


def sailing(parts: MeshParts, p: dict, all_masts: list[Mast], trim: Trim) -> None:
    """The spars trimmed to the wind and every sail set."""
    spars(parts, all_masts, trim.brace_deg, trim.boom_deg, trim.gaff_deg - trim.boom_deg)
    for spec in p["sails"]:
        mast = all_masts[spec["mast"]]
        kind = spec["kind"]
        if kind == "square":
            _sheet(parts, square_corners(mast, spec, trim.brace_deg), _SQUARE, trim.downwind)
        elif kind == "gaff":
            tack, clew = boom_ends(mast, trim.boom_deg)
            throat, peak = gaff_ends(mast, trim.gaff_deg)
            _sheet(parts, (tack, clew, peak, throat), _GAFF, trim.downwind)
        elif kind == "jib":
            tack, clew, head = jib_corners(p, mast, spec, trim.jib_deg)
            _sheet(parts, (tack, clew, head, head), _JIB, trim.downwind)
        else:
            msg = f"unknown sail kind '{kind}'"
            raise ValueError(msg)


# ---------------------------------------------------------------- furled


def _furl(parts: MeshParts, start: Vector, end: Vector, height: float, offset: Vector) -> None:
    d = max(FURL_MIN_DIAMETER, height * FURL_OF_HEIGHT)
    trim = (end - start) * (1 - FURL_OF_SPAR) / 2
    pole(parts, start + trim + offset * d, end - trim + offset * d, d, d, FURL_SIDES, "SailCloth")


def anchored(parts: MeshParts, p: dict, all_masts: list[Mast]) -> None:
    """The spars squared and every sail rolled up on its yard, boom or bowsprit."""
    spars(parts, all_masts, 0.0, 0.0, 0.0)
    above = Vector((0.0, 0.0, 0.6))
    for spec in p["sails"]:
        mast = all_masts[spec["mast"]]
        kind = spec["kind"]
        if kind == "square":
            foot_l, _, top_r, top_l = square_corners(mast, spec, 0.0)
            _furl(parts, top_l, top_r, top_l.z - foot_l.z, above)
        elif kind == "gaff":
            tack, clew = boom_ends(mast, 0.0)
            throat, _ = gaff_ends(mast, 0.0)
            _furl(parts, tack, clew, throat.z - tack.z, above)
        elif kind == "jib":
            start, end = bowsprit(p)
            tack = start.lerp(end, spec["tack"])
            _furl(parts, tack.lerp(start, 0.5), tack, spec["head_z"] - tack.z, above)
