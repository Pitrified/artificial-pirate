"""A sailing ship from a dict of numbers: a lofted hull with a deck and bulwark, masts, spars and sails.

A ship type is a builder module whose PARAMS fill this dict and whose build is this one's.
Metres, Z up, origin on the design waterline amidships, bow toward +Y, starboard +X.
Five material slots: HullBottom, Topsides, Trim, ShipWood and SailCloth.

Hull keys: length (on deck), beam, draft, freeboard (amidships, to the rail), sheer_bow and
sheer_stern (rise of the rail toward each end), sheer_low (where the rail is lowest, as a fraction
from the stern), widest (where the hull is widest, same), transom (the transom's width over the
beam), transom_rake_deg, bulwark (height above the deck), band (the trim band's depth below the rail),
boot (how high the bottom paint shows above the water).

masts: each has at (fraction from the stern), height (above the water), diameter, rake_deg, yards
(each z and length), and optionally a boom (z, length, rise_deg) and a gaff (z, length, peak_deg).
bowsprit: length beyond the stem, steeve_deg, diameter.
sails: a square sail hangs from a yard (top) to the yard below it (bottom) or to a foot {z, length};
a gaff sail fills a mast's gaff and boom; a jib runs from head_z on a mast to a point along the bowsprit
(tack, 0 to 1), its clew clew_aft metres behind the tack at clew_z.
"""

from __future__ import annotations

import math
from dataclasses import dataclass, field

import bpy
from mathutils import Vector

SLOTS = ("HullBottom", "Topsides", "Trim", "ShipWood", "SailCloth")
BOTTOM, TOPSIDES, TRIM, WOOD, SAIL = range(len(SLOTS))

STATIONS = 20
UNDERWATER_POINTS = 5
BILGE_POWER = 2.5
STEM_HALF_WIDTH = 0.12
BULWARK_THICKNESS = 0.15
MAST_SIDES = 8
SPAR_SIDES = 6
SAIL_ROWS = 4
SAIL_COLUMNS = 4
SAIL_BELLY = 0.08
SAIL_CLEARANCE = 0.3


@dataclass
class MeshParts:
    """Vertices, faces with their slot, and sharp edges, gathered from every part before one mesh is made."""

    verts: list[Vector] = field(default_factory=list)
    faces: list[tuple[int, ...]] = field(default_factory=list)
    slots: list[int] = field(default_factory=list)
    sharp: set[tuple[int, int]] = field(default_factory=set)

    def add_verts(self, points: list[Vector]) -> int:
        start = len(self.verts)
        self.verts.extend(points)
        return start

    def add_face(self, indices: tuple[int, ...], slot: int) -> None:
        self.faces.append(indices)
        self.slots.append(slot)

    def mark_sharp(self, a: int, b: int) -> None:
        self.sharp.add((min(a, b), max(a, b)))


# ---------------------------------------------------------------- hull


def _rail_height(p: dict, t: float) -> float:
    low = p["sheer_low"]
    if t >= low:
        return p["freeboard"] + p["sheer_bow"] * ((t - low) / (1 - low)) ** 2
    return p["freeboard"] + p["sheer_stern"] * ((low - t) / low) ** 2


def _half_beam(p: dict, t: float) -> float:
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
    forefoot = 0.75
    if t <= forefoot:
        return p["draft"]
    return p["draft"] * (1 - ((t - forefoot) / (1 - forefoot)) ** 2)


def _section(p: dict, t: float) -> list[Vector]:
    """One station's outline, a closed loop: deck to rail to waterline to keel, both sides."""
    y = (t - 0.5) * p["length"]
    b = _half_beam(p, t)
    rail = _rail_height(p, t)
    deck = rail - p["bulwark"]
    inner = max(b - BULWARK_THICKNESS, b * 0.5)
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


# Which slot each band of the loop takes, between consecutive points of the starboard half.
_SECTION_SLOTS = (WOOD, TRIM, TRIM, TOPSIDES, BOTTOM) + (BOTTOM,) * UNDERWATER_POINTS
_SHARP_POINTS = (0, 1, 2)


def _hull(parts: MeshParts, p: dict) -> None:
    sections = [_section(p, i / STATIONS) for i in range(STATIONS + 1)]
    loops = [parts.add_verts(section) for section in sections]
    n = len(sections[0])
    # Starboard's bands, port's mirrored, and the deck closing the loop.
    slots = (*_SECTION_SLOTS, *reversed(_SECTION_SLOTS), WOOD)

    for i in range(STATIONS):
        a, b = loops[i], loops[i + 1]
        for k in range(n):
            j = (k + 1) % n
            parts.add_face((a + k, a + j, b + j, b + k), slots[k])
        for k in _SHARP_POINTS + tuple(n - 1 - k for k in _SHARP_POINTS):
            parts.mark_sharp(a + k, b + k)

    stern, bow = loops[0], loops[-1]
    parts.add_face(tuple(stern + k for k in range(n)), TOPSIDES)
    parts.add_face(tuple(bow + k for k in reversed(range(n))), TOPSIDES)
    for k in range(n):
        parts.mark_sharp(stern + k, stern + (k + 1) % n)
        parts.mark_sharp(bow + k, bow + (k + 1) % n)


def _deck_height(p: dict, t: float) -> float:
    return _rail_height(p, t) - p["bulwark"]


def _rudder(parts: MeshParts, p: dict) -> None:
    y = -p["length"] / 2
    depth = p["draft"] * 0.9
    chord = p["draft"] * 0.4
    half = 0.1
    corners = [Vector((x, y - dy, z)) for z in (-depth, p["boot"]) for dy in (0.0, chord) for x in (-half, half)]
    _box(parts, corners, BOTTOM)


# ---------------------------------------------------------------- rig


def _box(parts: MeshParts, c: list[Vector], slot: int) -> None:
    """Eight corners ordered by z, then y, then x, each pair low and high."""
    s = parts.add_verts(c)
    for face in ((0, 2, 3, 1), (4, 5, 7, 6), (0, 1, 5, 4), (2, 6, 7, 3), (0, 4, 6, 2), (1, 3, 7, 5)):
        parts.add_face(tuple(s + i for i in face), slot)


def _spar(parts: MeshParts, start: Vector, end: Vector, d0: float, d1: float, sides: int) -> None:
    """A tapered pole between two points, capped at both ends."""
    axis = (end - start).normalized()
    side = axis.cross(Vector((0, 0, 1)))
    if side.length < 1e-3:
        side = axis.cross(Vector((1, 0, 0)))
    side.normalize()
    up = side.cross(axis)
    rings = []
    for centre, d in ((start, d0), (end, d1)):
        ring = [centre + (side * math.cos(a) + up * math.sin(a)) * d / 2
                for a in (2 * math.pi * i / sides for i in range(sides))]
        rings.append(parts.add_verts(ring))
    a, b = rings
    for i in range(sides):
        j = (i + 1) % sides
        parts.add_face((a + i, b + i, b + j, a + j), WOOD)
    parts.add_face(tuple(a + i for i in range(sides)), WOOD)
    parts.add_face(tuple(b + i for i in reversed(range(sides))), WOOD)


@dataclass
class Mast:
    """A mast's foot on deck and its direction, so spars and sails can find a point at any height."""

    foot: Vector
    direction: Vector
    spec: dict

    def at(self, z: float) -> Vector:
        return self.foot + self.direction * ((z - self.foot.z) / self.direction.z)


def _mast(p: dict, spec: dict) -> Mast:
    t = spec["at"]
    foot = Vector((0.0, (t - 0.5) * p["length"], _deck_height(p, t)))
    rake = math.radians(spec["rake_deg"])
    return Mast(foot, Vector((0.0, -math.sin(rake), math.cos(rake))), spec)


def _boom_end(mast: Mast) -> tuple[Vector, Vector]:
    boom = mast.spec["boom"]
    rise = math.radians(boom["rise_deg"])
    start = mast.at(boom["z"])
    return start, start + Vector((0.0, -math.cos(rise), math.sin(rise))) * boom["length"]


def _gaff_end(mast: Mast) -> tuple[Vector, Vector]:
    gaff = mast.spec["gaff"]
    peak = math.radians(gaff["peak_deg"])
    start = mast.at(gaff["z"])
    return start, start + Vector((0.0, -math.cos(peak), math.sin(peak))) * gaff["length"]


def _yard_ends(mast: Mast, yard: dict) -> tuple[Vector, Vector]:
    centre = mast.at(yard["z"]) + Vector((0.0, SAIL_CLEARANCE, 0.0))
    half = Vector((yard["length"] / 2, 0.0, 0.0))
    return centre - half, centre + half


def _bowsprit(p: dict) -> tuple[Vector, Vector]:
    spec = p["bowsprit"]
    steeve = math.radians(spec["steeve_deg"])
    start = Vector((0.0, p["length"] / 2 - 1.0, _deck_height(p, 1.0) + 0.3))
    end = Vector((0.0, p["length"] / 2, start.z)) + Vector((0.0, math.cos(steeve), math.sin(steeve))) * spec["length"]
    return start, end


def _rig(parts: MeshParts, p: dict, masts: list[Mast]) -> None:
    for mast in masts:
        d = mast.spec["diameter"]
        _spar(parts, mast.foot, mast.at(mast.spec["height"]), d, d * 0.5, MAST_SIDES)
        spar_d = d * 0.45
        for yard in mast.spec["yards"]:
            _spar(parts, *_yard_ends(mast, yard), spar_d, spar_d, SPAR_SIDES)
        if "boom" in mast.spec:
            _spar(parts, *_boom_end(mast), spar_d, spar_d * 0.8, SPAR_SIDES)
        if "gaff" in mast.spec:
            _spar(parts, *_gaff_end(mast), spar_d, spar_d * 0.8, SPAR_SIDES)
    d = p["bowsprit"]["diameter"]
    _spar(parts, *_bowsprit(p), d, d * 0.5, SPAR_SIDES)


# ---------------------------------------------------------------- sails


def _sheet(parts: MeshParts, corners: tuple[Vector, Vector, Vector, Vector], belly: Vector) -> None:
    """A two-sided sail over four corners (foot start, foot end, head end, head start), bellied along `belly`."""
    a, b, c, d = corners
    grid = []
    for row in range(SAIL_ROWS + 1):
        v = row / SAIL_ROWS
        for col in range(SAIL_COLUMNS + 1):
            u = col / SAIL_COLUMNS
            point = a.lerp(b, u).lerp(d.lerp(c, u), v)
            grid.append(point + belly * (math.sin(math.pi * u) * math.sin(math.pi * v)))
    width = SAIL_COLUMNS + 1
    for side in (0, 1):
        s = parts.add_verts(grid)
        for row in range(SAIL_ROWS):
            for col in range(SAIL_COLUMNS):
                q = (s + row * width + col, s + row * width + col + 1,
                     s + (row + 1) * width + col + 1, s + (row + 1) * width + col)
                parts.add_face(q if side == 0 else tuple(reversed(q)), SAIL)


def _square_sail(parts: MeshParts, mast: Mast, spec: dict) -> None:
    yards = mast.spec["yards"]
    top_l, top_r = _yard_ends(mast, yards[spec["top"]])
    bottom = spec["bottom"]
    foot = yards[bottom] if isinstance(bottom, int) else bottom
    foot_l, foot_r = _yard_ends(mast, foot)
    width = (top_r - top_l).length
    _sheet(parts, (foot_l, foot_r, top_r, top_l), Vector((0.0, SAIL_BELLY * width, 0.0)))


def _gaff_sail(parts: MeshParts, mast: Mast) -> None:
    tack, clew = _boom_end(mast)
    throat, peak = _gaff_end(mast)
    width = (clew - tack).length
    _sheet(parts, (tack, clew, peak, throat), Vector((SAIL_BELLY * width, 0.0, 0.0)))


def _jib(parts: MeshParts, p: dict, mast: Mast, spec: dict) -> None:
    start, end = _bowsprit(p)
    tack = start.lerp(end, spec["tack"])
    head = mast.at(spec["head_z"]) + Vector((0.0, SAIL_CLEARANCE, 0.0))
    clew = Vector((0.0, tack.y - spec["clew_aft"], spec["clew_z"]))
    width = (clew - tack).length
    _sheet(parts, (clew, tack, head, head), Vector((SAIL_BELLY * width, 0.0, 0.0)))


def _sails(parts: MeshParts, p: dict, masts: list[Mast]) -> None:
    for spec in p["sails"]:
        mast = masts[spec["mast"]]
        kind = spec["kind"]
        if kind == "square":
            _square_sail(parts, mast, spec)
        elif kind == "gaff":
            _gaff_sail(parts, mast)
        elif kind == "jib":
            _jib(parts, p, mast, spec)
        else:
            msg = f"unknown sail kind '{kind}'"
            raise ValueError(msg)


# ---------------------------------------------------------------- mesh


def _material(name: str, rgb: list[float]) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*rgb, 1)
    bsdf.inputs["Roughness"].default_value = 0.8
    return mat


def build(params: dict) -> None:
    """Make the ship as one mesh object named after params['name']."""
    parts = MeshParts()
    _hull(parts, params)
    _rudder(parts, params)
    masts = [_mast(params, spec) for spec in params["masts"]]
    _rig(parts, params, masts)
    _sails(parts, params, masts)

    name = params["name"]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([tuple(v) for v in parts.verts], [], parts.faces)
    mesh.update()
    for poly, slot in zip(mesh.polygons, parts.slots):
        poly.material_index = slot
        poly.use_smooth = slot in (BOTTOM, TOPSIDES, SAIL)
    sharp = mesh.attributes.new("sharp_edge", "BOOLEAN", "EDGE")
    for edge in mesh.edges:
        a, b = edge.vertices
        sharp.data[edge.index].value = (min(a, b), max(a, b)) in parts.sharp
    for slot in SLOTS:
        mesh.materials.append(_material(slot, params["preview_rgb"][slot]))

    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
