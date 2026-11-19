namespace Game.Gameplay.World
{
    using UnityEngine;

    /// <summary>
    /// How the world is generated: the cells, how often and how large islands come, their shape,
    /// and the start island every world has at the origin. See docs/systems/world.md.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pirate/World Settings", fileName = "SO_WorldSettings")]
    public sealed class SO_WorldSettings : ScriptableObject
    {
        [Header("Cells")]
        [Tooltip("Side of a cell (m). A cell holds at most one island, centred anywhere in it; it may spill into the next cells.")]
        [SerializeField] private float _cellSize = 400f;

        [Tooltip("The share of cells with an island, 0 to 1.")]
        [Range(0f, 1f)]
        [SerializeField] private float _islandChance = 0.5f;

        [Header("Islands")]
        [Tooltip("Island radius at the waterline (m), smallest and largest, before stretch and lobes; shrunk if it would reach past the next cell.")]
        [SerializeField] private Vector2 _radius = new Vector2(30f, 110f);

        [Tooltip("Island peak above the waterline (m), lowest and highest.")]
        [SerializeField] private Vector2 _peak = new Vector2(4f, 40f);

        [Tooltip("How long an island is against its width, at most (1 is round).")]
        [SerializeField] private float _stretch = 2f;

        [Tooltip("Extra lobes an island may have, at most.")]
        [SerializeField] private int _lobes = 2;

        [Tooltip("How far a lobe's centre lies from the island's, at most, in radii.")]
        [SerializeField] private float _lobeSpread = 1.3f;

        [Tooltip("A lobe's size, smallest and largest, as a share of the radius.")]
        [SerializeField] private Vector2 _lobeSize = new Vector2(0.35f, 0.75f);

        [Tooltip("How far the coastline wanders in and out, as a share of the radius.")]
        [SerializeField] private float _coastWobble = 0.3f;

        [Tooltip("How far the shape is pushed about by noise before it is measured, as a share of the radius: bays, spits and bends.")]
        [SerializeField] private float _warp = 0.6f;

        [Tooltip("How much hills and hollows vary the land, 0 a smooth cone to 1 strongly varied.")]
        [Range(0f, 1f)]
        [SerializeField] private float _hills = 0.5f;

        [Tooltip("Power on the land's height: over 1 gives flat shores and a sharper peak.")]
        [SerializeField] private float _redistribution = 1.6f;

        [Tooltip("Width of the slope from the waterline down to the seabed (m).")]
        [SerializeField] private float _shelfWidth = 25f;

        [Tooltip("The open sea's floor (m, negative).")]
        [SerializeField] private float _seabed = -20f;

        [Header("Start island")]
        [Tooltip("The island at the origin in every world: its radius and peak (m).")]
        [SerializeField] private float _startRadius = 24f;
        [SerializeField] private float _startPeak = 8f;

        [Tooltip("Open water kept around the origin (m): no other island reaches inside it.")]
        [SerializeField] private float _startClearing = 160f;

        /// <summary>Side of a cell, in metres.</summary>
        public float CellSize => _cellSize;

        /// <summary>The share of cells with an island.</summary>
        public float IslandChance => _islandChance;

        /// <summary>Smallest (x) and largest (y) island radius, in metres.</summary>
        public Vector2 Radius => _radius;

        /// <summary>Lowest (x) and highest (y) island peak, in metres.</summary>
        public Vector2 Peak => _peak;

        /// <summary>The most an island is stretched (1 is round).</summary>
        public float Stretch => _stretch;

        /// <summary>The most extra lobes an island has.</summary>
        public int Lobes => _lobes;

        /// <summary>How far a lobe's centre lies from the island's, at most, in radii.</summary>
        public float LobeSpread => _lobeSpread;

        /// <summary>Smallest (x) and largest (y) lobe, as a share of the radius.</summary>
        public Vector2 LobeSize => _lobeSize;

        /// <summary>How far the coastline wanders, as a share of the radius.</summary>
        public float CoastWobble => _coastWobble;

        /// <summary>How far noise pushes the shape about, as a share of the radius.</summary>
        public float Warp => _warp;

        /// <summary>How much the land's height varies from a smooth cone, 0 to 1.</summary>
        public float Hills => _hills;

        /// <summary>Power on the land's height.</summary>
        public float Redistribution => _redistribution;

        /// <summary>Width of the slope from the waterline to the seabed, in metres.</summary>
        public float ShelfWidth => _shelfWidth;

        /// <summary>The open sea's floor, in metres (negative).</summary>
        public float Seabed => _seabed;

        /// <summary>The start island's radius, in metres.</summary>
        public float StartRadius => _startRadius;

        /// <summary>The start island's peak, in metres.</summary>
        public float StartPeak => _startPeak;

        /// <summary>Open water kept around the origin, in metres.</summary>
        public float StartClearing => _startClearing;
    }
}
