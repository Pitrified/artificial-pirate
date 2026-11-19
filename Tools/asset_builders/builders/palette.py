"""A palette texture: flat colour swatches in a square grid, so faces of many colours share one material.

A face picks its colour by putting all its UVs on a swatch's centre. textures.py paints the texture
from the same table, so the order here is the texture's layout.
"""

from __future__ import annotations

SWATCHES_PER_ROW = 8

COLOURS: dict[str, tuple[float, float, float]] = {
    "skin": (0.80, 0.60, 0.45),
    "skin_dark": (0.48, 0.32, 0.22),
    "shirt_white": (0.85, 0.82, 0.74),
    "shirt_red": (0.62, 0.15, 0.12),
    "shirt_blue": (0.18, 0.25, 0.42),
    "trousers": (0.20, 0.18, 0.16),
    "hat": (0.08, 0.07, 0.06),
    "iron": (0.12, 0.12, 0.13),
    "carriage": (0.45, 0.30, 0.17),
    "brass": (0.72, 0.56, 0.24),
    "lamp": (1.00, 0.85, 0.45),
}


def swatch_uv(name: str) -> tuple[float, float]:
    """The centre of a colour's swatch, in UV."""
    index = list(COLOURS).index(name)
    col, row = index % SWATCHES_PER_ROW, index // SWATCHES_PER_ROW
    return (col + 0.5) / SWATCHES_PER_ROW, 1 - (row + 0.5) / SWATCHES_PER_ROW
