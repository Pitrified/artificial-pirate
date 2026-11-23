"""Generate the project's procedural textures as PNG, deterministically.

Run from this folder:

    uv run --no-sync python textures.py water_normals   (or palette, sail_canvas, hull_planks, deck_planks, flag, hull_marks,
                                                         spar_wood, ship_wood, particles)

Each texture is a function below. Tiling comes from building the height field out of
waves with integer frequencies over the tile, so the edges meet. The same seed gives
the same bytes, so git sees only real changes.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

from builders import palette as palette_table

REPO_ROOT = Path(__file__).resolve().parents[2]
TEXTURES_DIR = REPO_ROOT / "ArtificialPirate" / "Assets" / "Art" / "Textures"


def tiling_height(size: int, waves: int, min_freq: int, max_freq: int, falloff: float, seed: int,
                  max_freq_x: int | None = None) -> np.ndarray:
    """A tiling height field in [-1, 1]: a sum of plane waves with integer frequencies. A low max_freq_x
    stretches it along x, as wood grain."""
    rng = np.random.default_rng(seed)
    y, x = np.mgrid[0:size, 0:size] / size
    h = np.zeros((size, size))
    along = max_freq if max_freq_x is None else max_freq_x
    for _ in range(waves):
        fx, fy = rng.integers(-along, along + 1), rng.integers(-max_freq, max_freq + 1)
        f = float(np.hypot(fx, fy))
        if f < min_freq:
            continue
        phase = rng.uniform(0, 2 * np.pi)
        h += np.sin(2 * np.pi * (fx * x + fy * y) + phase) / f ** falloff
    return h / np.abs(h).max()


def normal_map(height: np.ndarray, strength: float) -> Image.Image:
    """Tangent-space normal map from a tiling height field (wrapped gradients), RGB 0-255."""
    dx = (np.roll(height, -1, axis=1) - np.roll(height, 1, axis=1)) * 0.5
    dy = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * 0.5
    n = np.stack([-dx * strength, -dy * strength, np.ones_like(height)], axis=-1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    rgb = np.round((n * 0.5 + 0.5) * 255).astype(np.uint8)
    return Image.fromarray(rgb, "RGB")


def water_normals() -> Path:
    """Small choppy detail for the water surface, tiled a few times per wavelength."""
    height = tiling_height(size=256, waves=48, min_freq=3, max_freq=14, falloff=1.2, seed=11)
    out = TEXTURES_DIR / "T_WaterNormal.png"
    out.parent.mkdir(parents=True, exist_ok=True)
    normal_map(height, strength=24.0).save(out, optimize=False)
    return out


def _save(image: Image.Image, name: str) -> Path:
    out = TEXTURES_DIR / f"{name}.png"
    out.parent.mkdir(parents=True, exist_ok=True)
    image.save(out, optimize=False)
    return out


def _grey(values: np.ndarray) -> Image.Image:
    return Image.fromarray(np.round(np.clip(values, 0, 1) * 255).astype(np.uint8), "L").convert("RGB")


def palette() -> Path:
    """The flat-colour swatches of builders/palette.py, 8 px each, for point filtering."""
    swatch = 8
    size = palette_table.SWATCHES_PER_ROW * swatch
    rgb = np.zeros((size, size, 3))
    for i, colour in enumerate(palette_table.COLOURS.values()):
        row, col = divmod(i, palette_table.SWATCHES_PER_ROW)
        rgb[row * swatch:(row + 1) * swatch, col * swatch:(col + 1) * swatch] = colour
    return _save(Image.fromarray(np.round(rgb * 255).astype(np.uint8), "RGB"), "T_Palette")


def sail_canvas() -> Path:
    """Worn canvas over 4 x 4 m, tiling: seams between 0.5 m cloths, dirt and wear in broad patches."""
    size = 256
    dirt = tiling_height(size, waves=24, min_freq=1, max_freq=5, falloff=1.0, seed=21)
    grain = tiling_height(size, waves=48, min_freq=12, max_freq=40, falloff=0.8, seed=22)
    tone = np.array([0.9, 0.86, 0.77]) * (1 - 0.06 * (dirt[..., None] * 0.5 + 0.5) - 0.02 * grain[..., None])
    x = np.arange(size)
    seams = (x % (size // 8)) < 2
    tone[:, seams] *= 0.86
    return _save(Image.fromarray(np.round(np.clip(tone, 0, 1) * 255).astype(np.uint8), "RGB"), "T_SailCanvas")


def _planks(width_px: int, height_px: int, plank_px: int, butt_every_px: int, seed: int) -> np.ndarray:
    """Planks running along x: dark seams between them, staggered butt joints, grain. Grey, about 1."""
    rng = np.random.default_rng(seed)
    grain = tiling_height(max(width_px, height_px), waves=40, min_freq=8, max_freq=32, falloff=0.9, seed=seed)
    tone = 0.92 + 0.05 * grain[:height_px, :width_px]
    for row in range(0, height_px, plank_px):
        tone[row:row + 1, :] *= 0.55
        tone[row:row + plank_px, :] *= 0.95 + 0.08 * rng.random()
        offset = rng.integers(0, butt_every_px)
        for x in range(offset, width_px, butt_every_px):
            tone[row:row + plank_px, x:x + 1] *= 0.6
    return tone


def hull_planks() -> Path:
    """The hull's sides: 4 m along, 0 to 8 m up (the bottom row is the waterline), strakes 0.25 m,
    and a wet band darkening the lowest 0.9 m. Grey, multiplied by each hull's paint."""
    width, height = 256, 512  # 64 px a metre
    tone = _planks(width, height, plank_px=16, butt_every_px=192, seed=31)
    metres = (height - 1 - np.arange(height)) / 64.0
    wet = 0.6 + 0.4 * np.clip(metres / 0.9, 0, 1) ** 0.7
    return _save(_grey(tone * wet[:, None]), "T_HullPlanks")


def deck_planks() -> Path:
    """The deck: planks 0.25 m wide running along 4 m, tiling. Grey, multiplied by the deck's colour."""
    return _save(_grey(_planks(256, 256, plank_px=16, butt_every_px=128, seed=41)), "T_DeckPlanks")


def _wood(name: str, tone: np.ndarray, height: np.ndarray, strength: float) -> Path:
    """A grey wood texture, multiplied by its material's colour, and its normal map, `<name>_N`."""
    normal_map(height, strength).save(_save(_grey(tone), name).with_name(f"{name}_N.png"), optimize=False)
    return TEXTURES_DIR / f"{name}.png"


def spar_wood() -> Path:
    """Masts and spars: long grain running along u, 2 x 2 m a tile, and its normal map."""
    size = 256
    grain = tiling_height(size, waves=60, min_freq=6, max_freq=60, falloff=0.8, seed=51, max_freq_x=2)
    streaks = tiling_height(size, waves=16, min_freq=2, max_freq=8, falloff=1.0, seed=52, max_freq_x=1)
    return _wood("T_Spar", 0.88 + 0.07 * grain + 0.05 * streaks, grain, strength=2.0)


def ship_wood() -> Path:
    """Bulwarks, rails and deck furniture: boards 0.25 m wide along u with seams and butt joints, 2 x 2 m
    a tile, and its normal map."""
    tone = _planks(256, 256, plank_px=32, butt_every_px=256, seed=61)
    return _wood("T_ShipWood", tone, tone, strength=3.0)


def flag() -> Path:
    """A black flag with a white skull over crossed bones, 2:1.3."""
    w, h = 256, 166
    image = Image.new("RGB", (w, h), (18, 18, 20))
    draw = ImageDraw.Draw(image)
    white = (232, 228, 214)
    cx, cy = w // 2, h // 2 - 8
    for angle in (-1, 1):
        draw.line([(cx - 52, cy + 30 * angle + 22), (cx + 52, cy - 30 * angle + 22)], fill=white, width=14)
    draw.ellipse([cx - 30, cy - 38, cx + 30, cy + 18], fill=white)
    draw.rectangle([cx - 18, cy + 8, cx + 18, cy + 28], fill=white)
    for ex in (-12, 12):
        draw.ellipse([cx + ex - 8, cy - 16, cx + ex + 8, cy], fill=(18, 18, 20))
    return _save(image, "T_Flag")


def _ragged_blob(size: int, radius: float, roughness: float, rng: np.random.Generator) -> np.ndarray:
    """A mask in [0, 1] of a blob with a ragged edge, wider along x (the planks), centred in the cell."""
    y, x = (np.mgrid[0:size, 0:size] + 0.5) / size - 0.5
    angle = np.arctan2(y, x)
    edge = np.ones_like(angle)
    for k in range(3, 14):
        edge += roughness / k ** 0.6 * np.sin(k * angle + rng.uniform(0, 2 * np.pi))
    r = np.hypot(x / 1.15, y / 0.8)
    inside = np.maximum(np.abs(x), np.abs(y)) < 0.47  # clear of the cell's border, so mips do not bleed
    return ((r < radius * edge) & inside).astype(float)


def hull_marks() -> Path:
    """Damage on a hull's side, a 2 x 2 atlas: two splintered patches (bottom) and two holes (top).
    Bare wood with grain along x, dark cracks and splinters; a hole is a black-brown gap in a ring of
    broken planks. Alpha is cut: 0 outside the mark."""
    cell = 128
    rng = np.random.default_rng(51)
    rgba = np.zeros((2 * cell, 2 * cell, 4))
    grain = tiling_height(cell, waves=40, min_freq=6, max_freq=30, falloff=0.8, seed=52)
    bare = np.array([0.62, 0.48, 0.32])
    gap = np.array([0.08, 0.05, 0.03])
    for row in range(2):
        for col in range(2):
            hole = row == 1
            outer = _ragged_blob(cell, 0.31, 0.16, rng)
            colour = bare * (0.85 + 0.15 * grain[..., None])
            for _ in range(10):
                y0 = rng.integers(cell // 5, 4 * cell // 5)
                x0, x1 = sorted(rng.integers(cell // 8, 7 * cell // 8, size=2))
                colour[y0:y0 + 2, x0:x1] *= 0.45
            if hole:
                inner = _ragged_blob(cell, 0.2, 0.28, rng)
                colour = colour * (1 - inner[..., None]) + gap * inner[..., None]
            ys = slice((1 - row) * cell, (2 - row) * cell)
            xs = slice(col * cell, (col + 1) * cell)
            rgba[ys, xs, :3] = colour
            rgba[ys, xs, 3] = outer
    return _save(Image.fromarray(np.round(np.clip(rgba, 0, 1) * 255).astype(np.uint8), "RGBA"), "T_HullMarks")


def particles() -> Path:
    """Combat's particles, a 2 x 2 atlas in the order a texture sheet counts its frames, top left first:
    a soft puff of smoke, a flash (a bright core with spikes), a splinter (a thin shard) and a drop.
    White with alpha: each particle's colour tints it."""
    cell = 128
    rng = np.random.default_rng(61)
    y, x = (np.mgrid[0:cell, 0:cell] + 0.5) / cell - 0.5
    r = np.hypot(x, y)
    angle = np.arctan2(y, x)
    lumps = tiling_height(cell, waves=30, min_freq=2, max_freq=8, falloff=1.0, seed=62)
    puff_alpha = np.clip(1 - (r / 0.46) ** 2, 0, 1) ** 0.8 * (0.75 + 0.25 * lumps)
    puff_shade = 0.8 + 0.2 * np.clip(0.5 - y, 0, 1)  # lit from above
    spikes = 1 + 0.6 * np.sum([np.cos(k * angle + rng.uniform(0, 2 * np.pi)) ** 8 for k in (3, 5, 7)], axis=0)
    inside = np.maximum(np.abs(x), np.abs(y)) < 0.47  # clear of the cell's border, so mips do not bleed
    flash_alpha = np.clip(1 - r / (0.22 * spikes), 0, 1) ** 1.2 * inside
    shard = (np.abs(y + 0.15 * x) < 0.05 * (1 - np.abs(x) / 0.45)) & (np.abs(x) < 0.45)
    drop_alpha = np.clip(1 - r / 0.3, 0, 1) ** 0.7
    cells = [
        (np.ones_like(r) * puff_shade, puff_alpha),
        (np.ones_like(r), flash_alpha),
        (0.8 + 0.2 * lumps, shard.astype(float)),
        (np.ones_like(r), drop_alpha),
    ]
    rgba = np.zeros((2 * cell, 2 * cell, 4))
    for i, (shade, alpha) in enumerate(cells):
        ys = slice((i // 2) * cell, (i // 2 + 1) * cell)
        xs = slice((i % 2) * cell, (i % 2 + 1) * cell)
        rgba[ys, xs, :3] = shade[..., None]
        rgba[ys, xs, 3] = alpha
    return _save(Image.fromarray(np.round(np.clip(rgba, 0, 1) * 255).astype(np.uint8), "RGBA"), "T_Particles")


TEXTURES = {
    "water_normals": water_normals,
    "palette": palette,
    "sail_canvas": sail_canvas,
    "hull_planks": hull_planks,
    "deck_planks": deck_planks,
    "flag": flag,
    "hull_marks": hull_marks,
    "spar_wood": spar_wood,
    "ship_wood": ship_wood,
    "particles": particles,
}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("name", choices=sorted(TEXTURES))
    args = parser.parse_args()
    print(f"texture: {TEXTURES[args.name]()}")


if __name__ == "__main__":
    main()
