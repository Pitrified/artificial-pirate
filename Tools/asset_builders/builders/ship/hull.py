"""The hull: stations lofted from bow to stern, each a loop from the deck over the rail to the keel."""

from __future__ import annotations

import math

from mathutils import Vector

from builders.ship.mesh import MeshParts, box

STATIONS = 20
UNDERWATER_POINTS = 5
BILGE_POWER = 2.5
STEM_HALF_WIDTH = 0.12
BULWARK_THICKNESS = 0.15
FOREFOOT = 0.75

# Which slot each band of the loop takes, between consecutive points of the starboard half.
_SECTION_SLOTS = ("ShipWood", "Trim", "Trim", "Topsides", "HullBottom") + ("HullBottom",) * UNDERWATER_POINTS
_SMOOTH_SLOTS = ("Topsides", "HullBottom")
_SHARP_POINTS = (0, 1, 2)


def rail_height(p: dict, t: float) -> float:
    """The rail above the water at t, a fraction of the length from the stern."""
    low = p["sheer_low"]
    if t >= low:
        return p["freeboard"] + p["sheer_bow"] * ((t - low) / (1 - low)) ** 2
    return p["freeboard"] + p["sheer_stern"] * ((low - t) / low) ** 2


def deck_height(p: dict, t: float) -> float:
    return rail_height(p, t) - p["bulwark"]


def half_beam(p: dict, t: float) -> float:
    widest = p["widest"]
    if t >= widest:
        fraction = 1 - ((t - widest) / (1 - widest)) ** 2
    else:
        fraction = 1 - (1 - p["transom"]) * ((widest - t) / widest) ** 2.5
    return max(p["beam"] / 2 * fraction, STEM_HALF_WIDTH)


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
    b = half_beam(p, t)
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

    starboard = [
        (inner, deck),
        (inner, rail),
        (b, rail),
        (b, rail - p["band"]),
        (b, p["boot"]),
        (b, 0.0),
    ]
    for k in range(1, UNDERWATER_POINTS + 1):
        s = k / UNDERWATER_POINTS
        x = b * (1 - s**BILGE_POWER) ** (1 / BILGE_POWER) * (1 - (1 - full) * s)
        starboard.append((x, -depth * s))
    port = [(-x, z) for x, z in reversed(starboard[:-1])]

    rake = math.tan(math.radians(p["transom_rake_deg"])) if t == 0 else 0.0
    return [Vector((x, y - z * rake, z)) for x, z in starboard + port]


def hull(parts: MeshParts, p: dict) -> None:
    sections = [_section(p, i / STATIONS) for i in range(STATIONS + 1)]
    loops = [parts.add_verts(section, [(v.y, v.z) for v in section]) for section in sections]
    n = len(sections[0])
    # Starboard's bands and port's mirrored; the deck between them is its own surface (fittings.deck).
    slots = (*_SECTION_SLOTS, *reversed(_SECTION_SLOTS))

    for i in range(STATIONS):
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


def rudder(parts: MeshParts, p: dict) -> None:
    y = -p["length"] / 2
    depth = p["draft"] * 0.9
    chord = p["draft"] * 0.4
    half = 0.1
    corners = [Vector((x, y - dy, z)) for z in (-depth, p["boot"]) for dy in (0.0, chord) for x in (-half, half)]
    box(parts, corners, "HullBottom")
