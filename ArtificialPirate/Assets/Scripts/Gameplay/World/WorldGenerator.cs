namespace Game.Gameplay.World
{
    using UnityEngine;

    /// <summary>
    /// The world from a seed. The plane is cut into square cells, cell (0, 0) centred on the
    /// origin; each cell's draws come from a hash of the seed and its coordinates, so any cell can
    /// be asked for in any order and always comes out the same. A cell holds at most one island,
    /// kept wholly inside it (a jittered grid), so <see cref="HeightAt"/> asks one cell only. The
    /// origin's cell always holds the start island. Plain C#: no scene objects, no allocations.
    /// See docs/systems/world.md.
    /// </summary>
    public sealed class WorldGenerator
    {
        private const int StartIslandSeed = 1; // the start island has the same shape in every world
        private const int StreamSalt = 0x2C1B3C6D;
        private const int HillSalt = 0x68E31DA4;
        private const int CoastOctaves = 2;
        private const int HillOctaves = 3;
        private const float CoastFeature = 0.5f; // coastline bays and points about half a radius apart
        private const float HillFeature = 0.35f;

        // Each draw a cell makes has its own stream, so adding one later moves none of the others.
        private const int PresenceStream = 1;
        private const int RadiusStream = 2;
        private const int PeakStream = 3;
        private const int OffsetXStream = 4;
        private const int OffsetZStream = 5;
        private const int SeedStream = 6;

        private readonly SO_WorldSettings _settings;
        private readonly int _seed;

        /// <summary>The world of <paramref name="seed"/>, shaped by <paramref name="settings"/>.</summary>
        public WorldGenerator(int seed, SO_WorldSettings settings)
        {
            _seed = seed;
            _settings = settings;
        }

        /// <summary>The world's seed.</summary>
        public int Seed => _seed;

        /// <summary>The settings it is shaped by.</summary>
        public SO_WorldSettings Settings => _settings;

        /// <summary>The cell holding the point (<paramref name="x"/>, <paramref name="z"/>).</summary>
        public CellCoord CellAt(float x, float z)
        {
            float size = _settings.CellSize;
            return new CellCoord(Mathf.FloorToInt(x / size + 0.5f), Mathf.FloorToInt(z / size + 0.5f));
        }

        /// <summary>The centre of <paramref name="cell"/> on the water plane (x, z).</summary>
        public Vector2 CellCentre(CellCoord cell)
        {
            return new Vector2(cell.X, cell.Z) * _settings.CellSize;
        }

        /// <summary>The island in <paramref name="cell"/>, if it has one.</summary>
        public bool TryGetIsland(CellCoord cell, out IslandSite site)
        {
            if (cell.X == 0 && cell.Z == 0)
            {
                site = new IslandSite(cell, Vector2.zero, _settings.StartRadius, _settings.StartPeak, StartIslandSeed);
                return true;
            }
            if (Draw(cell, PresenceStream) >= _settings.IslandChance)
            {
                site = default;
                return false;
            }

            float half = _settings.CellSize * 0.5f;
            float widest = (half - _settings.ShelfWidth) / (1f + _settings.CoastWobble);
            Vector2 radii = _settings.Radius;
            float radius = Mathf.Min(Mathf.Lerp(radii.x, radii.y, Draw(cell, RadiusStream)), widest);
            float slack = half - _settings.Reach(radius);
            Vector2 offset = new Vector2(Draw(cell, OffsetXStream) * 2f - 1f, Draw(cell, OffsetZStream) * 2f - 1f) * slack;
            Vector2 peaks = _settings.Peak;
            float peak = Mathf.Lerp(peaks.x, peaks.y, Draw(cell, PeakStream));
            int seed = (int)WorldHash.Hash(StreamSeed(SeedStream), cell.X, cell.Z);
            site = new IslandSite(cell, CellCentre(cell) + offset, radius, peak, seed);
            return true;
        }

        /// <summary>The ground's height at (<paramref name="x"/>, <paramref name="z"/>), in metres; the waterline is 0.</summary>
        public float HeightAt(float x, float z)
        {
            return TryGetIsland(CellAt(x, z), out IslandSite site) ? IslandHeight(site, x, z) : _settings.Seabed;
        }

        /// <summary>
        /// The ground's height from <paramref name="site"/> alone: a blob whose coastline wanders
        /// with noise, its land a cone varied by hills and raised to a power, and under the
        /// waterline a smooth slope to the seabed.
        /// </summary>
        public float IslandHeight(in IslandSite site, float x, float z)
        {
            float dx = x - site.Centre.x;
            float dz = z - site.Centre.y;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);

            float coastScale = 1f / (site.Radius * CoastFeature);
            float coast = WorldHash.Fractal(site.Seed, x * coastScale, z * coastScale, CoastOctaves);
            float coastline = site.Radius * (1f + _settings.CoastWobble * (2f * coast - 1f));

            if (distance < coastline)
            {
                float hillScale = 1f / (site.Radius * HillFeature);
                float hill = WorldHash.Fractal(site.Seed ^ HillSalt, x * hillScale, z * hillScale, HillOctaves);
                float rise = (1f - distance / coastline) * Mathf.Lerp(1f, 2f * hill, _settings.Hills);
                return site.Peak * Mathf.Pow(Mathf.Clamp01(rise), _settings.Redistribution);
            }

            float t = Mathf.Clamp01((distance - coastline) / _settings.ShelfWidth);
            return _settings.Seabed * t * t * (3f - 2f * t);
        }

        private float Draw(CellCoord cell, int stream)
        {
            return WorldHash.Unit(WorldHash.Hash(StreamSeed(stream), cell.X, cell.Z));
        }

        private int StreamSeed(int stream)
        {
            return unchecked(_seed ^ (stream * StreamSalt));
        }
    }
}
