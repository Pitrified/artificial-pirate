"""What a hull carries: the planked deck, cap rail and stanchions, wales, gun ports, stern windows and
taffrail, the cutwater, figurehead and head rails, and deck furniture.

Type keys (all but the deck optional):
- wales: heights of the wales' centres above the water;
- gun_ports: {count (a side), from, to (fractions from the stern), z (centre), size};
- stern_windows: how many across the transom;
- deck: {hatches: [[t, length, width]], capstan: t, helm: "wheel" or "tiller", helm_at: t}.
"""

from __future__ import annotations

import itertools
import math
from collections.abc import Callable

from mathutils import Vector

from builders.ship.hull import (
    STATIONS,
    bulwark_inner,
    deck_height,
    half_beam,
    rail_height,
    transom_y,
)
from builders.ship.mesh import MeshParts, axis_box, box, line, pole, ring

WALE_HEIGHT = 0.25
WALE_OUT = 0.08
CAP_RAIL_HEIGHT = 0.1
CAP_RAIL_OVERHANG = 0.08
STANCHION_STEP = 1.2
STANCHION = 0.12
PORT_DEPTH = 0.06
PORT_LID = 0.05
WINDOW_WIDTH = 0.5
WINDOW_HEIGHT = 0.7
WINDOW_FRAME = 0.08
TAFFRAIL_HEIGHT = 0.45
HATCH_HEIGHT = 0.35
CAPSTAN_DIAMETER = 0.8
CAPSTAN_HEIGHT = 0.9
WHEEL_DIAMETER = 1.4  # to the handles' ends
WHEEL_RIM = 0.58  # radius
WHEEL_SPOKES = 8
WHEEL_PEDESTAL = 0.3  # forward of the wheel
TILLER_LENGTH = 2.5
TILLER_RISE = 0.25
RUDDER_HEAD = 0.26

Profile = Callable[[float], list[tuple[float, float]]]


def _y(p: dict, t: float) -> float:
    return (t - 0.5) * p["length"]


def _strip(parts: MeshParts, p: dict, profile: Profile, t0: float, t1: float, slot: str) -> None:
    """A small section lofted along the hull, both sides: its starboard points x, z, clockwise seen from astern."""
    steps = max(2, round((t1 - t0) * STATIONS))
    ts = [t0 + (t1 - t0) * i / steps for i in range(steps + 1)]
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
    for i in range(STATIONS):
        t0, t1 = i / STATIONS, (i + 1) / STATIONS
        corners = []
        for t, x_side in ((t0, 1.0), (t1, 1.0), (t1, -1.0), (t0, -1.0)):
            corners.append(Vector((x_side * bulwark_inner(p, t), _y(p, t), deck_height(p, t))))
        s = parts.add_verts(corners, [(v.y, v.x) for v in corners])
        parts.add_face((s, s + 1, s + 2, s + 3), "Deck")


def rail(parts: MeshParts, p: dict) -> None:
    """A cap rail standing proud of the bulwark, and stanchions inside it."""

    def cap(t: float) -> list[tuple[float, float]]:
        inner, outer, z = bulwark_inner(p, t) - CAP_RAIL_OVERHANG, half_beam(p, t) + CAP_RAIL_OVERHANG, rail_height(p, t)
        return [(inner, z), (inner, z + CAP_RAIL_HEIGHT), (outer, z + CAP_RAIL_HEIGHT), (outer, z)]

    _strip(parts, p, cap, 0.0, 1.0, "ShipWood")
    count = int(p["length"] / STANCHION_STEP)
    for i in range(1, count):
        t = i / count
        z0, z1 = deck_height(p, t), rail_height(p, t)
        for side in (1.0, -1.0):
            x = side * (bulwark_inner(p, t) - STANCHION / 2)
            axis_box(parts, Vector((x, _y(p, t), (z0 + z1) / 2)), Vector((STANCHION, STANCHION, z1 - z0)), "ShipWood")


def wales(parts: MeshParts, p: dict) -> None:
    for centre in p.get("wales", []):
        z0, z1 = centre - WALE_HEIGHT / 2, centre + WALE_HEIGHT / 2

        def wale(t: float, z0: float = z0, z1: float = z1) -> list[tuple[float, float]]:
            b = half_beam(p, t)
            return [(b, z1), (b + WALE_OUT, z1), (b + WALE_OUT, z0), (b, z0)]

        _strip(parts, p, wale, 0.03, 0.97, "Tar")


def gun_port_centres(p: dict) -> list[tuple[float, float, Vector]]:
    """Each gun port's station t, side (+1 starboard, -1 port) and centre on the hull's side."""
    spec = p.get("gun_ports")
    if not spec:
        return []
    ports = []
    for i in range(spec["count"]):
        t = spec["from"] + (spec["to"] - spec["from"]) * i / max(1, spec["count"] - 1)
        for side in (1.0, -1.0):
            ports.append((t, side, Vector((side * half_beam(p, t), _y(p, t), spec["z"]))))
    return ports


def gun_ports(parts: MeshParts, p: dict) -> None:
    for _, side, centre in gun_port_centres(p):
        size = p["gun_ports"]["size"]
        port = centre + Vector((side * PORT_DEPTH / 2, 0.0, 0.0))
        axis_box(parts, port, Vector((PORT_DEPTH, size, size)), "Tar")
        lid = port + Vector((side * size / 2, 0.0, size / 2 + PORT_LID))
        axis_box(parts, lid, Vector((size, size, PORT_LID)), "Trim")


def stern(parts: MeshParts, p: dict) -> None:
    """Windows across the transom between the deck and the rail, and a carved taffrail above it."""
    count = p.get("stern_windows", 0)
    rail_z, deck_z = rail_height(p, 0.0), deck_height(p, 0.0)
    z = (rail_z + deck_z) / 2 + 0.2
    span = half_beam(p, 0.0) * 1.2
    for i in range(count):
        x = -span / 2 + span * (i + 0.5) / count
        y = transom_y(p, z)
        axis_box(parts, Vector((x, y - 0.02, z)), Vector((WINDOW_WIDTH + WINDOW_FRAME * 2, 0.06, WINDOW_HEIGHT + WINDOW_FRAME * 2)), "Gilt")
        axis_box(parts, Vector((x, y - 0.06, z)), Vector((WINDOW_WIDTH, 0.06, WINDOW_HEIGHT)), "Tar")
    top = rail_z + TAFFRAIL_HEIGHT / 2
    axis_box(parts, Vector((0.0, transom_y(p, top) + 0.1, top)), Vector((half_beam(p, 0.0) * 1.8, 0.15, TAFFRAIL_HEIGHT)), "Gilt")


def bow(parts: MeshParts, p: dict) -> None:
    """A cutwater forward of the stem, a figurehead on it leaning out, and head rails back to the bow."""
    stem_y = p["length"] / 2
    top = rail_height(p, 1.0) - 0.4
    reach = p["length"] * 0.045
    corners = [Vector((x, y, z))
               for z, ys in ((0.4, (stem_y - 0.3, stem_y + 0.3)), (top, (stem_y - 0.3, stem_y + reach)))
               for y in ys for x in (-0.12, 0.12)]
    box(parts, corners, "Trim")

    foot = Vector((0.0, stem_y + reach - 0.2, top - 0.3))
    body_top = foot + Vector((0.0, 0.5, 1.1))
    pole(parts, foot, body_top, 0.5, 0.35, 6, "Gilt")
    pole(parts, body_top, body_top + Vector((0.0, 0.15, 0.4)), 0.35, 0.3, 6, "Gilt")

    for side in (1.0, -1.0):
        aft = Vector((side * half_beam(p, 0.93), stem_y - 0.07 * p["length"], rail_height(p, 0.93) - 0.3))
        for drop in (0.0, 0.5):
            line(parts, foot + Vector((side * 0.15, 0.0, 0.2 - drop)), aft - Vector((0.0, 0.0, drop)), 0.12, "Trim")


def furniture(parts: MeshParts, p: dict) -> None:
    spec = p.get("deck")
    if not spec:
        return
    for t, length, width in spec.get("hatches", []):
        base = deck_height(p, t)
        axis_box(parts, Vector((0.0, _y(p, t), base + HATCH_HEIGHT / 2)), Vector((width, length, HATCH_HEIGHT)), "ShipWood")
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
    gun_ports(parts, p)
    stern(parts, p)
    bow(parts, p)
    furniture(parts, p)
