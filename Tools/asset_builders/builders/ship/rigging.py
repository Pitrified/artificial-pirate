"""Ropes and the platforms they lead to: tops and crosstrees, shrouds with ratlines from channels on
the hull's sides, stays (the jibs hang on theirs), backstays; and, per sail state, the running rigging:
sheets, braces, lifts, and the running backstays of a mast with a boom, the lee one let go so the
boom can swing out. Braces lead forward to the mast ahead, as a brig's mainmast's do; the foremost
mast's lead aft to the deck. Each rope goes in the parts of the mast it belongs to: a stay with the
mast it holds up from behind, a jib's with the jib's mast; the channels stay on the hull.
"""

from __future__ import annotations

from mathutils import Vector

from builders.ship.hull import rail_height
from builders.ship.hull import side as side_at
from builders.ship.mesh import MeshParts, Rope, axis_box, line, pole
from builders.ship.rig import Mast, boom_ends, bowsprit, stay_ends, yard_ends
from builders.ship.sails import (
    Trim,
    jib_corners,
    sail_name,
    square_corners,
    staysail_corners,
)

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
TOPMAST_SHROUDS_AFT = 0.25  # of the top's length: the topmast shrouds' feet aft of the mast, clear of braced yards
CROSSTREES_OF_TOP = 0.6
CROSSTREES_THICKNESS = 0.15
TOP_RIM = Vector((0.06, 0.06, 0.12))  # thickness, and height above the platform
BRACE_LEAD_AFT = 6.0
BACKSTAY_AFT = 2.5
LIFT_SLOPE = 0.5
BOBSTAY_AT = 0.6  # along the bowsprit from its heel
BOBSTAY_FOOT = 0.8  # m above the water, at the stem
CUTWATER_AHEAD = 0.3  # m, the cutwater's front ahead of the stem near the water
STRIKER_AT = 0.5  # along the bowsprit: the dolphin striker's head
STRIKER_OF_BOWSPRIT = 0.12  # its length, of the bowsprit's
STRIKER_OF_DIAMETER = 0.4  # its diameter, of the bowsprit's
MARTINGALE_AT = 0.95  # along the bowsprit: the martingale stay's head, under it
MARTINGALE_UNDER = 0.6  # of the bowsprit's diameter: clear of a jib stowed along its end
BACK_ROPES_AT = 0.96  # fraction from the stern: where the martingale's back ropes meet the bows
BACK_ROPES_UNDER_RAIL = 1.0  # m  # rise to the mast over the yard's half length
JIB_SHEET_AFT = 2.0
COURSE_SHEET_AFT = 3.0


def _rope(parts: MeshParts, kind: str, start: Vector, end: Vector, thickness: float, sail: str | None = None) -> None:
    line(parts, start, end, thickness, "Tar")
    parts.ropes.append(Rope(kind, start, end, thickness, sail))


def _station(p: dict, y: float) -> float:
    return y / p["length"] + 0.5


def _deck_edge(p: dict, x_side: float, y: float) -> Vector:
    t = min(max(_station(p, y), 0.02), 0.98)
    return Vector((x_side * side_at(p, t, rail_height(p, t)), y, rail_height(p, t)))


def _chainplates(p: dict, mast: Mast, side: float) -> list[Vector]:
    """Where a mast's lower shrouds meet the channel on one side (side +1 starboard, -1 port)."""
    points = []
    for i in range(mast.spec["shrouds"]):
        y = mast.foot.y - 0.3 - i * SHROUD_SPACING
        t = _station(p, y)
        points.append(Vector((side * (side_at(p, t, rail_height(p, t)) + CHANNEL_OUT), y, rail_height(p, t) - 0.3)))
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


def _rim(parts: MeshParts, centre: Vector, width: float, length: float) -> None:
    """A low rim round a top's platform, so it reads as a platform."""
    for x in (-1.0, 1.0):
        axis_box(parts, centre + Vector((x * (width - TOP_RIM.x) / 2, 0.0, 0.0)), Vector((TOP_RIM.x, length, TOP_RIM.z)), "Spar")
    for y in (-1.0, 1.0):
        axis_box(parts, centre + Vector((0.0, y * (length - TOP_RIM.y) / 2, 0.0)), Vector((width - TOP_RIM.x * 2, TOP_RIM.y, TOP_RIM.z)),
                 "Spar")


def _mast_rigging(body: MeshParts, parts: MeshParts, p: dict, mast: Mast) -> None:
    top_z, cross_z = mast.spec["top_z"], mast.spec["crosstrees_z"]
    d = mast.spec["diameter"]
    width = _top_width(p)
    top_centre = mast.at(top_z) + Vector((0.0, 0.0, TOP_THICKNESS / 2))
    axis_box(parts, top_centre, Vector((width, width * TOP_LENGTH, TOP_THICKNESS)), "Spar")
    _rim(parts, top_centre + Vector((0.0, 0.0, (TOP_THICKNESS + TOP_RIM.z) / 2)), width, width * TOP_LENGTH)
    cross_centre = mast.at(cross_z)
    axis_box(parts, cross_centre, Vector((width * CROSSTREES_OF_TOP, 0.4, CROSSTREES_THICKNESS)), "Spar")

    for side in (1.0, -1.0):
        plates = _chainplates(p, mast, side)
        t = _station(p, plates[0].y)
        channel_start = Vector((side * side_at(p, t, plates[0].z), plates[0].y + 0.5, plates[0].z))
        channel_end = Vector((side * (side_at(p, t, plates[0].z) + CHANNEL_OUT), plates[-1].y - 0.5, plates[0].z + CHANNEL_THICKNESS))
        axis_box(body, (channel_start + channel_end) / 2,
                 Vector((CHANNEL_OUT, abs(channel_end.y - channel_start.y), CHANNEL_THICKNESS)), "ShipWood")

        head = mast.at(top_z) + Vector((side * d * 0.4, 0.0, 0.0))
        lower = [(plate, head) for plate in plates]
        for low, high in lower:
            _rope(parts, "shroud", low, high, SHROUD)
        _ratlines(parts, lower, plates[0].z + 0.6, top_z - 1.0)

        edge = top_centre + Vector((side * width / 2, -width * TOP_LENGTH * TOPMAST_SHROUDS_AFT, 0.0))
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


def _y_at(p: dict, t: float) -> float:
    return (t - 0.5) * p["length"]


def _under_bowsprit(body: MeshParts, p: dict) -> None:
    """The bobstay from the bowsprit down to the stem near the water, and the dolphin striker hanging
    under the bowsprit with its martingale stay from the bowsprit's end and back ropes to either bow."""
    heel, end = bowsprit(p)
    stem_y = p["length"] / 2
    _rope(body, "bobstay", heel.lerp(end, BOBSTAY_AT), Vector((0.0, stem_y + CUTWATER_AHEAD, BOBSTAY_FOOT)), SHROUD)
    top = heel.lerp(end, STRIKER_AT)
    foot = top - Vector((0.0, 0.0, (end - heel).length * STRIKER_OF_BOWSPRIT))
    d = p["bowsprit"]["diameter"] * STRIKER_OF_DIAMETER
    pole(body, top, foot, d, d * 0.7, 6, "Spar")
    under = heel.lerp(end, MARTINGALE_AT) - Vector((0.0, 0.0, p["bowsprit"]["diameter"] * MARTINGALE_UNDER))
    _rope(body, "martingale stay", under, foot, SHROUD)
    t = BACK_ROPES_AT
    z = rail_height(p, t) - BACK_ROPES_UNDER_RAIL
    for side in (1.0, -1.0):
        _rope(body, "martingale back rope", foot, Vector((side * side_at(p, t, z), _y_at(p, t), z)), SHROUD)


def standing(body: MeshParts, per_mast: list[MeshParts], p: dict, masts: list[Mast]) -> None:
    """Tops, channels, shrouds and ratlines, backstays, the stays between masts and to the bowsprit,
    and the bowsprit's own from under it."""
    for parts, mast in zip(per_mast, masts, strict=True):
        _mast_rigging(body, parts, p, mast)
    _under_bowsprit(body, p)
    stem, _ = bowsprit(p)
    by_place = sorted(range(len(masts)), key=lambda i: masts[i].foot.y, reverse=True)
    fore = masts[by_place[0]]
    if not any(spec["kind"] == "jib" and spec["tack"] == 0.0 for spec in p["sails"]):
        _rope(per_mast[by_place[0]], "forestay", fore.at(fore.spec["top_z"]), stem, SHROUD)
    hung = {spec["mast"]: sail_name(i, spec) for i, spec in enumerate(p["sails"]) if spec["kind"] == "staysail"}
    for a, b in zip(by_place[1:], by_place[:-1]):
        after, before = masts[a], masts[b]
        top, foot = stay_ends(masts, after)
        _rope(per_mast[a], "stay", top, foot, SHROUD, hung.get(a))
        _rope(per_mast[a], "topmast stay", after.head, before.at(before.spec["crosstrees_z"]), SHROUD)
    for i, spec in enumerate(p["sails"]):
        if spec["kind"] == "jib":
            tack, _, head = jib_corners(p, masts[spec["mast"]], spec, 0.0)
            _rope(per_mast[spec["mast"]], "jib stay", head, tack, SHROUD, sail_name(i, spec))


def running(per_mast: list[MeshParts], p: dict, masts: list[Mast], trim: Trim, sails_set: bool) -> None:
    """Braces and lifts on every yard, running backstays; with the sails set, the sheets from their clews.
    A type with lifts_under_sails false drops, while its sails are set, the lifts of a yard a sail above is
    sheeted to: on close-set yards they would run up through that sail."""
    for m, (parts, mast) in enumerate(zip(per_mast, masts, strict=True)):
        ahead = min((other for other in masts if other.foot.y > mast.foot.y), key=lambda other: other.foot.y, default=None)
        sheeted = {spec["bottom"] for spec in p["sails"]
                   if spec["kind"] == "square" and spec["mast"] == m and isinstance(spec["bottom"], int)}
        for k, yard in enumerate(mast.spec["yards"]):
            port_end, starboard_end = yard_ends(mast, yard, trim.brace_deg)
            lifted = not (sails_set and k in sheeted and not p.get("lifts_under_sails", True))
            lift_to = mast.at(min(mast.spec["height"], yard["z"] + yard["length"] / 2 * LIFT_SLOPE))
            for end, side in ((port_end, -1.0), (starboard_end, 1.0)):
                lead = ahead.at(min(yard["z"], ahead.spec["height"])) if ahead else _deck_edge(p, side, end.y - BRACE_LEAD_AFT)
                _rope(parts, "brace", end, lead, RUNNING)
                if lifted:
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
        parts = per_mast[spec["mast"]]
        if spec["kind"] == "jib":
            _, clew, _ = jib_corners(p, mast, spec, trim.jib_deg)
            side = 1.0 if clew.x >= 0 else -1.0
            _rope(parts, "jib sheet", clew, _deck_edge(p, side, min(clew.y - JIB_SHEET_AFT, fore_y)), RUNNING, name)
        elif spec["kind"] == "staysail":
            _, clew, _ = staysail_corners(masts, mast, spec, trim.jib_deg)
            side = 1.0 if clew.x >= 0 else -1.0
            _rope(parts, "staysail sheet", clew, _deck_edge(p, side, clew.y), RUNNING, name)
        elif spec["kind"] == "square" and not isinstance(spec["bottom"], int):
            foot_l, foot_r, _, _ = square_corners(mast, spec, trim.brace_deg)
            for clew, side in ((foot_l, -1.0), (foot_r, 1.0)):
                _rope(parts, "course sheet", clew, _deck_edge(p, side, clew.y - COURSE_SHEET_AFT), RUNNING, name)
