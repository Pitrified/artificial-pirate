"""Mesh assembly for the ship builder: parts gathered with their slots and UVs, then made into objects."""

from __future__ import annotations

from collections.abc import Callable

import math
from dataclasses import dataclass, field

import bpy
from mathutils import Vector

DEFAULT_PREVIEW = (0.5, 0.5, 0.5)
LINE_SIDES = 3

UV = tuple[float, float]


@dataclass(frozen=True)
class Rope:
    """A rope laid as a line, kept so the sails can be checked against it."""

    kind: str
    start: Vector
    end: Vector
    thickness: float
    sail: str | None = None  # the sail it carries or sheets, which it meets along its edge


@dataclass(frozen=True)
class Cloth:
    """A sail's faces among the parts' faces."""

    name: str
    faces: range


@dataclass
class MeshParts:
    """Vertices with UVs (in metres), faces with their slot and shading, and sharp edges; the ropes and
    cloths among them, by name. A face can carry its own UV per corner, where faces meeting at a vertex
    map it differently (a box's edges, a pole's seam)."""

    verts: list[Vector] = field(default_factory=list)
    uvs: list[tuple[float, float]] = field(default_factory=list)
    faces: list[tuple[int, ...]] = field(default_factory=list)
    slots: list[str] = field(default_factory=list)
    smooth: list[bool] = field(default_factory=list)
    corner_uvs: list[tuple[UV, ...] | None] = field(default_factory=list)
    sharp: set[tuple[int, int]] = field(default_factory=set)
    ropes: list[Rope] = field(default_factory=list)
    cloths: list[Cloth] = field(default_factory=list)

    def add_verts(self, points: list[Vector], uvs: list[tuple[float, float]] | None = None) -> int:
        start = len(self.verts)
        self.verts.extend(points)
        self.uvs.extend(uvs if uvs is not None else [(0.0, 0.0)] * len(points))
        return start

    def add_face(self, indices: tuple[int, ...], slot: str, smooth: bool = False,
                 uvs: tuple[UV, ...] | None = None) -> None:
        self.faces.append(indices)
        self.slots.append(slot)
        self.smooth.append(smooth)
        self.corner_uvs.append(uvs)

    def add_planar_face(self, indices: tuple[int, ...], slot: str) -> None:
        """A flat face mapped in metres on its own plane, u along its longer first edge."""
        self.add_face(indices, slot, uvs=planar_uvs([self.verts[i] for i in indices]))

    def mark_sharp(self, a: int, b: int) -> None:
        self.sharp.add((min(a, b), max(a, b)))


def planar_uvs(points: list[Vector]) -> tuple[UV, ...]:
    """A flat polygon's corners in metres on its plane: u along the longer of its two edges from the
    first corner, so a board's grain runs along it."""
    origin = points[0]
    first, last = points[1] - origin, points[-1] - origin
    along = first if first.length >= last.length else last
    normal = first.cross(last)
    if along.length < 1e-9 or normal.length < 1e-12:
        return tuple((0.0, 0.0) for _ in points)
    u = along.normalized()
    v = normal.cross(u).normalized()
    return tuple(((q - origin).dot(u), (q - origin).dot(v)) for q in points)


def box(parts: MeshParts, corners: list[Vector], slot: str, uv: UV | None = None) -> None:
    """Eight corners ordered by z, then y, then x, each pair low and high; uv for every corner if given,
    else each face mapped in metres."""
    s = parts.add_verts(corners, [uv] * 8 if uv else None)
    for face in ((0, 2, 3, 1), (4, 5, 7, 6), (0, 1, 5, 4), (2, 6, 7, 3), (0, 4, 6, 2), (1, 3, 7, 5)):
        indices = tuple(s + i for i in face)
        if uv:
            parts.add_face(indices, slot)
        else:
            parts.add_planar_face(indices, slot)


def axis_box(parts: MeshParts, centre: Vector, size: Vector, slot: str, uv: UV | None = None) -> None:
    """A box aligned to the axes, by its centre and size."""
    h = size / 2
    corners = [centre + Vector((x * h.x, y * h.y, z * h.z)) for z in (-1, 1) for y in (-1, 1) for x in (-1, 1)]
    box(parts, corners, slot, uv)


def frame(axis: Vector) -> tuple[Vector, Vector]:
    """Two unit vectors square to the axis and to each other."""
    side = axis.cross(Vector((0, 0, 1)))
    if side.length < 1e-3:
        side = axis.cross(Vector((1, 0, 0)))
    side.normalize()
    return side, side.cross(axis)


def pole(parts: MeshParts, start: Vector, end: Vector, d0: float, d1: float, sides: int, slot: str,
         caps: bool = True, uv: UV | None = None, smooth: bool = False) -> None:
    """A tapered pole between two points, mapped in metres: u along it, v around; smooth rounds its
    sides and keeps the caps flat."""
    axis = (end - start).normalized()
    side, up = frame(axis)
    rings = []
    for centre, d in ((start, d0), (end, d1)):
        ring = [centre + (side * math.cos(a) + up * math.sin(a)) * d / 2
                for a in (2 * math.pi * i / sides for i in range(sides))]
        rings.append(parts.add_verts(ring, [uv] * sides if uv else None))
    a, b = rings
    length = (end - start).length
    step = math.pi * (d0 + d1) / 2 / sides
    for i in range(sides):
        j = (i + 1) % sides
        uvs = None if uv else ((0.0, i * step), (length, i * step), (length, (i + 1) * step), (0.0, (i + 1) * step))
        parts.add_face((a + i, b + i, b + j, a + j), slot, smooth, uvs)
    if caps:
        for cap in (tuple(a + i for i in range(sides)), tuple(b + i for i in reversed(range(sides)))):
            if uv:
                parts.add_face(cap, slot)
            else:
                parts.add_planar_face(cap, slot)
        if smooth:
            for ring in rings:
                for i in range(sides):
                    parts.mark_sharp(ring + i, ring + (i + 1) % sides)


def ring(parts: MeshParts, centre: Vector, axis: Vector, radius: float, thickness: float, segments: int,
         slot: str, sides: int = 6, uv: UV | None = None) -> None:
    """A smooth hoop round an axis: a rim, a band on a mast. Mapped in metres, u around the hoop, or
    every corner on uv if given."""
    axis = axis.normalized()
    side, up = frame(axis)
    loops = []
    for i in range(segments):
        a = 2 * math.pi * i / segments
        out = side * math.cos(a) + up * math.sin(a)
        loops.append(parts.add_verts([centre + out * (radius + math.cos(b) * thickness / 2) + axis * (math.sin(b) * thickness / 2)
                                      for b in (2 * math.pi * k / sides for k in range(sides))],
                                     [uv] * sides if uv else None))
    step, tube = 2 * math.pi * radius / segments, math.pi * thickness / sides
    for i in range(segments):
        a, b = loops[i], loops[(i + 1) % segments]
        for k in range(sides):
            n = (k + 1) % sides
            uvs = None if uv else ((i * step, k * tube), (i * step, (k + 1) * tube),
                                   ((i + 1) * step, (k + 1) * tube), ((i + 1) * step, k * tube))
            parts.add_face((a + k, a + n, b + n, b + k), slot, True, uvs)


def line(parts: MeshParts, start: Vector, end: Vector, thickness: float, slot: str) -> None:
    """A rope: a three-sided prism with no caps, the cheapest pole that reads from every side."""
    pole(parts, start, end, thickness, thickness, LINE_SIDES, slot, caps=False)


def make_object(name: str, parts: MeshParts, preview_rgb: dict[str, list[float]],
                origin: Vector | None = None, warp: Callable[[Vector], Vector] | None = None) -> bpy.types.Object:
    """One mesh object from the parts, turned half round so the bow faces -Y (Unity's +Z after export);
    its origin at the given point (a part that turns about it), else at the ship's. A warp, if given,
    moves every vertex as the object is made, leaving the parts as they are."""
    o = origin or Vector((0.0, 0.0, 0.0))
    verts = [warp(v) for v in parts.verts] if warp else parts.verts
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([(-(v.x - o.x), -(v.y - o.y), v.z - o.z) for v in verts], [], parts.faces)
    mesh.update()

    slot_names = list(dict.fromkeys(parts.slots))
    for slot in slot_names:
        mesh.materials.append(_material(slot, preview_rgb.get(slot, DEFAULT_PREVIEW)))
    index = {slot: i for i, slot in enumerate(slot_names)}
    for poly, slot, smooth in zip(mesh.polygons, parts.slots, parts.smooth):
        poly.material_index = index[slot]
        poly.use_smooth = smooth

    sharp = mesh.attributes.new("sharp_edge", "BOOLEAN", "EDGE")
    for edge in mesh.edges:
        a, b = edge.vertices
        sharp.data[edge.index].value = (min(a, b), max(a, b)) in parts.sharp

    uv = mesh.uv_layers.new(name="UVMap")
    for poly, corners in zip(mesh.polygons, parts.corner_uvs):
        for k, loop in enumerate(poly.loop_indices):
            uv.data[loop].uv = corners[k] if corners else parts.uvs[mesh.loops[loop].vertex_index]

    obj = bpy.data.objects.new(name, mesh)
    obj.location = (-o.x, -o.y, o.z)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def _material(name: str, rgb: list[float]) -> bpy.types.Material:
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*rgb, 1)
    bsdf.inputs["Roughness"].default_value = 0.8
    return mat
