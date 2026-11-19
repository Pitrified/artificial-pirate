namespace Game.Gameplay.World
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The world from a seed. The plane is cut into square cells, cell (0, 0) centred on the
    /// origin; each cell's draws come from a hash of the seed and its coordinates, so any cell can
    /// be asked for in any order and always comes out the same. A cell holds at most one island,
    /// centred anywhere in it and free to spill into the next cells but no further, so
    /// <see cref="HeightAt"/> asks the 3 x 3 cells around a point and takes the highest ground.
    /// The origin's cell always holds the start island, with open water kept around it. A cell's
    /// island is kept once worked out. See docs/systems/world.md.
    /// </summary>
    public sealed class WorldGenerator
    {
        private const int StartIslandSeed = 1; // the start island has the same shape in every world
        private const int StreamSalt = 0x2C1B3C6D;
        private const int HillSalt = 0x68E31DA4;
        private const int WarpXSalt = 0x1B56C4E9;
        private const int WarpZSalt = 0x7F4A7C15;
        private const int CoastOctaves = 2;
        private const int HillOctaves = 3;
        private const int WarpOctaves = 2;
        private const float CoastFeature = 0.5f; // coastline bays and points about half a radius apart
        private const float HillFeature = 0.35f;
        private const float WarpFeature = 1.6f;

        // Each draw a cell makes has its own stream, so adding one later moves none of the others.
        private const int PresenceStream = 1;
        private const int RadiusStream = 2;
        private const int PeakStream = 3;
        private const int OffsetXStream = 4;
        private const int OffsetZStream = 5;
        private const int SeedStream = 6;
        private const int StretchStream = 7;
        private const int AngleStream = 8;
        private const int LobeCountStream = 9;
        private const int LobeBXStream = 10;
        private const int LobeBZStream = 11;
        private const int LobeBSizeStream = 12;
        private const int LobeCXStream = 13;
        private const int LobeCZStream = 14;
        private const int LobeCSizeStream = 15;

        private readonly SO_WorldSettings _settings;
        private readonly int _seed;
        private readonly Dictionary<CellCoord, (bool has, IslandSite site)> _sites = new Dictionary<CellCoord, (bool, IslandSite)>();

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

        /// <summary>The island of <paramref name="cell"/>, if it has one.</summary>
        public bool TryGetIsland(CellCoord cell, out IslandSite site)
        {
            if (!_sites.TryGetValue(cell, out (bool has, IslandSite site) entry))
            {
                entry.has = Make(cell, out entry.site);
                _sites.Add(cell, entry);
            }
            site = entry.site;
            return entry.has;
        }

        /// <summary>Whether any island reaches into <paramref name="cell"/>.</summary>
        public bool AnyIslandOver(CellCoord cell)
        {
            float half = _settings.CellSize * 0.5f;
            var area = new Rect(CellCentre(cell) - Vector2.one * half, Vector2.one * (2f * half));
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (TryGetIsland(new CellCoord(cell.X + dx, cell.Z + dz), out IslandSite site) && site.Bounds.Overlaps(area))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// The ground's height at (<paramref name="x"/>, <paramref name="z"/>), in metres; the
        /// waterline is 0. The highest of the islands that reach the point, or the open seabed.
        /// </summary>
        public float HeightAt(float x, float z)
        {
            CellCoord cell = CellAt(x, z);
            var point = new Vector2(x, z);
            float height = _settings.Seabed;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (TryGetIsland(new CellCoord(cell.X + dx, cell.Z + dz), out IslandSite site) && site.Bounds.Contains(point))
                    {
                        height = Mathf.Max(height, IslandHeight(site, x, z));
                    }
                }
            }
            return height;
        }

        /// <summary>
        /// The ground's height from <paramref name="site"/> alone. The point is first pushed
        /// about by noise (the warp), then measured against each lobe: an ellipse along the
        /// island's length whose edge wanders with noise. Inside the highest lobe the land is a
        /// cone varied by hills and raised to a power; outside, a smooth slope to the seabed.
        /// </summary>
        public float IslandHeight(in IslandSite site, float x, float z)
        {
            float radius = site.Radius;
            float warpScale = 1f / (radius * WarpFeature);
            float warp = site.Warp * radius;
            float px = x + (2f * WorldHash.Fractal(site.Seed ^ WarpXSalt, x * warpScale, z * warpScale, WarpOctaves) - 1f) * warp;
            float pz = z + (2f * WorldHash.Fractal(site.Seed ^ WarpZSalt, x * warpScale, z * warpScale, WarpOctaves) - 1f) * warp;

            float coastScale = 1f / (radius * CoastFeature);
            float coast = 1f + _settings.CoastWobble * (2f * WorldHash.Fractal(site.Seed, px * coastScale, pz * coastScale, CoastOctaves) - 1f);

            float dx = px - site.Centre.x;
            float dz = pz - site.Centre.y;
            float cos = Mathf.Cos(site.Angle);
            float sin = Mathf.Sin(site.Angle);
            float along = dx * cos + dz * sin;
            float across = -dx * sin + dz * cos;

            float rise = LobeRise(site, along, across, coast, Vector2.zero, 1f);
            if (site.LobeB.Size > 0f)
            {
                rise = Mathf.Max(rise, LobeRise(site, along, across, coast, site.LobeB.Offset, site.LobeB.Size));
            }
            if (site.LobeC.Size > 0f)
            {
                rise = Mathf.Max(rise, LobeRise(site, along, across, coast, site.LobeC.Offset, site.LobeC.Size));
            }

            if (rise > 0f)
            {
                float hillScale = 1f / (radius * HillFeature);
                float hill = WorldHash.Fractal(site.Seed ^ HillSalt, x * hillScale, z * hillScale, HillOctaves);
                float land = rise * Mathf.Lerp(1f, 2f * hill, _settings.Hills);
                return site.Peak * Mathf.Pow(Mathf.Clamp01(land), _settings.Redistribution);
            }

            float t = Mathf.Clamp01(-rise * radius / _settings.ShelfWidth);
            return _settings.Seabed * t * t * (3f - 2f * t);
        }

        /// <summary>A lobe's rise: its size at its centre, 0 at its wandering edge, negative beyond (radii).</summary>
        private static float LobeRise(in IslandSite site, float along, float across, float coast, Vector2 offset, float size)
        {
            float u = (along - offset.x * site.Radius) / site.Stretch;
            float v = across - offset.y * site.Radius;
            float distance = Mathf.Sqrt(u * u + v * v) / (site.Radius * size * coast);
            return (1f - distance) * size;
        }

        private bool Make(CellCoord cell, out IslandSite site)
        {
            if (cell.X == 0 && cell.Z == 0)
            {
                site = Site(cell, Vector2.zero, _settings.StartRadius, _settings.StartPeak, 1f, 0f, default, default, 0f, StartIslandSeed);
                return true;
            }
            site = default;
            if (Draw(cell, PresenceStream) >= _settings.IslandChance)
            {
                return false;
            }

            float cellSize = _settings.CellSize;
            Vector2 centre = CellCentre(cell) + new Vector2(Draw(cell, OffsetXStream) - 0.5f, Draw(cell, OffsetZStream) - 0.5f) * cellSize;
            Vector2 radii = _settings.Radius;
            float radius = Mathf.Lerp(radii.x, radii.y, Draw(cell, RadiusStream));
            Vector2 peaks = _settings.Peak;
            float peak = Mathf.Lerp(peaks.x, peaks.y, Draw(cell, PeakStream));
            float stretch = Mathf.Lerp(1f, _settings.Stretch, Draw(cell, StretchStream));
            float angle = Draw(cell, AngleStream) * Mathf.PI;
            int lobes = Mathf.Min(Mathf.FloorToInt(Draw(cell, LobeCountStream) * (_settings.Lobes + 1)), _settings.Lobes);
            IslandLobe lobeB = lobes >= 1 ? Lobe(cell, LobeBXStream, LobeBZStream, LobeBSizeStream) : default;
            IslandLobe lobeC = lobes >= 2 ? Lobe(cell, LobeCXStream, LobeCZStream, LobeCSizeStream) : default;
            int seed = (int)WorldHash.Hash(StreamSeed(SeedStream), cell.X, cell.Z);

            site = Site(cell, centre, radius, peak, stretch, angle, lobeB, lobeC, _settings.Warp, seed);

            // No further than the next cell: the bounds grow with the radius plus the shelf, so shrink the radius to fit.
            float reach = Reach(site);
            float shelf = ShelfPad(site);
            if (reach > cellSize)
            {
                radius *= (cellSize - shelf) / (reach - shelf);
                site = Site(cell, centre, radius, peak, stretch, angle, lobeB, lobeC, _settings.Warp, seed);
            }

            // Open water around the start island.
            Rect bounds = site.Bounds;
            float nearestX = Mathf.Clamp(0f, bounds.xMin, bounds.xMax);
            float nearestZ = Mathf.Clamp(0f, bounds.yMin, bounds.yMax);
            return nearestX * nearestX + nearestZ * nearestZ >= _settings.StartClearing * _settings.StartClearing;
        }

        private IslandLobe Lobe(CellCoord cell, int xStream, int zStream, int sizeStream)
        {
            var offset = new Vector2(Draw(cell, xStream) * 2f - 1f, Draw(cell, zStream) * 2f - 1f) * _settings.LobeSpread;
            Vector2 sizes = _settings.LobeSize;
            return new IslandLobe(offset, Mathf.Lerp(sizes.x, sizes.y, Draw(cell, sizeStream)));
        }

        /// <summary>The farthest the site's bounds reach from its centre along x or z.</summary>
        private static float Reach(in IslandSite site)
        {
            Rect b = site.Bounds;
            return Mathf.Max(
                Mathf.Max(site.Centre.x - b.xMin, b.xMax - site.Centre.x),
                Mathf.Max(site.Centre.y - b.yMin, b.yMax - site.Centre.y));
        }

        /// <summary>The part of the bounds that does not grow with the radius: the seabed slope along the island's length.</summary>
        private float ShelfPad(in IslandSite site)
        {
            return _settings.ShelfWidth * site.Stretch * (1f + _settings.CoastWobble);
        }

        /// <summary>A site with its bounds: every lobe's ellipse at its widest coastline, turned, plus the warp and the shelf.</summary>
        private IslandSite Site(
            CellCoord cell, Vector2 centre, float radius, float peak, float stretch, float angle,
            IslandLobe lobeB, IslandLobe lobeC, float warp, int seed)
        {
            float widest = 1f + _settings.CoastWobble;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector2 min = centre;
            Vector2 max = centre;
            Grow(new IslandLobe(Vector2.zero, 1f));
            if (lobeB.Size > 0f)
            {
                Grow(lobeB);
            }
            if (lobeC.Size > 0f)
            {
                Grow(lobeC);
            }

            float pad = warp * radius + _settings.ShelfWidth * stretch * widest;
            var bounds = Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad);
            return new IslandSite(cell, centre, radius, peak, stretch, angle, lobeB, lobeC, warp, bounds, seed);

            void Grow(IslandLobe lobe)
            {
                float along = lobe.Offset.x * radius;
                float across = lobe.Offset.y * radius;
                var at = new Vector2(centre.x + along * cos - across * sin, centre.y + along * sin + across * cos);
                float a = radius * lobe.Size * widest * stretch;
                float b = radius * lobe.Size * widest;
                float halfX = Mathf.Sqrt(a * a * cos * cos + b * b * sin * sin);
                float halfZ = Mathf.Sqrt(a * a * sin * sin + b * b * cos * cos);
                min = Vector2.Min(min, at - new Vector2(halfX, halfZ));
                max = Vector2.Max(max, at + new Vector2(halfX, halfZ));
            }
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
