namespace Game.Gameplay.World
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A world drawn as map tiles, one a cell, painted from the ground's height once and kept:
    /// rows of <see cref="SO_MapStyle.TileSize"/> pixels from the cell's south-west corner. Cells
    /// without an island share one open-sea tile. A tile is allocated the first time its island is
    /// painted. See docs/systems/world.md.
    /// </summary>
    public sealed class WorldMapTiles
    {
        private readonly WorldGenerator _world;
        private readonly SO_MapStyle _style;
        private readonly Dictionary<CellCoord, Color32[]> _tiles = new Dictionary<CellCoord, Color32[]>();
        private readonly Color32[] _openSea;

        /// <summary>Tiles of <paramref name="world"/> drawn in <paramref name="style"/>.</summary>
        public WorldMapTiles(WorldGenerator world, SO_MapStyle style)
        {
            _world = world;
            _style = style;
            _openSea = new Color32[style.TileSize * style.TileSize];
            Color32 sea = style.ColourAt(world.Settings.Seabed);
            for (int i = 0; i < _openSea.Length; i++)
            {
                _openSea[i] = sea;
            }
        }

        /// <summary>The world the tiles are of.</summary>
        public WorldGenerator World => _world;

        /// <summary>Pixels along a tile's side.</summary>
        public int TileSize => _style.TileSize;

        /// <summary>The colour of a cell not yet explored.</summary>
        public Color32 Unexplored => _style.Unexplored;

        /// <summary>The tile of <paramref name="cell"/> if it costs nothing now: open sea, or already painted.</summary>
        public bool TryGet(CellCoord cell, out Color32[] tile)
        {
            if (_tiles.TryGetValue(cell, out tile))
            {
                return true;
            }
            if (!_world.TryGetIsland(cell, out _))
            {
                tile = _openSea;
                return true;
            }
            return false;
        }

        /// <summary>The tile of <paramref name="cell"/>, painted now if it was not yet.</summary>
        public Color32[] Get(CellCoord cell)
        {
            if (TryGet(cell, out Color32[] tile))
            {
                return tile;
            }

            int size = _style.TileSize;
            float cellSize = _world.Settings.CellSize;
            float pixel = cellSize / size;
            Vector2 corner = _world.CellCentre(cell) - Vector2.one * (cellSize * 0.5f);
            tile = new Color32[size * size];
            for (int row = 0; row < size; row++)
            {
                float z = corner.y + (row + 0.5f) * pixel;
                for (int column = 0; column < size; column++)
                {
                    float x = corner.x + (column + 0.5f) * pixel;
                    tile[row * size + column] = _style.ColourAt(_world.HeightAt(x, z));
                }
            }
            _tiles.Add(cell, tile);
            return tile;
        }
    }
}
