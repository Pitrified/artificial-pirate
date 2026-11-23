namespace Game.Gameplay.World
{
    using UnityEngine;
    using Game.Gameplay.Water;

    /// <summary>
    /// The water's seabed depth map, made at runtime from the world's ground around a target (the
    /// player's ship), in place of a baked one: 0 at the surface, 1 at the depth range or deeper.
    /// A new world is drawn at once; when the target has sailed <see cref="_recentreDistance"/>
    /// from the map's centre, a map around it is filled a few rows a frame into the hidden one of
    /// two textures, then swapped in whole. Its outermost texels always read deep: the shader
    /// clamps past the edge, and an island crossing it would otherwise smear its shallows out to
    /// the horizon. See docs/systems/world.md.
    /// </summary>
    public sealed class WorldDepthMap : MonoBehaviour
    {
        private const int Resolution = 256;

        [SerializeField] private WaterSurface _water;
        [SerializeField] private WorldStreamer _streamer;

        [Tooltip("What the map is centred on: the player's ship.")]
        [SerializeField] private Transform _target;

        [Tooltip("Side of the area the map covers (m).")]
        [SerializeField] private float _size = 512f;

        [Tooltip("Depth at which the map reads 1 (m).")]
        [SerializeField] private float _depthRange = 8f;

        [Tooltip("How far the target sails from the map's centre before a new map is made (m).")]
        [SerializeField] private float _recentreDistance = 128f;

        [Tooltip("Rows of a new map filled a frame.")]
        [SerializeField] private int _rowsPerFrame = 16;

        private readonly Texture2D[] _maps = new Texture2D[2];
        private readonly byte[] _depths = new byte[Resolution * Resolution];
        private int _front;
        private WorldGenerator _world;
        private Vector2 _shownCentre;
        private Vector2 _fillingCentre;
        private int _fillRow = -1;

        /// <summary>The map the water reads now, for tests.</summary>
        internal Texture2D Shown => _maps[_front];

        /// <summary>Whether a new map is being filled, for tests.</summary>
        internal bool Filling => _fillRow >= 0;

        /// <summary>Wires the water, the world and the target (maps built in code: tests).</summary>
        /// <summary>Draws the depth around <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target) => _target = target;

        internal void Configure(WaterSurface water, WorldStreamer streamer, Transform target)
        {
            _water = water;
            _streamer = streamer;
            _target = target;
        }

        private void Update()
        {
            Tick();
        }

        private void OnDestroy()
        {
            foreach (Texture2D map in _maps)
            {
                if (map != null)
                {
                    Destroy(map);
                }
            }
        }

        /// <summary>One frame: a new world at once, or a few more rows of a map around the target.</summary>
        internal void Tick()
        {
            if (_water == null || _streamer == null || _streamer.World == null || _target == null)
            {
                return;
            }

            Vector3 at = _target.position;
            Vector2 here = new Vector2(at.x, at.z);
            if (_streamer.World != _world)
            {
                _world = _streamer.World;
                _fillRow = -1;
                Fill(Snap(here), 0, Resolution);
                Show(Snap(here));
                return;
            }

            if (_fillRow < 0 && (here - _shownCentre).sqrMagnitude > _recentreDistance * _recentreDistance)
            {
                _fillingCentre = Snap(here);
                _fillRow = 0;
            }
            if (_fillRow < 0)
            {
                return;
            }

            int rows = Mathf.Min(_rowsPerFrame, Resolution - _fillRow);
            Fill(_fillingCentre, _fillRow, rows);
            _fillRow += rows;
            if (_fillRow >= Resolution)
            {
                Show(_fillingCentre);
                _fillRow = -1;
            }
        }

        private float Texel => _size / Resolution;

        /// <summary>The centre moved to whole texels, so a texel always covers the same ground.</summary>
        private Vector2 Snap(Vector2 point)
        {
            float texel = Texel;
            return new Vector2(Mathf.Round(point.x / texel) * texel, Mathf.Round(point.y / texel) * texel);
        }

        private void Fill(Vector2 centre, int fromRow, int rows)
        {
            float texel = Texel;
            float left = centre.x - _size * 0.5f;
            float bottom = centre.y - _size * 0.5f;
            float seaLevel = _water.SeaLevel;
            for (int row = fromRow; row < fromRow + rows; row++)
            {
                float z = bottom + (row + 0.5f) * texel;
                for (int column = 0; column < Resolution; column++)
                {
                    float x = left + (column + 0.5f) * texel;
                    bool edge = row == 0 || column == 0 || row == Resolution - 1 || column == Resolution - 1;
                    float depth = edge ? 1f : Mathf.Clamp01((seaLevel - _world.HeightAt(x, z)) / _depthRange);
                    _depths[row * Resolution + column] = (byte)Mathf.RoundToInt(depth * byte.MaxValue);
                }
            }
        }

        private void Show(Vector2 centre)
        {
            int back = 1 - _front;
            if (_maps[back] == null)
            {
                _maps[back] = new Texture2D(Resolution, Resolution, TextureFormat.R8, false, true)
                {
                    name = "WorldDepthMap",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                };
            }
            _maps[back].SetPixelData(_depths, 0);
            _maps[back].Apply(false);
            _front = back;
            _shownCentre = centre;
            _water.SetDepthMap(_maps[_front], new Vector4(centre.x - _size * 0.5f, centre.y - _size * 0.5f, _size, _depthRange));
        }
    }
}
