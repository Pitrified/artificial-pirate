"""Sails, set for one wind or furled, with the yards, booms and gaffs that carry them.

A set sail is a two-sided grid bellied downwind: camber about a tenth of the chord, deepest 40% back
from the luff on a fore-and-aft sail and in the middle of a square one, and its free edges cut
hollow between their corners.
"""

from __future__ import annotations

import dataclasses
import itertools
import math
from dataclasses import dataclass

from mathutils import Vector

from builders.ship.mesh import Cloth, MeshParts, frame, pole
from builders.ship.rig import Mast, boom_ends, bowsprit, gaff_ends, spars, yard_ends

WIND_FROM_DEG = 130.0  # the one wind for now: from the starboard quarter, degrees clockwise from the bow
MAX_BRACE_DEG = 40.0
GAFF_TWIST = 1.2
BOOM_IN_STEP_DEG = 1.0
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
FURL_SWELL = 0.5
FURL_LUMP = 0.12
FURL_FOLDS = 3
FURL_FOLD_DEPTH = 0.12
FURL_SAG = 0.35
GASKET_STEP = 1.6
GASKET_PINCH = 0.7
GASKET_WIDTH = 0.12
GASKET_OUT = 1.04
GASKET_SIDES = 6


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

    def boom_in(self) -> Trim:
        """The boom and gaff a step closer to the centreline."""
        boom = math.copysign(max(0.0, abs(self.boom_deg) - BOOM_IN_STEP_DEG), self.boom_deg)
        return dataclasses.replace(self, boom_deg=boom, gaff_deg=boom * GAFF_TWIST)

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


def sail_name(index: int, spec: dict) -> str:
    return f"sail {index} ({spec['kind']}, mast {spec['mast']})"


def _sheet(parts: MeshParts, name: str, corners: tuple[Vector, Vector, Vector, Vector], cut: _Cut,
           downwind: Vector) -> None:
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
    first = len(parts.faces)
    for side in (0, 1):
        s = parts.add_verts(grid, uvs)
        for row in range(ROWS):
            for col in range(COLUMNS):
                q = (s + row * width + col, s + row * width + col + 1,
                     s + (row + 1) * width + col + 1, s + (row + 1) * width + col)
                parts.add_face(q if side == 0 else tuple(reversed(q)), "SailCloth", smooth=True)
        if side == 0:
            parts.cloths.append(Cloth(name, range(first, len(parts.faces))))


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
    for i, spec in enumerate(p["sails"]):
        name = sail_name(i, spec)
        mast = all_masts[spec["mast"]]
        kind = spec["kind"]
        if kind == "square":
            _sheet(parts, name, square_corners(mast, spec, trim.brace_deg), _SQUARE, trim.downwind)
        elif kind == "gaff":
            tack, clew = boom_ends(mast, trim.boom_deg)
            throat, peak = gaff_ends(mast, trim.gaff_deg)
            _sheet(parts, name, (tack, clew, peak, throat), _GAFF, trim.downwind)
        elif kind == "jib":
            tack, clew, head = jib_corners(p, mast, spec, trim.jib_deg)
            _sheet(parts, name, (tack, clew, head, head), _JIB, trim.downwind)
        else:
            msg = f"unknown sail kind '{kind}'"
            raise ValueError(msg)


# ---------------------------------------------------------------- furled


def _furl(parts: MeshParts, name: str, start: Vector, end: Vector, height: float, rests: Vector) -> None:
    """A furled sail: a bundle resting on the spar on its `rests` side, tied by gaskets that pinch it, bulging and folded
    between them, swelling to the middle and sagging under its weight. Its UVs run along it and
    round it, so the canvas's seams wrap it like folds."""
    trim = (end - start) * (1 - FURL_OF_SPAR) / 2
    a, b = start + trim, end - trim
    length = (b - a).length
    axis = (b - a).normalized()
    side, up = frame(axis)
    middle = max(FURL_MIN_DIAMETER, height * FURL_OF_HEIGHT) / 2
    phase = a.x * 1.7 + a.y * 0.3 + a.z * 1.1  # a fixed seed per furl, so each bunches differently
    ties = max(2, round(length / GASKET_STEP))

    def radius(s: float, tied: bool) -> float:
        lump = 1 + FURL_LUMP * math.sin(2 * math.pi * 2.3 * s + phase)
        pinch = GASKET_PINCH if tied else 1.0
        return middle * (1 - FURL_SWELL * (1 - math.sin(math.pi * s))) * lump * pinch

    def centre(s: float) -> Vector:
        return a.lerp(b, s) + rests.normalized() * middle - Vector((0.0, 0.0, FURL_SAG * middle * math.sin(math.pi * s)))

    angles = [2 * math.pi * k / FURL_SIDES for k in range(FURL_SIDES + 1)]
    rings = []
    for i in range(2 * ties + 1):
        s = i / (2 * ties)
        tied = i % 2 == 0 and 0 < i < 2 * ties
        c, r = centre(s), radius(s, tied)
        folds = 0.0 if tied else FURL_FOLD_DEPTH
        twist = phase + i * 1.3  # each bulge folds a different way
        ring = [c + (side * math.cos(t) + up * math.sin(t)) * (r * (1 + folds * math.sin(FURL_FOLDS * t + twist)))
                for t in angles]
        rings.append(parts.add_verts(ring, [(s * length, t * middle) for t in angles]))
        if tied:
            d = 2 * r * GASKET_OUT
            pole(parts, c - axis * GASKET_WIDTH / 2, c + axis * GASKET_WIDTH / 2, d, d, GASKET_SIDES, "Tar", caps=False)
    first = len(parts.faces)
    for r0, r1 in itertools.pairwise(rings):
        for k in range(FURL_SIDES):
            parts.add_face((r0 + k, r1 + k, r1 + k + 1, r0 + k + 1), "FurledCloth", smooth=True)
    for ring, s, flip in ((rings[0], 0.0, True), (rings[-1], 1.0, False)):
        tip = parts.add_verts([centre(s) + axis * (radius(s, False) * (-0.5 if flip else 0.5))], [(s * length, 0.0)])
        for k in range(FURL_SIDES):
            tri = (ring + k + 1, ring + k, tip) if flip else (ring + k, ring + k + 1, tip)
            parts.add_face(tri, "FurledCloth", smooth=True)
    parts.cloths.append(Cloth(name, range(first, len(parts.faces))))


def anchored(parts: MeshParts, p: dict, all_masts: list[Mast]) -> None:
    """The spars squared and every sail rolled up: on top of and before its yard, on its boom, or
    stowed along the bowsprit, the jibs each in their own stretch of it, innermost tack first."""
    spars(parts, all_masts, 0.0, 0.0, 0.0)
    start, end = bowsprit(p)
    jibs = sorted((spec["tack"], i) for i, spec in enumerate(p["sails"]) if spec["kind"] == "jib")
    stretch = {i: k for k, (_, i) in enumerate(jibs)}
    above = Vector((0.0, 0.0, 1.0))
    before = Vector((0.0, 1.0, 1.0))
    for i, spec in enumerate(p["sails"]):
        name = sail_name(i, spec)
        mast = all_masts[spec["mast"]]
        kind = spec["kind"]
        if kind == "square":
            foot_l, _, top_r, top_l = square_corners(mast, spec, 0.0)
            _furl(parts, name, top_l, top_r, top_l.z - foot_l.z, before)
        elif kind == "gaff":
            tack, clew = boom_ends(mast, 0.0)
            throat, _ = gaff_ends(mast, 0.0)
            _furl(parts, name, tack, clew, throat.z - tack.z, above)
        elif kind == "jib":
            k = stretch[i]
            tack = start.lerp(end, spec["tack"])
            _furl(parts, name, start.lerp(end, k / len(jibs)), start.lerp(end, (k + 1) / len(jibs)),
                  spec["head_z"] - tack.z, above)
