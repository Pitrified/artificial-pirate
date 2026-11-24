"""What a hull carries: the planked deck, cap rail and stanchions, wales, gun ports, stern windows and
taffrail, the cutwater, figurehead and head rails, and deck furniture.

Type keys (all but the deck optional):
- wales: heights of the wales' centres above the water;
- stripes: bands of the trim's paint along the hull's side, each [low, high] above the water;
- gun_ports: a row, or a list of rows, each {count (a side), from, to (fractions from the stern), size}
  and either above_deck (an open row: the centre over the deck there, where the gun's axis sits on its
  carriage) or z (a covered row, on a deck below: the centre above the water, the barrels' muzzles
  showing through the ports), and guns: false for a row of ports with no guns behind them;
- chase_ports, stern_ports: {across (each port's x from the centreline), z (centre above the water),
  size}: guns firing ahead through the bow, and astern through the transom, below its windows;
- stern_windows: how many across the transom;
- deck: {hatches: [[t, length, width]], companionway: which hatch is the way below (its index), capstan: t,
  helm: "wheel" or "tiller", helm_at: t}. The other hatches are gratings.
"""

from __future__ import annotations

import itertools
import math
from collections.abc import Callable
from dataclasses import dataclass

from mathutils import Vector

from builders.ship.hull import (
    BULWARK_THICKNESS,
    bulwark_inner,
    deck_height,
    half_beam,
    rail_height,
    stations,
    transom_y,
)
from builders.ship.hull import side as side_at
from builders.ship.mesh import MeshParts, axis_box, box, line, pole, ring

WALE_HEIGHT = 0.25
STRIPE_OUT = 0.02  # m, how far a painted stripe stands off the side, so it shows over the hull's own paint
WALE_OUT = 0.08
CAP_RAIL_HEIGHT = 0.1
CAP_RAIL_OVERHANG = 0.08
CAP_RAIL_CHAMFER = 0.03
STANCHION_STEP = 1.2
STANCHION = 0.12
TIMBERHEAD_EVERY = 3  # stanchions
TIMBERHEAD_PROUD = 0.15  # above the cap rail
PORT_DEPTH = 0.06
PORT_LID = 0.05
WINDOW_WIDTH = 0.5
WINDOW_HEIGHT = 0.7
WINDOW_FRAME = 0.08
WINDOW_FRAME_PROUD = 0.05
WINDOW_GLASS_PROUD = 0.07
TAFFRAIL_HEIGHT = 0.45
TAFFRAIL_RAIL = Vector((0.0, 0.16, 0.08))  # the cap and foot rails' depth and height
BALUSTER_STEP = 0.25
BALUSTER = 0.11  # at the bulge
CARVING_WIDTH = 1.2
QUARTER_RAIL_TO = 0.1  # where the quarter rails meet the cap rail, from the stern
HATCH_HEIGHT = 0.35
COAMING = 0.1  # board thickness
GRATING_BAR = 0.05
GRATING_STEP = 0.2
HOOD_HEIGHT = 0.8  # above the coaming, at the closed forward end
HOOD_FALL = 0.2  # how much lower the roof is at the open end
HOOD_ROOF = 0.06
LADDER_STEPS = 3
CAPSTAN_DIAMETER = 0.8
CAPSTAN_HEIGHT = 0.9
WHEEL_DIAMETER = 1.4  # to the handles' ends
WHEEL_RIM = 0.58  # radius
WHEEL_SPOKES = 8
WHEEL_PEDESTAL = 0.3  # forward of the wheel
TILLER_LENGTH = 2.5
TILLER_RISE = 0.25
RUDDER_HEAD = 0.26
FIGURE_FOR_LENGTH = 40.0  # m: a longer ship's figurehead and trailboards grow with it

Profile = Callable[[float], list[tuple[float, float]]]


def _y(p: dict, t: float) -> float:
    return (t - 0.5) * p["length"]


def _strip(parts: MeshParts, p: dict, profile: Profile, t0: float, t1: float, slot: str) -> None:
    """A small section lofted along the hull, both sides: its starboard points x, z, clockwise seen from astern."""
    ts = sorted({t0, t1} | {t for t in stations(p) if t0 < t < t1})
    for side in (1.0, -1.0):
        rows = []
        for t in ts:
            section = profile(t)
            rows.append(parts.add_verts([Vector((side * x, _y(p, t), z)) for x, z in section], _around(section, _y(p, t))))
        n = len(profile(ts[0]))
        for a, b in itertools.pairwise(rows):
            for k in range(n - 1):
                quad = (a + k, a + k + 1, b + k + 1, b + k)
                parts.add_face(quad if side > 0 else tuple(reversed(quad)), slot)


def _around(section: list[tuple[float, float]], y: float) -> list[tuple[float, float]]:
    """UVs in metres for a lofted section's points: u along the hull, v around the section."""
    v = [0.0]
    for (x0, z0), (x1, z1) in itertools.pairwise(section):
        v.append(v[-1] + math.hypot(x1 - x0, z1 - z0))
    return [(y, d) for d in v]


def deck(parts: MeshParts, p: dict) -> None:
    """The deck between the bulwarks, mapped along its length so planks can run fore and aft."""
    for t0, t1 in itertools.pairwise(stations(p)):
        corners = []
        for t, x_side in ((t0, 1.0), (t1, 1.0), (t1, -1.0), (t0, -1.0)):
            z = deck_height(p, t)
            y = transom_y(p, z) + BULWARK_THICKNESS if t == 0 else _y(p, t)
            corners.append(Vector((x_side * bulwark_inner(p, t), y, z)))
        s = parts.add_verts(corners, [(v.y, v.x) for v in corners])
        parts.add_face((s, s + 1, s + 2, s + 3), "Deck")


def rail(parts: MeshParts, p: dict) -> None:
    """A cap rail standing proud of the bulwark, and stanchions inside it, every few running up through it
    as a timberhead."""

    def cap(t: float) -> list[tuple[float, float]]:
        inner, outer, z = bulwark_inner(p, t) - CAP_RAIL_OVERHANG, side_at(p, t, rail_height(p, t)) + CAP_RAIL_OVERHANG, rail_height(p, t)
        top, c = z + CAP_RAIL_HEIGHT, CAP_RAIL_CHAMFER
        return [(inner, z), (inner, top - c), (inner + c, top), (outer - c, top), (outer, top - c), (outer, z)]

    _strip(parts, p, cap, 0.0, 1.0, "ShipWood")
    count = int(p["length"] / STANCHION_STEP)
    for i in range(1, count):
        t = i / count
        z0, z1 = deck_height(p, t), rail_height(p, t)
        if i % TIMBERHEAD_EVERY == 0:
            z1 += CAP_RAIL_HEIGHT + TIMBERHEAD_PROUD
        for side in (1.0, -1.0):
            x = side * (bulwark_inner(p, t) - STANCHION / 2)
            axis_box(parts, Vector((x, _y(p, t), (z0 + z1) / 2)), Vector((STANCHION, STANCHION, z1 - z0)), "ShipWood")


def wales(parts: MeshParts, p: dict) -> None:
    for centre in p.get("wales", []):
        z0, z1 = centre - WALE_HEIGHT / 2, centre + WALE_HEIGHT / 2

        def wale(t: float, z0: float = z0, z1: float = z1) -> list[tuple[float, float]]:
            b1, b0 = side_at(p, t, z1), side_at(p, t, z0)
            return [(b1, z1), (b1 + WALE_OUT, z1), (b0 + WALE_OUT, z0), (b0, z0)]

        _strip(parts, p, wale, 0.03, 0.97, "Tar")


def stripes(parts: MeshParts, p: dict) -> None:
    for low, high in p.get("stripes", []):

        def stripe(t: float, low: float = low, high: float = high) -> list[tuple[float, float]]:
            return [(side_at(p, t, high) + STRIPE_OUT, high), (side_at(p, t, low) + STRIPE_OUT, low)]

        _strip(parts, p, stripe, 0.02, 0.98, "Trim")


@dataclass(frozen=True)
class Port:
    """A gun port: its station t, side (+1 starboard, -1 port, 0 the bow or the stern), centre on the hull,
    size, the way its gun fires, whether the gun stands on an open deck (on a carriage), and whether
    there is a gun behind it at all."""

    t: float
    side: float
    centre: Vector
    size: float
    out: Vector
    open: bool
    armed: bool = True


def _rows(p: dict) -> list[dict]:
    spec = p.get("gun_ports")
    if not spec:
        return []
    return spec if isinstance(spec, list) else [spec]


def gun_port_centres(p: dict) -> list[Port]:
    """Each broadside port, row by row, stern to bow, starboard then port at each station."""
    ports = []
    for row in _rows(p):
        for i in range(row["count"]):
            t = row["from"] + (row["to"] - row["from"]) * i / max(1, row["count"] - 1)
            is_open = "above_deck" in row
            z = deck_height(p, t) + row["above_deck"] if is_open else row["z"]
            for side in (1.0, -1.0):
                centre = Vector((side * side_at(p, t, z), _y(p, t), z))
                ports.append(Port(t, side, centre, row["size"], Vector((side, 0.0, 0.0)), is_open, row.get("guns", True)))
    return ports


def _bow_t(p: dict, x: float) -> float:
    """The station near the bow where the hull's side is x from the centreline."""
    low, high = p["widest"], 1.0
    for _ in range(40):
        mid = (low + high) / 2
        low, high = (mid, high) if half_beam(p, mid) > x else (low, mid)
    return low


def chase_port_centres(p: dict) -> list[Port]:
    """The bow chasers' ports through the bow, then the stern guns' through the transom, each across in turn."""
    ports = []
    spec = p.get("chase_ports")
    if spec:
        for x in spec["across"]:
            t = _bow_t(p, abs(x))
            ports.append(Port(t, 0.0, Vector((x, _y(p, t), spec["z"])), spec["size"], Vector((0.0, 1.0, 0.0)), False))
    spec = p.get("stern_ports")
    if spec:
        for x in spec["across"]:
            ports.append(Port(0.0, 0.0, Vector((x, transom_y(p, spec["z"]), spec["z"])), spec["size"], Vector((0.0, -1.0, 0.0)),
                              False))
    return ports


def gun_ports(parts: MeshParts, p: dict) -> None:
    for port in gun_port_centres(p):
        side, size = port.side, port.size
        hole = port.centre + Vector((side * PORT_DEPTH / 2, 0.0, 0.0))
        axis_box(parts, hole, Vector((PORT_DEPTH, size, size)), "Tar")
        lid = hole + Vector((side * size / 2, 0.0, size / 2 + PORT_LID))
        axis_box(parts, lid, Vector((size, size, PORT_LID)), "Trim")
    for port in chase_port_centres(p):
        if port.out.y > 0:
            axis_box(parts, port.centre, Vector((port.size, PORT_DEPTH * 4, port.size)), "Tar")
        else:
            _on_transom(parts, p, port.centre.x, port.centre.z, Vector((port.size, PORT_DEPTH, port.size)), "Tar")


def stern(parts: MeshParts, p: dict) -> None:
    """Windows across the transom between the deck and the rail, and a carved taffrail above it."""
    count = p.get("stern_windows", 0)
    rail_z, deck_z = rail_height(p, 0.0), deck_height(p, 0.0)
    z = (rail_z + deck_z) / 2
    span = half_beam(p, 0.0) * 1.2
    for i in range(count):
        x = -span / 2 + span * (i + 0.5) / count
        _on_transom(parts, p, x, z, Vector((WINDOW_WIDTH + WINDOW_FRAME * 2, WINDOW_FRAME_PROUD, WINDOW_HEIGHT + WINDOW_FRAME * 2)),
                    "Gilt")
        _on_transom(parts, p, x, z, Vector((WINDOW_WIDTH, WINDOW_GLASS_PROUD, WINDOW_HEIGHT)), "Tar")
    taffrail(parts, p)


def _on_transom(parts: MeshParts, p: dict, x: float, z: float, size: Vector, slot: str) -> None:
    """A box laid on the raked transom, centred at x and z: size.x across, size.z high, standing
    size.y proud of it."""
    corners = [Vector((x + dx, transom_y(p, zc) + dy, zc))
               for zc in (z - size.z / 2, z + size.z / 2)
               for dy in (-size.y, 0.0)
               for dx in (-size.x / 2, size.x / 2)]
    box(parts, corners, slot)


def taffrail(parts: MeshParts, p: dict) -> None:
    """An open rail across the stern: turned balusters between a foot rail and a gilt cap rail, a carved
    board in the middle, scrolls at the ends, and quarter rails from its ends down to the cap rail."""
    rail_z = rail_height(p, 0.0)
    top = rail_z + TAFFRAIL_HEIGHT
    y = transom_y(p, top) + 0.1
    width = side_at(p, 0.0, rail_z) * 1.8
    rails = Vector((width, TAFFRAIL_RAIL.y, TAFFRAIL_RAIL.z))
    axis_box(parts, Vector((0.0, y, top - TAFFRAIL_RAIL.z / 2)), rails, "Gilt")
    axis_box(parts, Vector((0.0, y, rail_z + TAFFRAIL_RAIL.z / 2)), rails, "Trim")
    low, high = rail_z + TAFFRAIL_RAIL.z, top - TAFFRAIL_RAIL.z
    count = round(width / BALUSTER_STEP)
    for k in range(count + 1):
        x = -width / 2 + width * k / count
        if abs(x) > CARVING_WIDTH / 2:
            _baluster(parts, Vector((x, y, low)), high - low)
    axis_box(parts, Vector((0.0, y, (low + high) / 2)), Vector((CARVING_WIDTH, 0.08, high - low)), "Gilt")
    pole(parts, Vector((0.0, y - 0.04, (low + high) / 2)), Vector((0.0, y - 0.1, (low + high) / 2)), high - low, (high - low) * 0.8, 12,
         "Gilt", smooth=True)
    for side in (1.0, -1.0):
        end = Vector((side * width / 2, y, top - TAFFRAIL_RAIL.z))
        ring(parts, end + Vector((side * 0.08, 0.0, 0.0)), Vector((0.0, 1.0, 0.0)), 0.1, 0.06, 10, "Gilt")
        t = QUARTER_RAIL_TO
        forward = Vector((side * side_at(p, t, rail_height(p, t)), _y(p, t), rail_height(p, t) + CAP_RAIL_HEIGHT))
        pole(parts, end, forward, 0.1, 0.1, 8, "Gilt", smooth=True)
        _baluster(parts, Vector(((end.x + forward.x) / 2, (end.y + forward.y) / 2, rail_height(p, t / 2))),
                  (end.z + forward.z) / 2 - rail_height(p, t / 2))


def _baluster(parts: MeshParts, foot: Vector, height: float) -> None:
    """A turned post: narrow at the foot, a bulge low down, a neck under the rail."""
    bulge = foot + Vector((0.0, 0.0, height * 0.35))
    pole(parts, foot, bulge, BALUSTER * 0.55, BALUSTER, 8, "Trim", smooth=True)
    pole(parts, bulge, foot + Vector((0.0, 0.0, height)), BALUSTER, BALUSTER * 0.45, 8, "Trim", smooth=True)


def bow(parts: MeshParts, p: dict) -> None:
    """A cutwater forward of the stem, a figurehead on it leaning out, and head rails back to the bow."""
    stem_y = p["length"] / 2
    top = rail_height(p, 1.0) - 0.4
    reach = p["length"] * 0.045
    corners = [Vector((x, y, z))
               for z, ys in ((0.4, (stem_y - 0.3, stem_y + 0.3)), (top, (stem_y - 0.3, stem_y + reach)))
               for y in ys for x in (-0.12, 0.12)]
    box(parts, corners, "Trim")

    k = max(1.0, p["length"] / FIGURE_FOR_LENGTH)
    foot = Vector((0.0, stem_y + reach - 0.2 * k, top - 0.3 * k))
    body_top = foot + Vector((0.0, 0.5, 1.1)) * k
    pole(parts, foot, body_top, 0.5 * k, 0.35 * k, 6, "Gilt")
    pole(parts, body_top, body_top + Vector((0.0, 0.15, 0.4)) * k, 0.35 * k, 0.3 * k, 6, "Gilt")
    for side in (1.0, -1.0):
        board = [Vector((side * 0.13, y, z)) for y, z in ((stem_y + 0.2, top - 0.4 * k), (stem_y + reach * 0.8, top - 0.4 * k),
                                                           (stem_y + reach * 0.8, top - 0.9 * k), (stem_y + 0.2, top - 1.4 * k))]
        s = parts.add_verts(board)
        parts.add_face((s, s + 1, s + 2, s + 3) if side > 0 else (s + 3, s + 2, s + 1, s), "Gilt")

    for side in (1.0, -1.0):
        aft = Vector((side * side_at(p, 0.93, rail_height(p, 0.93)), stem_y - 0.07 * p["length"], rail_height(p, 0.93) - 0.3))
        for drop in (0.0, 0.5):
            line(parts, foot + Vector((side * 0.15, 0.0, 0.2 - drop)), aft - Vector((0.0, 0.0, drop)), 0.12, "Trim")


def furniture(parts: MeshParts, p: dict) -> None:
    spec = p.get("deck")
    if not spec:
        return
    for i, (t, length, width) in enumerate(spec.get("hatches", [])):
        base = Vector((0.0, _y(p, t), deck_height(p, t)))
        coaming(parts, base, length, width)
        if i == spec.get("companionway"):
            companionway(parts, base, length, width)
        else:
            grating(parts, base, length, width)
    if "capstan" in spec:
        t = spec["capstan"]
        base = Vector((0.0, _y(p, t), deck_height(p, t)))
        pole(parts, base, base + Vector((0.0, 0.0, CAPSTAN_HEIGHT)), CAPSTAN_DIAMETER, CAPSTAN_DIAMETER * 0.8, 12, "ShipWood",
             smooth=True)
    t = spec.get("helm_at", 0.08)
    base = Vector((0.0, _y(p, t), deck_height(p, t)))
    if spec.get("helm") == "wheel":
        wheel(parts, base + Vector((0.0, 0.0, WHEEL_DIAMETER / 2 + 0.3)), base.z)
    else:
        tiller(parts, p)


def coaming(parts: MeshParts, base: Vector, length: float, width: float) -> None:
    """Four boards round a hatch on the deck at base, and the dark below inside them."""
    z = base.z + HATCH_HEIGHT / 2
    for x in (-1.0, 1.0):
        axis_box(parts, Vector((x * (width - COAMING) / 2, base.y, z)), Vector((COAMING, length, HATCH_HEIGHT)), "ShipWood")
    for y in (-1.0, 1.0):
        axis_box(parts, Vector((0.0, base.y + y * (length - COAMING) / 2, z)), Vector((width - COAMING * 2, COAMING, HATCH_HEIGHT)),
                 "ShipWood")
    axis_box(parts, base + Vector((0.0, 0.0, 0.02)), Vector((width - COAMING * 2, length - COAMING * 2, 0.04)), "Tar")


def grating(parts: MeshParts, base: Vector, length: float, width: float) -> None:
    """Crossed bars over the opening, the fore and aft ones a little lower so the two do not meet flush."""
    inside = Vector((width - COAMING * 2, length - COAMING * 2))
    top = base.z + HATCH_HEIGHT
    for k in range(1, round(inside.x / GRATING_STEP)):
        x = -inside.x / 2 + inside.x * k / round(inside.x / GRATING_STEP)
        axis_box(parts, Vector((x, base.y, top - GRATING_BAR)), Vector((GRATING_BAR, inside.y, GRATING_BAR)), "ShipWood")
    for k in range(1, round(inside.y / GRATING_STEP)):
        y = -inside.y / 2 + inside.y * k / round(inside.y / GRATING_STEP)
        axis_box(parts, Vector((0.0, base.y + y, top - GRATING_BAR / 2)), Vector((inside.x, GRATING_BAR, GRATING_BAR)), "ShipWood")


def companionway(parts: MeshParts, base: Vector, length: float, width: float) -> None:
    """The way below: a hood on the coaming, its roof falling aft to an open end facing the helm, a ladder
    going down inside."""
    y0, y1 = base.y - length / 2, base.y + length / 2
    z0 = base.z + HATCH_HEIGHT

    def roof(y: float) -> float:
        return z0 + HOOD_HEIGHT - HOOD_FALL * (y1 - y) / length

    for x in (-1.0, 1.0):
        x0, x1 = sorted((x * width / 2, x * (width / 2 - COAMING)))
        corners = [Vector((xx, y, roof(y) if high else z0)) for high in (False, True) for y in (y0, y1) for xx in (x0, x1)]
        box(parts, corners, "ShipWood")
    axis_box(parts, Vector((0.0, y1 - COAMING / 2, (z0 + roof(y1)) / 2)), Vector((width - COAMING * 2, COAMING, roof(y1) - z0)),
             "ShipWood")
    box(parts, [Vector((x, y, roof(y) + dz)) for dz in (0.0, HOOD_ROOF) for y in (y0 - COAMING, y1)
                for x in (-width / 2 - COAMING / 2, width / 2 + COAMING / 2)], "ShipWood")
    rise = HATCH_HEIGHT / LADDER_STEPS
    for k in range(LADDER_STEPS):
        step = Vector((0.0, y0 + COAMING + (k + 0.5) * (length - COAMING * 2) / LADDER_STEPS, z0 - rise * (k + 1) + 0.02))
        axis_box(parts, step, Vector((width - COAMING * 2.5, 0.25, 0.04)), "ShipWood")


def wheel(parts: MeshParts, hub: Vector, deck_z: float) -> None:
    """A ship's wheel facing aft: a rim on spokes that run past it as handles, its axle in a pedestal of
    two posts and a cross piece forward of it."""
    along = Vector((0.0, 1.0, 0.0))
    ring(parts, hub, along, WHEEL_RIM, 0.07, 24, "Spar")
    pole(parts, hub - along * 0.1, hub + along * 0.1, 0.22, 0.22, 10, "Spar", smooth=True)
    for k in range(WHEEL_SPOKES):
        a = 2 * math.pi * k / WHEEL_SPOKES
        out = Vector((math.cos(a), 0.0, math.sin(a)))
        pole(parts, hub + out * 0.1, hub + out * WHEEL_RIM, 0.05, 0.04, 6, "Spar", caps=False, smooth=True)
        pole(parts, hub + out * WHEEL_RIM, hub + out * WHEEL_DIAMETER / 2, 0.06, 0.045, 6, "Spar", smooth=True)
    pedestal = hub + along * WHEEL_PEDESTAL
    pole(parts, hub, pedestal, 0.08, 0.08, 8, "Spar", smooth=True)
    for x in (-0.3, 0.3):
        post = Vector((pedestal.x + x, pedestal.y, deck_z))
        axis_box(parts, post + Vector((0.0, 0.0, (hub.z + 0.15 - deck_z) / 2)), Vector((0.12, 0.12, hub.z + 0.15 - deck_z)), "Spar")
    axis_box(parts, pedestal, Vector((0.72, 0.12, 0.14)), "Spar")


EYE_HEIGHT = 1.7
BEHIND_WHEEL = 0.8


def helm_eye(p: dict) -> Vector:
    """Where the helmsman's eyes are: behind the wheel, or beside the tiller's grip, standing on the deck."""
    spec = p.get("deck", {})
    if spec.get("helm") == "wheel":
        t = spec.get("helm_at", 0.08)
        y = _y(p, t) - BEHIND_WHEEL
    else:
        y = -p["length"] / 2 + 0.3 + RUDDER_HEAD / 2 + TILLER_LENGTH
    t = y / p["length"] + 0.5
    return Vector((0.0, y, deck_height(p, t) + EYE_HEIGHT))


def tiller(parts: MeshParts, p: dict) -> None:
    """A rudder head standing above the deck at the stern, the tiller from it rising slightly forward to
    a thinner grip."""
    deck_z = deck_height(p, 0.0)
    head = Vector((0.0, -p["length"] / 2 + 0.3, deck_z))
    axis_box(parts, head + Vector((0.0, 0.0, 0.4)), Vector((RUDDER_HEAD, RUDDER_HEAD, 0.8)), "Spar")
    points = [head + Vector((0.0, RUDDER_HEAD / 2 + TILLER_LENGTH * f, 0.65 + TILLER_RISE * f * f)) for f in (0.0, 0.4, 0.8, 1.0)]
    for (start, end), (d0, d1) in zip(itertools.pairwise(points), ((0.17, 0.14), (0.14, 0.11), (0.07, 0.07)), strict=True):
        pole(parts, start, end, d0, d1, 8, "Spar", smooth=True)


def fit(parts: MeshParts, p: dict) -> None:
    deck(parts, p)
    rail(parts, p)
    wales(parts, p)
    stripes(parts, p)
    gun_ports(parts, p)
    stern(parts, p)
    bow(parts, p)
    furniture(parts, p)
