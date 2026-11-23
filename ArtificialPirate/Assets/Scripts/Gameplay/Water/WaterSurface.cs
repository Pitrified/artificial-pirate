namespace Game.Gameplay.Water
{
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// One body of water: owns the <see cref="WaveField"/> and its clock, and hands the shader
    /// the same waves, the same time and the seabed depth as globals every frame, so what floats
    /// and what is drawn agree. Things that float hold a reference to it. Given a target, its grid
    /// follows it in whole quads, and the drawn waves calm to nothing toward the grid's coarse
    /// outer part. Runs in the Editor too, so a scene capture sees the waves.
    /// See docs/systems/water.md.
    /// </summary>
    [ExecuteAlways]
    public sealed class WaterSurface : MonoBehaviour
    {
        private static readonly int WaveA = Shader.PropertyToID("_WaterWaveA");
        private static readonly int WaveB = Shader.PropertyToID("_WaterWaveB");
        private static readonly int WaveC = Shader.PropertyToID("_WaterWaveC");
        private static readonly int WaveD = Shader.PropertyToID("_WaterWaveD");
        private static readonly int WaterTime = Shader.PropertyToID("_WaterTime");
        private static readonly int SeaLevelId = Shader.PropertyToID("_WaterSeaLevel");
        private static readonly int Shallow = Shader.PropertyToID("_WaterShallow");
        private static readonly int Deep = Shader.PropertyToID("_WaterDeep");
        private static readonly int Sky = Shader.PropertyToID("_WaterSky");
        private static readonly int Foam = Shader.PropertyToID("_WaterFoam");
        private static readonly int Detail = Shader.PropertyToID("_WaterDetail");
        private static readonly int FoamParams = Shader.PropertyToID("_WaterFoamParams");
        private static readonly int DepthMap = Shader.PropertyToID("_WaterDepthMap");
        private static readonly int DepthRectId = Shader.PropertyToID("_WaterDepthRect");
        private static readonly int FarCalmId = Shader.PropertyToID("_WaterFarCalm");

        [SerializeField] private SO_WaterSettings _settings;

        [Tooltip("Seabed depth under the sea level: 0 at the surface, 1 at Depth Range. Handed over at runtime by the world.")]
        [SerializeField] private Texture2D _depthMap;

        [Tooltip("World area the depth map covers: x min, z min, size (m), depth range (m).")]
        [SerializeField] private Vector4 _depthRect = new Vector4(-90f, -90f, 180f, 8f);

        [Tooltip("What the grid follows (the player's ship); none, and it stays put.")]
        [SerializeField] private Transform _follow;

        [Tooltip("The grid's fine quad (m): it moves by whole quads, so its waves do not swim.")]
        [SerializeField] private float _followStep = 2f;

        [Tooltip("The drawn waves calm from this distance from the grid's centre (m)...")]
        [SerializeField] private float _farCalmStart = 100f;

        [Tooltip("...to nothing at this one, inside the fine core's edge (m).")]
        [SerializeField] private float _farCalmEnd = 124f;

        private WaveField _field;
        private bool _pinned;
        private float _pinnedTime;

        /// <summary>The waves, for anything that needs the surface on the CPU.</summary>
        public WaveField Waves => _field ??= new WaveField(_settings);

        /// <summary>The water clock in seconds: the level's time, unless a capture pinned it.</summary>
        public float Now => _pinned ? _pinnedTime : Time.timeSinceLevelLoad;

        /// <summary>Height of the still surface.</summary>
        public float SeaLevel => _settings.SeaLevel;

        /// <summary>The settings this water draws and moves with.</summary>
        public SO_WaterSettings Settings => _settings;

        /// <summary>The world area and depth range the seabed depth map covers.</summary>
        public Vector4 DepthRect => _depthRect;

        /// <summary>The surface height above a world position, now.</summary>
        public float HeightAt(Vector3 position) => Waves.HeightAt(position.x, position.z, Now);

        /// <summary>Freezes the water clock at <paramref name="time"/>, for repeatable captures and tests.</summary>
        public void PinClock(float time)
        {
            _pinned = true;
            _pinnedTime = time;
            Push();
        }

        /// <summary>Lets the clock run with the level's time again.</summary>
        public void ReleaseClock()
        {
            _pinned = false;
            Push();
        }

        /// <summary>Uses <paramref name="settings"/> (water built in code: tests).</summary>
        internal void UseSettings(SO_WaterSettings settings)
        {
            _settings = settings;
            _field = new WaveField(settings);
            Push();
        }

        /// <summary>Keeps the water's grid round <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target) => _follow = target;

        /// <summary>Makes the grid follow <paramref name="target"/> in steps of <paramref name="step"/> (water built in code: tests).</summary>
        internal void UseFollow(Transform target, float step)
        {
            _follow = target;
            _followStep = step;
        }

        /// <summary>Points the depth map at <paramref name="map"/>, covering <paramref name="rect"/>.</summary>
        public void SetDepthMap(Texture2D map, Vector4 rect)
        {
            _depthMap = map;
            _depthRect = rect;
            Push();
        }

        private void OnEnable()
        {
            if (_settings == null)
            {
                Log.Warn(LogCat.Gameplay, "WaterSurface: no settings assigned");
                return;
            }
            _field = new WaveField(_settings);
            Push();
        }

        private void OnValidate()
        {
            if (_settings != null)
            {
                _field = new WaveField(_settings);
                Push();
            }
        }

        private void Update()
        {
            Follow();
            if (_settings != null)
            {
                Push();
            }
        }

        /// <summary>Moves the grid under its target, in whole quads. Exposed internally for tests.</summary>
        internal void Follow()
        {
            if (_follow == null || _followStep <= 0f)
            {
                return;
            }
            Vector3 target = _follow.position;
            Transform grid = transform;
            grid.position = new Vector3(
                Mathf.Round(target.x / _followStep) * _followStep,
                grid.position.y,
                Mathf.Round(target.z / _followStep) * _followStep);
        }

        private void Push()
        {
            if (_settings == null)
            {
                return;
            }
            WaveField field = Waves;
            Shader.SetGlobalVector(WaveA, field.PackedWave(0));
            Shader.SetGlobalVector(WaveB, field.PackedWave(1));
            Shader.SetGlobalVector(WaveC, field.PackedWave(2));
            Shader.SetGlobalVector(WaveD, field.PackedWave(3));
            Shader.SetGlobalFloat(WaterTime, Now);
            Shader.SetGlobalFloat(SeaLevelId, _settings.SeaLevel);
            Shader.SetGlobalColor(Shallow, _settings.Shallow);
            Shader.SetGlobalColor(Deep, _settings.Deep);
            Shader.SetGlobalColor(Sky, _settings.Sky);
            Shader.SetGlobalColor(Foam, _settings.Foam);
            Shader.SetGlobalVector(Detail, new Vector4(
                _settings.NormalTiling, _settings.NormalStrength, _settings.NormalScroll, _settings.DeepAt));
            Shader.SetGlobalVector(FoamParams, new Vector4(
                _settings.ShoreFoamDepth, _settings.ShoreBandSpeed, _settings.CrestFoam, _settings.CalmDepth));
            if (_depthMap != null)
            {
                Shader.SetGlobalTexture(DepthMap, _depthMap);
            }
            Shader.SetGlobalVector(DepthRectId, _depthRect);
            Vector3 centre = transform.position;
            Shader.SetGlobalVector(FarCalmId, new Vector4(centre.x, centre.z, _farCalmStart, _farCalmEnd));
        }
    }
}
