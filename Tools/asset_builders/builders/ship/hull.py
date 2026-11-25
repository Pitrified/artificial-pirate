"""The hull: stations lofted from bow to stern, each a loop from the deck over the rail to the keel."""

from __future__ import annotations

import math

from mathutils import Vector

from builders.ship.mesh import MeshParts, box

STATIONS = 20
END_SPLITS = 3  # extra slices within the hull's last at each end
UNDERWATER_POINTS = 5
TOPSIDE_POINTS = 4  # between the boot and the band, so a tumblehome curves
TUMBLE_FROM = 0.3  # of the freeboard: the height the sides start to fall in from
BILGE_POWER = 2.5
STEM_HALF_WIDTH = 0.12
BULWARK_THICKNESS = 0.15
FOREFOOT = 0.75
ENTRY = 2.0
RUN = 2.5
COUNTER_POWER = 0.5  # how quickly a counter stern swells from the waterline's width to the rail's  # the power the half beam falls off with toward the transom: higher keeps it full, then rounds it  # the power the half beam falls off with toward the stem: lower is a finer bow
OVERHANG_REACH = 0.14  # of the length: how far in from each end an overhang starts
OVERHANG_BOW_FROM = 0.5  # of the draft below the water: where the stem starts to lean forward
OVERHANG_BOW_POWER = 1.5  # the stem's curve
OVERHANG_STERN_POWER = 0.6  # the counter's: out quickly above the water
BREAK = 0.004  # of the length, over which a raised deck's edge rises from the deck below
SILL = 0.25  # m above the deck: the hull's top where the bulwark gives way to an open rail

# Which slot each band of the loop takes, between consecutive points of the starboard half.
_SECTION_SLOTS = ("ShipWood", "Trim", "Trim") + ("Topsides",) * (TOPSIDE_POINTS + 1) + ("HullBottom",) * (UNDERWATER_POINTS + 1)
_SMOOTH_SLOTS = ("Topsides", "HullBottom")
_SHARP_POINTS = (0, 1, 2)


def sheer(p: dict, t: float) -> float:
    """How far the rail rises above the freeboard at t, a fraction of the length from the stern."""
    low = p["sheer_low"]
    if t >= low:
        return p["sheer_bow"] * ((t - low) / (1 - low)) ** 2
    return p["sheer_stern"] * ((low - t) / low) ** 2


def rail_height(p: dict, t: float) -> float:
    """The rail above the water at t: the sheer, and a raised deck's height over it."""
    return p["freeboard"] + sheer(p, t) + _raised(p, t)


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


def overhang(p: dict, v: Vector) -> Vector:
    """A point of the ship moved by the bow's and the stern's overhangs (bow_overhang, stern_overhang:
    metres at the rail): forward of the stem's foot and aft of the counter's, more the higher it is, and
    everything above the rail by the whole of it, so the bowsprit and the taffrail go with the hull."""
    half = p["length"] / 2
    start = half - OVERHANG_REACH * p["length"]
    reach = abs(v.y)
    if reach <= start:
        return v
    w = min(1.0, (reach - start) / (half - start))
    w = w * w * (3 - 2 * w)
    if v.y > 0:
        low = -p["draft"] * OVERHANG_BOW_FROM
        h = min(1.0, max(0.0, (v.z - low) / (rail_height(p, 1.0) - low))) ** OVERHANG_BOW_POWER
        return Vector((v.x, v.y + p.get("bow_overhang", 0.0) * w * h, v.z))
    h = min(1.0, max(0.0, v.z / rail_height(p, 0.0))) ** OVERHANG_STERN_POWER
    return Vector((v.x, v.y - p.get("stern_overhang", 0.0) * w * h, v.z))


def stations(p: dict) -> list[float]:
    """Where the hull is lofted along its length, from the stern: evenly, closer within the last slice at
    each end, and either side of each raised deck's step so it stands near upright."""
    count = p.get("stations", STATIONS)
    ts = {i / count for i in range(count + 1)}
    # The ends curve fastest, and an overhang stretches them further: split their last slices.
    for k in range(1, END_SPLITS + 1):
        ts.update((k / (END_SPLITS + 1) / count, 1 - k / (END_SPLITS + 1) / count))
    for deck in p.get("raised_decks", []):
        for edge in (deck["from"], deck["to"]):
            if 0.0 < edge < 1.0:
                ts.update((edge - BREAK / 2, edge + BREAK / 2))
    return sorted(ts)


def deck_height(p: dict, t: float) -> float:
    return rail_height(p, t) - p["bulwark"]


class OpenRailError(ValueError):
    """A type's painted band reaches above the sill of its open rail."""


def is_open(p: dict, t: float) -> bool:
    """Whether the rail at t stands open on posts over a sill (open_rail: [from, to]) rather than on a
    closed bulwark."""
    span = p.get("open_rail")
    return bool(span) and span[0] < t < span[1]


def hull_top(p: dict, t: float) -> float:
    """Where the hull's side ends at t: the rail over a bulwark, the sill under an open rail."""
    return deck_height(p, t) + SILL if is_open(p, t) else rail_height(p, t)


def _plan(p: dict, t: float, transom: float) -> float:
    """The half beam along the length for a transom transom wide over the beam."""
    widest = p["widest"]
    if t >= widest:
        fraction = 1 - ((t - widest) / (1 - widest)) ** p.get("entry", ENTRY)
    else:
        fraction = 1 - (1 - transom) * ((widest - t) / widest) ** p.get("run", RUN)
    return max(p["beam"] / 2 * fraction, STEM_HALF_WIDTH)


def half_beam(p: dict, t: float) -> float:
    """The half beam at the waterline at t: narrower aft than at the rail where the type has a counter
    (transom_water under transom)."""
    return _plan(p, t, p.get("transom_water", p["transom"]))


def side(p: dict, t: float, z: float) -> float:
    """The hull's side at t and height z above the water, from the centreline: the waterline's half beam
    swelling to the rail's above it (a counter stern), less the tumblehome as the side falls in above
    TUMBLE_FROM of the freeboard toward the rail."""
    b = half_beam(p, t)
    if z <= 0.0:
        return b
    rail = rail_height(p, t)
    up = min(1.0, z / rail) ** COUNTER_POWER
    b += (_plan(p, t, p["transom"]) - b) * up
    tumble = p.get("tumblehome", 0.0)
    low = p["freeboard"] * TUMBLE_FROM
    if tumble <= 0.0 or z <= low:
        return b
    s = min(1.0, (z - low) / (rail - low))
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


def _section(p: dict, t: float, opened: bool) -> list[Vector]:
    """One station's outline, a closed loop: deck to rail (or the sill, opened) to waterline to keel, both sides."""
    y = (t - 0.5) * p["length"]
    b = half_beam(p, t)
    rail = rail_height(p, t)
    deck = rail - p["bulwark"]
    top = deck + SILL if opened else rail
    inner = bulwark_inner(p, t)
    depth = _depth(p, t)
    full = _fullness(p, t)

    band = rail - p["band"]
    if band >= top:
        raise OpenRailError(f"{p['name']}: the band's foot at {band:.2f} m is above the hull's top at {top:.2f} m (t {t:.3f})")
    starboard = [
        (inner, deck),
        (inner, top),
        (side(p, t, top), top),
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


def _lofted(p: dict) -> list[tuple[float, bool]]:
    """The stations with whether each is open; each end of the open rail twice, closed and open, so the
    bulwark ends in an upright face there."""
    span = p.get("open_rail")
    ts = sorted(set(stations(p)) | set(span or ()))
    lofted = []
    for t in ts:
        if span and t == span[0]:
            lofted += [(t, False), (t, True)]
        elif span and t == span[1]:
            lofted += [(t, True), (t, False)]
        else:
            lofted.append((t, is_open(p, t)))
    return lofted


def hull(parts: MeshParts, p: dict) -> None:
    lofted = _lofted(p)
    sections = [_section(p, t, opened) for t, opened in lofted]
    loops = [parts.add_verts(section, [(v.y, v.z) for v in section]) for section in sections]
    n = len(sections[0])
    # Starboard's bands and port's mirrored; the deck between them is its own surface (fittings.deck).
    slots = (*_SECTION_SLOTS, *reversed(_SECTION_SLOTS))

    for i in range(len(sections) - 1):
        a, b = loops[i], loops[i + 1]
        if lofted[i][0] == lofted[i + 1][0]:
            # The bulwark's end: only the rail's top band sweeps down to the sill, upright; the rest coincide.
            for k in (1, n - 3):
                parts.add_face((a + k, a + k + 1, b + k + 1, b + k), "ShipWood")
            continue
        for k in range(n - 1):
            parts.add_face((a + k, a + k + 1, b + k + 1, b + k), slots[k], slots[k] in _SMOOTH_SLOTS)
        for k in _SHARP_POINTS + tuple(n - 1 - k for k in _SHARP_POINTS):
            parts.mark_sharp(a + k, b + k)

    stern, bow = loops[0], loops[-1]
    if "transom_slot" in p:
        # Painted from the band up, the hull's paint below it: the band is the starboard half's fourth point.
        upper = (3, 2, 1, 0, n - 1, n - 2, n - 3, n - 4)
        parts.add_face(tuple(stern + k for k in upper), p["transom_slot"])
        parts.add_face(tuple(stern + k for k in range(n - 4, 2, -1)), "Topsides")
    else:
        parts.add_face(tuple(stern + k for k in reversed(range(n))), "Topsides")
    parts.add_face(tuple(bow + k for k in range(n)), "Topsides")
    for k in range(n):
        parts.mark_sharp(stern + k, stern + (k + 1) % n)
        parts.mark_sharp(bow + k, bow + (k + 1) % n)
    _transom_board(parts, stern, n, p.get("transom_slot", "Topsides"))


def _transom_board(parts: MeshParts, stern: int, n: int, slot: str) -> None:
    """The transom between the bulwarks from the deck up to the rail, which the stern's loop leaves open:
    its face aft, a top, and planking inside."""
    face = (stern, stern + 1, stern + n - 2, stern + n - 1)
    parts.add_planar_face(face, slot)
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
