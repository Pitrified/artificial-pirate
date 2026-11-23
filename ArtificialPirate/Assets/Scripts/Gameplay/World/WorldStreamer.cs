namespace Game.Gameplay.World
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Keeps the islands around a target (the player's ship) built: the cells within
    /// <see cref="_loadRadius"/> of the target's cell are loaded, and a cell left behind is
    /// unloaded. Islands are built one a frame, nearest first, into pooled objects with a mesh
    /// collider. The cells within <see cref="_seeRadius"/> of the target's are marked explored.
    /// See docs/systems/world.md.
    /// </summary>
    public sealed class WorldStreamer : MonoBehaviour
    {
        [SerializeField] private SO_WorldSettings _settings;

        [Tooltip("What the world is built around: the player's ship.")]
        [SerializeField] private Transform _target;

        [Tooltip("The materials of an island's sand and grass submeshes.")]
        [SerializeField] private Material _sand;
        [SerializeField] private Material _grass;

        [Tooltip("Cells loaded each way from the target's cell: 2 is a 5 x 5 square.")]
        [SerializeField] private int _loadRadius = 2;

        [Tooltip("Cells marked explored each way from the target's cell: 1 is a 3 x 3 square.")]
        [SerializeField] private int _seeRadius = 1;

        [Tooltip("The world built when nothing hands it a seed (the scene played on its own).")]
        [SerializeField] private int _seed = 1;

        private readonly Dictionary<CellCoord, IslandTile> _loaded = new Dictionary<CellCoord, IslandTile>();
        private readonly Stack<IslandTile> _pool = new Stack<IslandTile>();
        private readonly List<CellCoord> _queue = new List<CellCoord>();
        private readonly List<CellCoord> _leaving = new List<CellCoord>();
        private readonly IslandMeshBuilder _builder = new IslandMeshBuilder();
        private readonly ExploredCells _explored = new ExploredCells();
        private Material[] _materials;
        private WorldGenerator _world;
        private CellCoord _centre;
        private bool _centred;

        /// <summary>The world being built, from <see cref="Begin"/>.</summary>
        public WorldGenerator World => _world;

        /// <summary>The cells the player has seen in this world.</summary>
        public ExploredCells Explored => _explored;

        /// <summary>The cells with an island built, for tests.</summary>
        internal int LoadedCount => _loaded.Count;

        /// <summary>Whether the island in <paramref name="cell"/> is built, for tests.</summary>
        internal bool IsLoaded(CellCoord cell) => _loaded.ContainsKey(cell);

        /// <summary>Islands waiting to be built, for tests.</summary>
        internal int QueuedCount => _queue.Count;

        /// <summary>Streams the world around <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target) => _target = target;

        /// <summary>Wires the settings, the target and the materials (streamers built in code: tests).</summary>
        internal void Configure(SO_WorldSettings settings, Transform target, Material sand, Material grass, int loadRadius)
        {
            _settings = settings;
            _target = target;
            _sand = sand;
            _grass = grass;
            _loadRadius = loadRadius;
            _materials = null;
        }

        private void Awake()
        {
            Begin(_seed);
        }

        private void Update()
        {
            Tick();
        }

        /// <summary>Starts the world of <paramref name="seed"/>, dropping the islands built and the cells explored before.</summary>
        public void Begin(int seed)
        {
            foreach (IslandTile tile in _loaded.Values)
            {
                Release(tile);
            }
            _loaded.Clear();
            _queue.Clear();
            _explored.Clear();
            _centred = false;
            _world = _settings == null ? null : new WorldGenerator(seed, _settings);
        }

        /// <summary>
        /// One frame: re-centres on the target's cell when it changes, then builds at most one
        /// island. Exposed internally for tests.
        /// </summary>
        internal void Tick()
        {
            if (_world == null || _target == null)
            {
                return;
            }

            Vector3 at = _target.position;
            CellCoord cell = _world.CellAt(at.x, at.z);
            if (!_centred || !cell.Equals(_centre))
            {
                Recentre(cell);
            }
            BuildNext();
        }

        private bool InRange(CellCoord cell)
        {
            return Mathf.Abs(cell.X - _centre.X) <= _loadRadius && Mathf.Abs(cell.Z - _centre.Z) <= _loadRadius;
        }

        private void Recentre(CellCoord cell)
        {
            _centre = cell;
            _centred = true;
            _explored.Reveal(cell, _seeRadius);

            _leaving.Clear();
            foreach (CellCoord loaded in _loaded.Keys)
            {
                if (!InRange(loaded))
                {
                    _leaving.Add(loaded);
                }
            }
            foreach (CellCoord leaving in _leaving)
            {
                Release(_loaded[leaving]);
                _loaded.Remove(leaving);
            }

            // Ring by ring outward, so the nearest islands are built first.
            _queue.Clear();
            for (int ring = 0; ring <= _loadRadius; ring++)
            {
                for (int dx = -ring; dx <= ring; dx++)
                {
                    for (int dz = -ring; dz <= ring; dz++)
                    {
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)) != ring)
                        {
                            continue;
                        }
                        var candidate = new CellCoord(cell.X + dx, cell.Z + dz);
                        if (!_loaded.ContainsKey(candidate) && _world.TryGetIsland(candidate, out _))
                        {
                            _queue.Add(candidate);
                        }
                    }
                }
            }
        }

        private void BuildNext()
        {
            if (_queue.Count == 0)
            {
                return;
            }
            CellCoord cell = _queue[0];
            _queue.RemoveAt(0);
            if (!_world.TryGetIsland(cell, out IslandSite site))
            {
                return;
            }

            IslandTile tile = _pool.Count > 0 ? _pool.Pop() : CreateTile();
            _builder.Build(_world, site, tile.Mesh);
            tile.Collider.sharedMesh = null; // re-assigning the same mesh is what makes the collider re-bake
            tile.Collider.sharedMesh = tile.Mesh;
            tile.Object.transform.position = new Vector3(site.Centre.x, 0f, site.Centre.y);
            tile.Object.name = $"Island {cell}";
            tile.Object.SetActive(true);
            _loaded.Add(cell, tile);
        }

        private IslandTile CreateTile()
        {
            _materials ??= new[] { _sand, _grass };
            var go = new GameObject("Island");
            go.transform.SetParent(transform, false);
            var mesh = new Mesh { name = "Island" };
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = _materials;
            MeshCollider collider = go.AddComponent<MeshCollider>();
            return new IslandTile(go, mesh, collider);
        }

        private void Release(IslandTile tile)
        {
            tile.Object.SetActive(false);
            _pool.Push(tile);
        }

        private void OnDestroy()
        {
            foreach (IslandTile tile in _loaded.Values)
            {
                Destroy(tile.Mesh);
            }
            foreach (IslandTile tile in _pool)
            {
                Destroy(tile.Mesh);
            }
        }

        private sealed class IslandTile
        {
            public readonly GameObject Object;
            public readonly Mesh Mesh;
            public readonly MeshCollider Collider;

            public IslandTile(GameObject go, Mesh mesh, MeshCollider collider)
            {
                Object = go;
                Mesh = mesh;
                Collider = collider;
            }
        }
    }
}
