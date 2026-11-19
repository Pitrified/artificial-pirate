namespace Game.Gameplay.World
{
    using UnityEngine;

    /// <summary>A cell's address: integer coordinates, cell (0, 0) centred on the origin.</summary>
    public readonly struct CellCoord : System.IEquatable<CellCoord>
    {
        /// <summary>Column, along world X.</summary>
        public readonly int X;

        /// <summary>Row, along world Z.</summary>
        public readonly int Z;

        /// <summary>The cell at column <paramref name="x"/> and row <paramref name="z"/>.</summary>
        public CellCoord(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <inheritdoc/>
        public bool Equals(CellCoord other) => X == other.X && Z == other.Z;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is CellCoord other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => (int)WorldHash.Hash(0, X, Z);

        /// <inheritdoc/>
        public override string ToString() => $"({X}, {Z})";
    }

    /// <summary>One lobe of an island: where its centre is from the island's, and how large it is, in radii.</summary>
    public readonly struct IslandLobe
    {
        /// <summary>The lobe's centre from the island's, in radii (x, z).</summary>
        public readonly Vector2 Offset;

        /// <summary>The lobe's size, as a share of the radius; 0 for no lobe.</summary>
        public readonly float Size;

        /// <summary>A lobe at <paramref name="offset"/> of <paramref name="size"/>.</summary>
        public IslandLobe(Vector2 offset, float size)
        {
            Offset = offset;
            Size = size;
        }
    }

    /// <summary>
    /// One island: where it is, how large, long, turned and high, its lobes, the world area it
    /// can touch, and its own seed for its coastline and, later, whatever is put on it. Its cell
    /// is its stable id.
    /// </summary>
    public readonly struct IslandSite
    {
        /// <summary>The cell the island belongs to; it is the island's id.</summary>
        public readonly CellCoord Cell;

        /// <summary>The centre on the water plane (x, z), in metres.</summary>
        public readonly Vector2 Centre;

        /// <summary>The main lobe's radius at the waterline, before stretch and wobble, in metres.</summary>
        public readonly float Radius;

        /// <summary>The highest the land rises above the waterline, in metres.</summary>
        public readonly float Peak;

        /// <summary>How long the island is against its width (1 is round).</summary>
        public readonly float Stretch;

        /// <summary>The direction of its length, in radians from world X toward Z.</summary>
        public readonly float Angle;

        /// <summary>The second lobe; size 0 if none.</summary>
        public readonly IslandLobe LobeB;

        /// <summary>The third lobe; size 0 if none.</summary>
        public readonly IslandLobe LobeC;

        /// <summary>How far noise pushes the shape about, as a share of the radius.</summary>
        public readonly float Warp;

        /// <summary>The world area (x, z) outside which the island is open seabed.</summary>
        public readonly Rect Bounds;

        /// <summary>The island's own seed.</summary>
        public readonly int Seed;

        /// <summary>An island in <paramref name="cell"/>.</summary>
        public IslandSite(
            CellCoord cell, Vector2 centre, float radius, float peak, float stretch, float angle,
            IslandLobe lobeB, IslandLobe lobeC, float warp, Rect bounds, int seed)
        {
            Cell = cell;
            Centre = centre;
            Radius = radius;
            Peak = peak;
            Stretch = stretch;
            Angle = angle;
            LobeB = lobeB;
            LobeC = lobeC;
            Warp = warp;
            Bounds = bounds;
            Seed = seed;
        }
    }
}
