namespace Game.Gameplay
{
    /// <summary>Saved ship position and heading.</summary>
    [System.Serializable]
    public struct ShipSaveData
    {
        /// <summary>World-space X position.</summary>
        public float x;

        /// <summary>World-space Y position.</summary>
        public float y;

        /// <summary>World-space Z position.</summary>
        public float z;

        /// <summary>Yaw angle in degrees.</summary>
        public float yawDeg;
    }

    /// <summary>A cell of the world, by its coordinates.</summary>
    [System.Serializable]
    public struct CellSaveData
    {
        /// <summary>Column, along world X.</summary>
        public int x;

        /// <summary>Row, along world Z.</summary>
        public int z;
    }

    /// <summary>The world: its seed and the cells the player has seen.</summary>
    [System.Serializable]
    public struct WorldSaveData
    {
        /// <summary>The world's seed; 0 when no world was saved.</summary>
        public int seed;

        /// <summary>The cells explored, in no order.</summary>
        public CellSaveData[] explored;
    }

    /// <summary>
    /// The game's save payload, written by <c>Game.Systems.SaveSystem</c> inside its envelope.
    /// Must stay in sync with the JSON schema in docs/functional-specs.md §9.
    /// </summary>
    [System.Serializable]
    public struct GameSaveData
    {
        /// <summary>
        /// Payload schema version, passed to the save system on every load and save.
        /// Increment on any breaking field change; a mismatch resets the save.
        /// 2: the payload moved inside the save system's envelope (1 had the ship at the top level).
        /// 3: the world's seed and explored cells.
        /// </summary>
        public const int CurrentVersion = 3;

        /// <summary>Ship state at the time of save.</summary>
        public ShipSaveData ship;

        /// <summary>The world at the time of save.</summary>
        public WorldSaveData world;
    }
}
