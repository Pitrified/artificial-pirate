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

    /// <summary>
    /// One island: where it is, how large and high, and its own seed for its shape and, later,
    /// whatever is put on it. Its cell is its stable id.
    /// </summary>
    public readonly struct IslandSite
    {
        /// <summary>The cell the island is in; it is the island's id.</summary>
        public readonly CellCoord Cell;

        /// <summary>The centre on the water plane (x, z), in metres.</summary>
        public readonly Vector2 Centre;

        /// <summary>The radius at the waterline, before the coastline's wobble, in metres.</summary>
        public readonly float Radius;

        /// <summary>The highest the land rises above the waterline, in metres.</summary>
        public readonly float Peak;

        /// <summary>The island's own seed.</summary>
        public readonly int Seed;

        /// <summary>An island in <paramref name="cell"/>.</summary>
        public IslandSite(CellCoord cell, Vector2 centre, float radius, float peak, int seed)
        {
            Cell = cell;
            Centre = centre;
            Radius = radius;
            Peak = peak;
            Seed = seed;
        }
    }
}
