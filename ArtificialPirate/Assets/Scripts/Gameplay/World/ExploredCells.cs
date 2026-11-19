namespace Game.Gameplay.World
{
    using System.Collections.Generic;

    /// <summary>
    /// The cells the player has seen: what the maps show. Kept by cell, as the islands are, so
    /// later state (an island visited, looted) can be kept the same way. <see cref="Version"/>
    /// changes whenever a cell is added, so a map can tell when to redraw without an event.
    /// </summary>
    public sealed class ExploredCells
    {
        private readonly HashSet<CellCoord> _cells = new HashSet<CellCoord>();

        /// <summary>How many cells have been seen.</summary>
        public int Count => _cells.Count;

        /// <summary>Goes up by one each time cells are added.</summary>
        public int Version { get; private set; }

        /// <summary>The cells seen, for saving and drawing.</summary>
        public IReadOnlyCollection<CellCoord> Cells => _cells;

        /// <summary>Whether <paramref name="cell"/> has been seen.</summary>
        public bool Contains(CellCoord cell) => _cells.Contains(cell);

        /// <summary>Marks the cells within <paramref name="radius"/> of <paramref name="centre"/> as seen.</summary>
        public void Reveal(CellCoord centre, int radius)
        {
            bool added = false;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    added |= _cells.Add(new CellCoord(centre.X + dx, centre.Z + dz));
                }
            }
            if (added)
            {
                Version++;
            }
        }

        /// <summary>Marks <paramref name="cell"/> as seen (a save restored).</summary>
        public void Add(CellCoord cell)
        {
            if (_cells.Add(cell))
            {
                Version++;
            }
        }

        /// <summary>Forgets every cell (a new world).</summary>
        public void Clear()
        {
            if (_cells.Count > 0)
            {
                _cells.Clear();
                Version++;
            }
        }
    }
}
