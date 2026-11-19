namespace Game.Gameplay.World
{
    using UnityEngine;

    /// <summary>
    /// The world as the maps show it: the streamer's world drawn as <see cref="WorldMapTiles"/>,
    /// with the cells not yet explored dark. The minimap and the full map compose their pictures
    /// from it, so a cell is painted once for both. See docs/systems/world.md.
    /// </summary>
    public sealed class WorldMap : MonoBehaviour
    {
        [SerializeField] private WorldStreamer _streamer;
        [SerializeField] private SO_MapStyle _style;

        private WorldMapTiles _tiles;

        /// <summary>The world shown, or null before one begins.</summary>
        public WorldGenerator World => _streamer == null ? null : _streamer.World;

        /// <summary>The cells explored in it.</summary>
        public ExploredCells Explored => _streamer.Explored;

        /// <summary>Pixels along a cell's side.</summary>
        public int TileSize => _style.TileSize;

        /// <summary>Wires the streamer and the style (maps built in code: tests).</summary>
        internal void Configure(WorldStreamer streamer, SO_MapStyle style)
        {
            _streamer = streamer;
            _style = style;
            _tiles = null;
        }

        /// <summary>
        /// Draws the <paramref name="columns"/> x <paramref name="rows"/> cells from
        /// <paramref name="southWest"/> into <paramref name="pixels"/> (rows from the south, a tile's
        /// pixels a cell), painting at most <paramref name="paintBudget"/> tiles not painted before;
        /// a tile it could not paint yet is left as it was. Returns whether every cell was drawn.
        /// </summary>
        public bool Compose(CellCoord southWest, int columns, int rows, Color32[] pixels, int paintBudget)
        {
            WorldGenerator world = World;
            if (world == null)
            {
                return false;
            }
            if (_tiles == null || _tiles.World != world)
            {
                _tiles = new WorldMapTiles(world, _style);
            }

            int size = _tiles.TileSize;
            int width = columns * size;
            bool complete = true;
            for (int cz = 0; cz < rows; cz++)
            {
                for (int cx = 0; cx < columns; cx++)
                {
                    var cell = new CellCoord(southWest.X + cx, southWest.Z + cz);
                    int origin = cz * size * width + cx * size;
                    if (!_streamer.Explored.Contains(cell))
                    {
                        Fill(pixels, origin, width, size, _tiles.Unexplored);
                        continue;
                    }
                    if (!_tiles.TryGet(cell, out Color32[] tile))
                    {
                        if (paintBudget <= 0)
                        {
                            complete = false;
                            continue;
                        }
                        paintBudget--;
                        tile = _tiles.Get(cell);
                    }
                    for (int row = 0; row < size; row++)
                    {
                        System.Array.Copy(tile, row * size, pixels, origin + row * width, size);
                    }
                }
            }
            return complete;
        }

        private static void Fill(Color32[] pixels, int origin, int width, int size, Color32 colour)
        {
            for (int row = 0; row < size; row++)
            {
                int start = origin + row * width;
                for (int column = 0; column < size; column++)
                {
                    pixels[start + column] = colour;
                }
            }
        }
    }
}
