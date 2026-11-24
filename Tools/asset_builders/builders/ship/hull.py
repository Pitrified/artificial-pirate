"""The hull: stations lofted from bow to stern, each a loop from the deck over the rail to the keel."""

from __future__ import annotations

import math

from mathutils import Vector

from builders.ship.mesh import MeshParts, box

STATIONS = 20
UNDERWATER_POINTS = 5
TOPSIDE_POINTS = 4  # between the boot and the band, so a tumblehome curves
TUMBLE_FROM = 0.3  # of the freeboard: the height the sides start to fall in from
BILGE_POWER = 2.5
STEM_HALF_WIDTH = 0.12
BULWARK_THICKNESS = 0.15
FOREFOOT = 0.75
BREAK = 0.004  # of the length, over which a raised deck's edge rises from the deck below

# Which slot each band of the loop takes, between consecutive points of the starboard half.
_SECTION_SLOTS = ("ShipWood", "Trim", "Trim") + ("Topsides",) * (TOPSIDE_POINTS + 1) + ("HullBottom",) * (UNDERWATER_POINTS + 1)
_SMOOTH_SLOTS = ("Topsides", "HullBottom")
_SHARP_POINTS = (0, 1, 2)


def rail_height(p: dict, t: float) -> float:
    """The rail above the water at t, a fraction of the length from the stern: the sheer, and a raised
    deck's height over it."""
    low = p["sheer_low"]
    if t >= low:
        sheer = p["sheer_bow"] * ((t - low) / (1 - low)) ** 2
    else:
        sheer = p["sheer_stern"] * ((low - t) / low) ** 2
    return p["freeboard"] + sheer + _raised(p, t)


def _raised(p: dict, t: float) -> float:
    """How far a raised deck (a quarterdeck or poop aft, a forecastle forward) lifts the deck and the rail
    at t: its height between its ends, stepping up within BREAK of an end that is not the hull's own."""
    up = 0.0
    for deck in p.get("raised_decks", []):
        start, end = deck["from"], deck["to"]
        rise = 1.0 if start <= 0.0 else min(1.0, max(0.0, (t - start) / BREAK + 0.5))
        fall = 1.0 if end >= 1.0 else min(1.0, max(0.0, (end - t) / BREAK + 0.5))
        up += deck["height"] * min(rise, fall)
    return up


def stations(p: dict) -> list[float]:
    """Where the hull is lofted along its length, from the stern: evenly, and either side of each raised
    deck's step so it stands near upright."""
    count = p.get("stations", STATIONS)
    ts = {i / count for i in range(count + 1)}
    for deck in p.get("raised_decks", []):
        for edge in (deck["from"], deck["to"]):
            if 0.0 < edge < 1.0:
                ts.update((edge - BREAK / 2, edge + BREAK / 2))
    return sorted(ts)


def deck_height(p: dict, t: float) -> float:
    return rail_height(p, t) - p["bulwark"]


def half_beam(p: dict, t: float) -> float:
    widest = p["widest"]
    if t >= widest:
        fraction = 1 - ((t - widest) / (1 - widest)) ** 2
    else:
        fraction = 1 - (1 - p["transom"]) * ((widest - t) / widest) ** 2.5
    return max(p["beam"] / 2 * fraction, STEM_HALF_WIDTH)


def side(p: dict, t: float, z: float) -> float:
    """The hull's side at t and height z above the water, from the centreline: the half beam, less the
    tumblehome as the side falls in above TUMBLE_FROM of the freeboard toward the rail."""
    b = half_beam(p, t)
    tumble = p.get("tumblehome", 0.0)
    low = p["freeboard"] * TUMBLE_FROM
    if tumble <= 0.0 or z <= low:
        return b
    s = min(1.0, (z - low) / (rail_height(p, t) - low))
    return max(b - tumble * b * s * s, STEM_HALF_WIDTH)


def _fullness(p: dict, t: float) -> float:
    """How full the underwater section is: 1 amidships, a V toward the ends."""
    widest = p["widest"]
    u = (t - widest) / (1 - widest) if t >= widest else (widest - t) / widest
    return 1 - 0.75 * u**2


def _depth(p: dict, t: float) -> float:
    """The keel below the water; it sweeps up to the stem at the forefoot."""
    if t <= FOREFOOT:
        return p["draft"]
    return p["draft"] * (1 - ((t - FOREFOOT) / (1 - FOREFOOT)) ** 2)


def bulwark_inner(p: dict, t: float) -> float:
    """The inside of the bulwark from the centreline, where the deck meets it."""
    b = side(p, t, rail_height(p, t))
    return max(b - BULWARK_THICKNESS, b * 0.5)


def transom_y(p: dict, z: float) -> float:
    """The transom's face at height z: raked aft as it rises."""
    return -p["length"] / 2 - z * math.tan(math.radians(p["transom_rake_deg"]))


def _section(p: dict, t: float) -> list[Vector]:
    """One station's outline, a closed loop: deck to rail to waterline to keel, both sides."""
    y = (t - 0.5) * p["length"]
    b = half_beam(p, t)
    rail = rail_height(p, t)
    deck = rail - p["bulwark"]
    inner = bulwark_inner(p, t)
    depth = _depth(p, t)
    full = _fullness(p, t)

    band = rail - p["band"]
    starboard = [
        (inner, deck),
        (inner, rail),
        (side(p, t, rail), rail),
        (side(p, t, band), band),
    ]
    for k in range(1, TOPSIDE_POINTS + 1):
        z = band + (p["boot"] - band) * k / (TOPSIDE_POINTS + 1)
        starboard.append((side(p, t, z), z))
    starboard += [(b, p["boot"]), (b, 0.0)]
    for k in range(1, UNDERWATER_POINTS + 1):
        s = k / UNDERWATER_POINTS
        x = b * (1 - s**BILGE_POWER) ** (1 / BILGE_POWER) * (1 - (1 - full) * s)
        starboard.append((x, -depth * s))
    port = [(-x, z) for x, z in reversed(starboard[:-1])]

    rake = math.tan(math.radians(p["transom_rake_deg"])) if t == 0 else 0.0
    return [Vector((x, y - z * rake, z)) for x, z in starboard + port]


def hull(parts: MeshParts, p: dict) -> None:
    sections = [_section(p, t) for t in stations(p)]
    loops = [parts.add_verts(section, [(v.y, v.z) for v in section]) for section in sections]
    n = len(sections[0])
    # Starboard's bands and port's mirrored; the deck between them is its own surface (fittings.deck).
    slots = (*_SECTION_SLOTS, *reversed(_SECTION_SLOTS))

    for i in range(len(sections) - 1):
        a, b = loops[i], loops[i + 1]
        for k in range(n - 1):
            parts.add_face((a + k, a + k + 1, b + k + 1, b + k), slots[k], slots[k] in _SMOOTH_SLOTS)
        for k in _SHARP_POINTS + tuple(n - 1 - k for k in _SHARP_POINTS):
            parts.mark_sharp(a + k, b + k)

    stern, bow = loops[0], loops[-1]
    parts.add_face(tuple(stern + k for k in reversed(range(n))), "Topsides")
    parts.add_face(tuple(bow + k for k in range(n)), "Topsides")
    for k in range(n):
        parts.mark_sharp(stern + k, stern + (k + 1) % n)
        parts.mark_sharp(bow + k, bow + (k + 1) % n)
    _transom_board(parts, stern, n)


def _transom_board(parts: MeshParts, stern: int, n: int) -> None:
    """The transom between the bulwarks from the deck up to the rail, which the stern's loop leaves open:
    its face aft, a top, and planking inside."""
    face = (stern, stern + 1, stern + n - 2, stern + n - 1)
    parts.add_planar_face(face, "Topsides")
    inside = parts.add_verts([parts.verts[i] + Vector((0.0, BULWARK_THICKNESS, 0.0)) for i in face])
    parts.add_planar_face(tuple(range(inside + 3, inside - 1, -1)), "ShipWood")
    parts.add_planar_face((face[1], inside + 1, inside + 2, face[2]), "Trim")
    for a, b in zip(face, (*face[1:], face[0])):
        parts.mark_sharp(a, b)


def rudder(parts: MeshParts, p: dict) -> None:
    y = -p["length"] / 2
    depth = p["draft"] * 0.9
    chord = p["draft"] * 0.4
    half = 0.1
    corners = [Vector((x, y - dy, z)) for z in (-depth, p["boot"]) for dy in (0.0, chord) for x in (-half, half)]
    box(parts, corners, "HullBottom")
