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
        private const int MaxMapCells = 21; // the full map's side at most, in cells (1 008 px at 48 a cell)

        [SerializeField] private WorldStreamer _streamer;
        [SerializeField] private SO_MapStyle _style;

        private WorldMapTiles _tiles;
        private Texture2D _full;
        private Color32[] _fullPixels;

        /// <summary>The world shown, or null before one begins.</summary>
        public WorldGenerator World => _streamer == null ? null : _streamer.World;

        /// <summary>The cells explored in it.</summary>
        public ExploredCells Explored => _streamer.Explored;

        /// <summary>Pixels along a cell's side.</summary>
        public int TileSize => _style.TileSize;

        /// <summary>Cells across the full map's view when it opens.</summary>
        public float ViewCells => _style.ViewCells;

        /// <summary>Cells across the full map's view zoomed in as far as it goes.</summary>
        public float ClosestCells => _style.ClosestCells;

        /// <summary>How the maps are drawn: the ground's colours, the ships' icons.</summary>
        public SO_MapStyle Style => _style;

        /// <summary>Wires the streamer and the style (maps built in code: tests).</summary>
        internal void Configure(WorldStreamer streamer, SO_MapStyle style)
        {
            _streamer = streamer;
            _style = style;
            _tiles = null;
        }

        private void OnDestroy()
        {
            if (_full != null)
            {
                Destroy(_full);
            }
        }

        /// <summary>
        /// Draws the explored cells into the full map's picture: the smallest square of cells
        /// holding them all, or <see cref="MaxMapCells"/> a side centred on
        /// <paramref name="around"/> when they spread wider. <paramref name="area"/> is the world
        /// square it covers (x and z of its south-west corner, its side in metres). Every tile not
        /// painted yet is painted now. The picture is kept and reused.
        /// </summary>
        public Texture2D ComposeExplored(CellCoord around, out Rect area)
        {
            int minX = around.X, maxX = around.X, minZ = around.Z, maxZ = around.Z;
            foreach (CellCoord cell in Explored.Cells)
            {
                minX = Mathf.Min(minX, cell.X);
                maxX = Mathf.Max(maxX, cell.X);
                minZ = Mathf.Min(minZ, cell.Z);
                maxZ = Mathf.Max(maxZ, cell.Z);
            }
            int width = maxX - minX + 1;
            int height = maxZ - minZ + 1;
            int side = Mathf.Max(width, height);
            CellCoord southWest;
            if (side > MaxMapCells)
            {
                side = MaxMapCells;
                southWest = new CellCoord(around.X - side / 2, around.Z - side / 2);
            }
            else
            {
                southWest = new CellCoord(minX - (side - width) / 2, minZ - (side - height) / 2);
            }

            int pixels = side * TileSize;
            if (_full == null)
            {
                _full = new Texture2D(pixels, pixels, TextureFormat.RGBA32, false)
                {
                    name = "WorldMap",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                };
            }
            else if (_full.width != pixels)
            {
                _full.Reinitialize(pixels, pixels);
            }
            if (_fullPixels == null || _fullPixels.Length != pixels * pixels)
            {
                _fullPixels = new Color32[pixels * pixels];
            }
            Compose(southWest, side, side, _fullPixels, int.MaxValue);
            _full.SetPixels32(_fullPixels);
            _full.Apply(false);

            float cellSize = World.Settings.CellSize;
            Vector2 corner = World.CellCentre(southWest) - Vector2.one * (cellSize * 0.5f);
            area = new Rect(corner, Vector2.one * (side * cellSize));
            return _full;
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
