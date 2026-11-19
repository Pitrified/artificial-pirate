"""Ropes and the platforms they lead to: tops and crosstrees, shrouds with ratlines from channels on
the hull's sides, stays (the jibs hang on theirs), backstays; and, per sail state, the running rigging:
sheets, braces, lifts, and the running backstays of a mast with a boom, the lee one let go so the
boom can swing out. Braces lead forward to the mast ahead, as a brig's mainmast's do; the foremost
mast's lead aft to the deck.
"""

from __future__ import annotations

from mathutils import Vector

from builders.ship.hull import half_beam, rail_height
from builders.ship.mesh import MeshParts, Rope, axis_box, line
from builders.ship.rig import Mast, boom_ends, bowsprit, yard_ends
from builders.ship.sails import Trim, jib_corners, sail_name, square_corners

SHROUD = 0.09
RATLINE = 0.05
RUNNING = 0.05
RATLINE_STEP = 0.6
SHROUD_SPACING = 0.9
CHANNEL_OUT = 0.5
CHANNEL_THICKNESS = 0.15
TOP_OF_BEAM = 0.35
TOP_LENGTH = 0.6
TOP_THICKNESS = 0.2
CROSSTREES_OF_TOP = 0.6
CROSSTREES_THICKNESS = 0.15
BRACE_LEAD_AFT = 6.0
BACKSTAY_AFT = 2.5
LIFT_SLOPE = 0.5  # rise to the mast over the yard's half length
JIB_SHEET_AFT = 2.0
COURSE_SHEET_AFT = 3.0


def _rope(parts: MeshParts, kind: str, start: Vector, end: Vector, thickness: float, sail: str | None = None) -> None:
    line(parts, start, end, thickness, "Tar")
    parts.ropes.append(Rope(kind, start, end, thickness, sail))


def _station(p: dict, y: float) -> float:
    return y / p["length"] + 0.5


def _deck_edge(p: dict, x_side: float, y: float) -> Vector:
    t = min(max(_station(p, y), 0.02), 0.98)
    return Vector((x_side * half_beam(p, t), y, rail_height(p, t)))


def _chainplates(p: dict, mast: Mast, side: float) -> list[Vector]:
    """Where a mast's lower shrouds meet the channel on one side (side +1 starboard, -1 port)."""
    points = []
    for i in range(mast.spec["shrouds"]):
        y = mast.foot.y - 0.3 - i * SHROUD_SPACING
        t = _station(p, y)
        points.append(Vector((side * (half_beam(p, t) + CHANNEL_OUT), y, rail_height(p, t) - 0.3)))
    return points


def _top_width(p: dict) -> float:
    return p["beam"] * TOP_OF_BEAM


def _ratlines(parts: MeshParts, shrouds: list[tuple[Vector, Vector]], bottom: float, top: float) -> None:
    z = bottom
    while z < top:
        rung = []
        for low, high in shrouds:
            f = (z - low.z) / (high.z - low.z)
            rung.append(low.lerp(high, f))
        _rope(parts, "ratline", rung[0], rung[-1], RATLINE)
        z += RATLINE_STEP


def _mast_rigging(parts: MeshParts, p: dict, mast: Mast) -> None:
    top_z, cross_z = mast.spec["top_z"], mast.spec["crosstrees_z"]
    d = mast.spec["diameter"]
    width = _top_width(p)
    top_centre = mast.at(top_z) + Vector((0.0, 0.0, TOP_THICKNESS / 2))
    axis_box(parts, top_centre, Vector((width, width * TOP_LENGTH, TOP_THICKNESS)), "ShipWood")
    cross_centre = mast.at(cross_z)
    axis_box(parts, cross_centre, Vector((width * CROSSTREES_OF_TOP, 0.4, CROSSTREES_THICKNESS)), "ShipWood")

    for side in (1.0, -1.0):
        plates = _chainplates(p, mast, side)
        t = _station(p, plates[0].y)
        channel_start = Vector((side * half_beam(p, t), plates[0].y + 0.5, plates[0].z))
        channel_end = Vector((side * (half_beam(p, t) + CHANNEL_OUT), plates[-1].y - 0.5, plates[0].z + CHANNEL_THICKNESS))
        axis_box(parts, (channel_start + channel_end) / 2,
                 Vector((CHANNEL_OUT, abs(channel_end.y - channel_start.y), CHANNEL_THICKNESS)), "ShipWood")

        head = mast.at(top_z) + Vector((side * d * 0.4, 0.0, 0.0))
        lower = [(plate, head) for plate in plates]
        for low, high in lower:
            _rope(parts, "shroud", low, high, SHROUD)
        _ratlines(parts, lower, plates[0].z + 0.6, top_z - 1.0)

        edge = top_centre + Vector((side * width / 2, 0.0, 0.0))
        cross = cross_centre + Vector((side * d * 0.25, 0.0, 0.0))
        upper = [(edge + Vector((0.0, -k * 0.5, 0.0)), cross) for k in range(2)]
        for low, high in upper:
            _rope(parts, "topmast shroud", low, high, SHROUD)
        _ratlines(parts, upper, edge.z + 0.4, cross_z - 1.0)

        if "boom" not in mast.spec:
            _backstay(parts, p, mast, side)


def _backstay(parts: MeshParts, p: dict, mast: Mast, side: float) -> None:
    aft = _chainplates(p, mast, side)[-1].y - BACKSTAY_AFT
    _rope(parts, "backstay", mast.head, _deck_edge(p, side, aft), SHROUD)


def standing(parts: MeshParts, p: dict, masts: list[Mast]) -> None:
    """Tops, channels, shrouds and ratlines, backstays, the stays between masts and to the bowsprit."""
    for mast in masts:
        _mast_rigging(parts, p, mast)
    stem, _ = bowsprit(p)
    by_height = sorted(masts, key=lambda m: m.foot.y, reverse=True)
    fore = by_height[0]
    if not any(spec["kind"] == "jib" and spec["tack"] == 0.0 for spec in p["sails"]):
        _rope(parts, "forestay", fore.at(fore.spec["top_z"]), stem, SHROUD)
    for after, before in zip(by_height[1:], by_height[:-1]):
        _rope(parts, "stay", after.at(after.spec["top_z"]), before.at(before.foot.z + 1.5), SHROUD)
        _rope(parts, "topmast stay", after.head, before.at(before.spec["crosstrees_z"]), SHROUD)
    for i, spec in enumerate(p["sails"]):
        if spec["kind"] == "jib":
            tack, _, head = jib_corners(p, masts[spec["mast"]], spec, 0.0)
            _rope(parts, "jib stay", head, tack, SHROUD, sail_name(i, spec))


def running(parts: MeshParts, p: dict, masts: list[Mast], trim: Trim, sails_set: bool) -> None:
    """Braces and lifts on every yard, running backstays; with the sails set, the sheets from their clews."""
    for mast in masts:
        ahead = min((m for m in masts if m.foot.y > mast.foot.y), key=lambda m: m.foot.y, default=None)
        for yard in mast.spec["yards"]:
            port_end, starboard_end = yard_ends(mast, yard, trim.brace_deg)
            lift_to = mast.at(min(mast.spec["height"], yard["z"] + yard["length"] / 2 * LIFT_SLOPE))
            for end, side in ((port_end, -1.0), (starboard_end, 1.0)):
                lead = ahead.at(min(yard["z"], ahead.spec["height"])) if ahead else _deck_edge(p, side, end.y - BRACE_LEAD_AFT)
                _rope(parts, "brace", end, lead, RUNNING)
                _rope(parts, "lift", end, lift_to, RUNNING)
        if "boom" in mast.spec:
            _, boom_end = boom_ends(mast, trim.boom_deg)
            _rope(parts, "main sheet", boom_end, _deck_edge(p, 0.0, min(boom_end.y + 1.0, -p["length"] / 2 + 0.5)), RUNNING)
            for side in (1.0, -1.0):
                if side * trim.boom_deg <= 0:
                    _backstay(parts, p, mast, side)
    if not sails_set:
        return
    fore_y = max(mast.foot.y for mast in masts)
    for i, spec in enumerate(p["sails"]):
        name = sail_name(i, spec)
        mast = masts[spec["mast"]]
        if spec["kind"] == "jib":
            _, clew, _ = jib_corners(p, mast, spec, trim.jib_deg)
            side = 1.0 if clew.x >= 0 else -1.0
            _rope(parts, "jib sheet", clew, _deck_edge(p, side, min(clew.y - JIB_SHEET_AFT, fore_y)), RUNNING, name)
        elif spec["kind"] == "square" and not isinstance(spec["bottom"], int):
            foot_l, foot_r, _, _ = square_corners(mast, spec, trim.brace_deg)
            for clew, side in ((foot_l, -1.0), (foot_r, 1.0)):
                _rope(parts, "course sheet", clew, _deck_edge(p, side, clew.y - COURSE_SHEET_AFT), RUNNING, name)
