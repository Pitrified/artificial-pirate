"""People and things on deck, all coloured from the palette texture (one slot, Palette): cannons run
out at the gun ports, crew, a stern lantern; and the flag, its own object for its waving material.

Type keys (optional): crew, a list of [t (fraction from the stern), x (fraction of the half beam)].
"""

from __future__ import annotations

from mathutils import Vector

from builders.palette import swatch_uv
from builders.ship.fittings import gun_port_centres
from builders.ship.hull import bulwark_inner, deck_height, rail_height, transom_y
from builders.ship.mesh import MeshParts, axis_box, pole

CARRIAGE = Vector((0.8, 1.0, 0.4))
BARREL_LENGTH = 1.8
BARREL_OUT = 0.35

FIGURE_HEIGHT = 1.75
SHIRTS = ("shirt_white", "shirt_red", "shirt_blue")
SKINS = ("skin", "skin_dark")

LANTERN_POST = 1.0
LANTERN = Vector((0.35, 0.35, 0.5))

FLAG_COLUMNS = 10
FLAG_ROWS = 5
FLAG_OF_LENGTH = 0.11
FLAG_ASPECT = 0.65


def _y(p: dict, t: float) -> float:
    return (t - 0.5) * p["length"]


def cannons(parts: MeshParts, p: dict) -> None:
    iron, wood = swatch_uv("iron"), swatch_uv("carriage")
    for t, side, port in gun_port_centres(p):
        inner = bulwark_inner(p, t)
        base = deck_height(p, t)
        muzzle = Vector((port.x + side * BARREL_OUT, port.y, port.z))
        breech = muzzle - Vector((side * BARREL_LENGTH, 0.0, 0.0))
        carriage = Vector((side * (inner - CARRIAGE.x / 2 - 0.3), port.y, base + CARRIAGE.z / 2))
        axis_box(parts, carriage, CARRIAGE, "Palette", wood)
        pole(parts, breech, muzzle, 0.34, 0.24, 8, "Palette", uv=iron)


def _figure(parts: MeshParts, foot: Vector, facing: float, shirt: str, skin: str) -> None:
    """A standing sailor, about 1.75 m: legs, a body, arms hanging, a head and a hat."""
    h = FIGURE_HEIGHT
    trousers, hat = swatch_uv("trousers"), swatch_uv("hat")
    for x in (-0.12, 0.12):
        axis_box(parts, foot + Vector((x, 0.0, h * 0.25)), Vector((0.16, 0.18, h * 0.5)), "Palette", trousers)
    hips, shoulders = foot + Vector((0.0, 0.0, h * 0.5)), foot + Vector((0.0, 0.0, h * 0.8))
    pole(parts, hips, shoulders, 0.42, 0.5, 6, "Palette", uv=swatch_uv(shirt))
    for x in (-0.3, 0.3):
        hand = foot + Vector((x * 1.1, 0.06 * facing, h * 0.5))
        pole(parts, shoulders + Vector((x, 0.0, -0.05)), hand, 0.13, 0.11, 4, "Palette", uv=swatch_uv(skin))
    neck = shoulders + Vector((0.0, 0.0, 0.05))
    pole(parts, neck, neck + Vector((0.0, 0.0, 0.25)), 0.24, 0.22, 6, "Palette", uv=swatch_uv(skin))
    brim = neck + Vector((0.0, 0.0, 0.25))
    pole(parts, brim, brim + Vector((0.0, 0.0, 0.05)), 0.42, 0.42, 6, "Palette", uv=hat)
    pole(parts, brim, brim + Vector((0.0, 0.0, 0.16)), 0.26, 0.22, 6, "Palette", uv=hat)


def crew(parts: MeshParts, p: dict) -> None:
    for i, (t, x) in enumerate(p.get("crew", [])):
        foot = Vector((x * bulwark_inner(p, t), _y(p, t), deck_height(p, t)))
        _figure(parts, foot, 1.0 if i % 2 else -1.0, SHIRTS[i % len(SHIRTS)], SKINS[i % len(SKINS)])


def lantern(parts: MeshParts, p: dict) -> None:
    top = rail_height(p, 0.0) + 0.45
    base = Vector((0.0, transom_y(p, top) + 0.1, top))
    lamp = base + Vector((0.0, 0.0, LANTERN_POST))
    pole(parts, base, lamp, 0.1, 0.1, 6, "Palette", uv=swatch_uv("iron"))
    axis_box(parts, lamp + Vector((0.0, 0.0, LANTERN.z / 2)), LANTERN, "Palette", swatch_uv("lamp"))
    cap = lamp + Vector((0.0, 0.0, LANTERN.z))
    pole(parts, cap, cap + Vector((0.0, 0.0, 0.25)), 0.45, 0.05, 6, "Palette", uv=swatch_uv("brass"))


def place(parts: MeshParts, p: dict) -> None:
    cannons(parts, p)
    crew(parts, p)
    lantern(parts, p)


def flag(parts: MeshParts, p: dict, head: Vector, downwind: Vector) -> None:
    """A flat flag at a masthead, its fly streaming downwind; UV u runs hoist to fly, v foot to head."""
    length = p["length"] * FLAG_OF_LENGTH
    height = length * FLAG_ASPECT
    fly = Vector((downwind.x, downwind.y, 0.0)).normalized() * length
    top = head - Vector((0.0, 0.0, 0.3))
    grid, uvs = [], []
    for row in range(FLAG_ROWS + 1):
        v = row / FLAG_ROWS
        for col in range(FLAG_COLUMNS + 1):
            u = col / FLAG_COLUMNS
            grid.append(top - Vector((0.0, 0.0, height * (1 - v))) + fly * u)
            uvs.append((u, v))
    s = parts.add_verts(grid, uvs)
    width = FLAG_COLUMNS + 1
    for row in range(FLAG_ROWS):
        for col in range(FLAG_COLUMNS):
            parts.add_face((s + row * width + col, s + row * width + col + 1,
                            s + (row + 1) * width + col + 1, s + (row + 1) * width + col), "Flag", smooth=True)
