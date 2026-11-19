"""A flat grid for the water surface; the shader displaces it.

A fine square core of even quads, so the vertex waves have the same resolution everywhere
near the ship, and around it coarse quads out to the far plane, which the waves never move:
the shader calms them to nothing before the core's edge, so the coarse quads meet a flat edge
and cannot crack against it. The grid follows the ship in whole core quads.
Centred on the origin at height 0. One material slot, Water (Unity's material is the water
shader, not this preview).
"""

from __future__ import annotations

import bpy

PARAMS = {
    "size": 256.0,         # m, side of the fine core
    "cells": 128,          # quads along the core's side: 2 m quads, 32 768 triangles; the shortest wave should be 3 m or more
    "outer_size": 2304.0,  # m, side of the whole grid: past the fog's end and the camera's far plane
    "outer_cell": 64.0,    # m, side of a coarse quad; the core's side is a whole number of them
    "water_rgb": [0.10, 0.35, 0.45],  # probe preview only
}


def build(params: dict) -> None:
    """Make the grid as one mesh object named WaterGrid."""
    n = params["cells"]
    half = params["size"] / 2
    step = params["size"] / n
    outer_half = params["outer_size"] / 2
    coarse = params["outer_cell"]

    verts: list[tuple[float, float, float]] = []
    index: dict[tuple[float, float], int] = {}

    def vertex(x: float, y: float) -> int:
        key = (round(x, 3), round(y, 3))
        if key not in index:
            index[key] = len(verts)
            verts.append((x, y, 0.0))
        return index[key]

    faces = []
    for j in range(n):
        for i in range(n):
            x0, y0 = -half + i * step, -half + j * step
            faces.append((vertex(x0, y0), vertex(x0 + step, y0), vertex(x0 + step, y0 + step), vertex(x0, y0 + step)))

    m = round(params["outer_size"] / coarse)
    for j in range(m):
        for i in range(m):
            x0, y0 = -outer_half + i * coarse, -outer_half + j * coarse
            if -half <= x0 < half and -half <= y0 < half:
                continue  # inside the core
            faces.append((vertex(x0, y0), vertex(x0 + coarse, y0), vertex(x0 + coarse, y0 + coarse), vertex(x0, y0 + coarse)))

    mesh = bpy.data.meshes.new("WaterGrid")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    # UVs in metres / outer size, 0 to 1 over the grid.
    uv = mesh.uv_layers.new(name="UVMap")
    for loop in mesh.loops:
        x, y, _ = mesh.vertices[loop.vertex_index].co
        uv.data[loop.index].uv = ((x + outer_half) / params["outer_size"], (y + outer_half) / params["outer_size"])
    mat = bpy.data.materials.new("Water")
    mat.use_nodes = True
    mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*params["water_rgb"], 1)
    mesh.materials.append(mat)
    for poly in mesh.polygons:
        poly.use_smooth = True
    obj = bpy.data.objects.new("WaterGrid", mesh)
    bpy.context.scene.collection.objects.link(obj)
