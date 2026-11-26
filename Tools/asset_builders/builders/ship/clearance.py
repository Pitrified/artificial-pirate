"""Where ropes pass through sails: each rope, as thick as it is drawn, against each cloth's triangles."""

from __future__ import annotations

import itertools
from collections import Counter
from dataclasses import dataclass

from mathutils import Vector
from mathutils.geometry import intersect_ray_tri, normal

from builders.ship.mesh import Cloth, MeshParts, Rope

END_GRACE = 0.6  # a rope made fast at a sail's corner (a sheet, a brace) meets it there; that is not a crossing


@dataclass(frozen=True)
class Crossing:
    rope: Rope
    cloth: str
    at: Vector


def _triangles(parts: MeshParts, cloth: Cloth) -> list[tuple[Vector, Vector, Vector]]:
    tris = []
    for f in cloth.faces:
        corners = [parts.verts[i] for i in parts.faces[f]]
        tris.extend((corners[0], corners[k], corners[k + 1]) for k in range(1, len(corners) - 1))
    return tris


def _bounds(points: list[Vector] | tuple[Vector, ...], margin: float) -> tuple[Vector, Vector]:
    lo = Vector([min(p[k] for p in points) - margin for k in range(3)])
    hi = Vector([max(p[k] for p in points) + margin for k in range(3)])
    return lo, hi


def _overlap(a: tuple[Vector, Vector], b: tuple[Vector, Vector]) -> bool:
    return all(a[0][k] <= b[1][k] and b[0][k] <= a[1][k] for k in range(3))


def _hit(rope: Rope, tris: list[tuple[Vector, Vector, Vector]]) -> Vector | None:
    """Where the rope's core or either face of it passes through a triangle, along any of its segments,
    away from its ends (measured along the rope)."""
    total = sum((b - a).length for a, b in itertools.pairwise(rope.points))
    walked = 0.0
    for start, end in itertools.pairwise(rope.points):
        along = end - start
        length = along.length
        direction = along / length
        for tri in tris:
            n = normal(*tri)
            for shift in (0.0, rope.thickness / 2, -rope.thickness / 2):
                origin = start + n * shift
                point = intersect_ray_tri(*tri, direction, origin, True)
                if point is None:
                    continue
                t = (point - origin).dot(direction)
                if t <= length and END_GRACE < walked + t < total - END_GRACE:
                    return point
        walked += length
    return None


def crossings(ropes: list[Rope], parts: MeshParts) -> list[Crossing]:
    """Every rope that passes through one of the parts' cloths, but for the cloth's own ropes."""
    found = []
    for cloth in parts.cloths:
        tris = _triangles(parts, cloth)
        box = _bounds([v for tri in tris for v in tri], 0.0)
        for rope in ropes:
            if rope.sail == cloth.name or not _overlap(box, _bounds(rope.points, rope.thickness)):
                continue
            point = _hit(rope, tris)
            if point is not None:
                found.append(Crossing(rope, cloth.name, point))
    return found


def report(name: str, found: list[Crossing]) -> None:
    print(f"clearance {name}: {len(found)} ropes through sails")
    for (cloth, kind), count in sorted(Counter((c.cloth, c.rope.kind) for c in found).items()):
        print(f"  {cloth} x {kind}: {count}")
