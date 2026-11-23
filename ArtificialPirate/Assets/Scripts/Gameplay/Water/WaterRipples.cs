namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// Ripples on the water over a square: a height field in two render textures, stepped a
    /// fixed number of times per second through the wave equation by blitting one into the other
    /// (WaterRippleSim.shader), with a foam channel for wakes that fades step by step.
    /// <see cref="RippleSource"/>s add impulses, <see cref="WakeSource"/>s impulses and foam; the
    /// water shader samples the field for normal bending and foam. The square stays put, or follows
    /// a target whole texels at a time, the field shifted so ripples keep their place in the world.
    /// Ripples are visual only: <see cref="WaveField"/> does not see them. Each step runs when it is
    /// blitted, so tests can step and read it at once.
    /// See docs/systems/water.md.
    /// </summary>
    public sealed class WaterRipples : MonoBehaviour
    {
        private const int MaxImpulses = 16;
        private const int MaxFoamStamps = 16;
        private const int MaxStepsPerFrame = 4;

        private static readonly int ImpulsesId = Shader.PropertyToID("_Impulses");
        private static readonly int ImpulseCountId = Shader.PropertyToID("_ImpulseCount");
        private static readonly int FoamStampsId = Shader.PropertyToID("_FoamStamps");
        private static readonly int FoamCountId = Shader.PropertyToID("_FoamCount");
        private static readonly int DampingId = Shader.PropertyToID("_Damping");
        private static readonly int FoamFadeId = Shader.PropertyToID("_FoamFade");
        private static readonly int FoamSpreadId = Shader.PropertyToID("_FoamSpread");
        private static readonly int ShiftId = Shader.PropertyToID("_Shift");
        private static readonly int RipplesId = Shader.PropertyToID("_WaterRipples");
        private static readonly int RippleRectId = Shader.PropertyToID("_WaterRippleRect");

        [SerializeField] private Material _simulation;

        [Tooltip("Centre of the rippling square, world X and Z; where it starts when it follows a target.")]
        [SerializeField] private Vector2 _centre = new Vector2(2f, 40f);

        [Tooltip("Optional: the square follows it (the ship), so its ripples and wake are always in the square.")]
        [SerializeField] private Transform _follow;

        [Tooltip("Side of the rippling square, metres.")]
        [SerializeField] private float _size = 48f;

        [SerializeField] private int _resolution = 256;

        [Tooltip("Simulation steps per second; with the texel size this sets how fast ripples travel.")]
        [SerializeField] private float _stepsPerSecond = 30f;

        [Tooltip("Height kept per step, 0 to 1: lower dies out sooner (0.97 halves a ring in about 0.75 s at 30 steps/s).")]
        [SerializeField] private float _damping = 0.97f;

        [Tooltip("Foam kept per step, 0 to 1: 0.985 halves the wake's foam in about 1.5 s at 30 steps/s.")]
        [SerializeField] private float _foamFade = 0.985f;

        [Tooltip("Share of a texel's foam that spreads to its neighbours per step: the wake widens as it fades.")]
        [SerializeField] private float _foamSpread = 0.15f;

        // Queued in world units (x, z, radius, amount) until the step, which may move the square.
        private readonly Vector4[] _impulses = new Vector4[MaxImpulses];
        private readonly Vector4[] _foamStamps = new Vector4[MaxFoamStamps];
        private readonly Vector4[] _stepImpulses = new Vector4[MaxImpulses];
        private readonly Vector4[] _stepFoam = new Vector4[MaxFoamStamps];
        private int _impulseCount;
        private int _foamCount;
        private RenderTexture _current;
        private RenderTexture _next;
        private float _stepDebt;

        /// <summary>The ripple height field as it is now (R height, G the step before, B foam).</summary>
        public RenderTexture Field => _current;

        /// <summary>The world square the ripples cover: x min, z min, size, and one texel in uv.</summary>
        public Vector4 Rect => new Vector4(_centre.x - _size * 0.5f, _centre.y - _size * 0.5f, _size, 1f / _resolution);

        /// <summary>
        /// Adds a bump of <paramref name="strength"/> (metres of height, roughly; negative for a crater)
        /// and <paramref name="radius"/> metres at a world position, at the next step. Positions outside
        /// the square at that step, and impulses beyond <see cref="MaxImpulses"/> in one step, are dropped.
        /// </summary>
        public void AddImpulse(Vector3 position, float radius, float strength)
        {
            if (_impulseCount < MaxImpulses)
            {
                _impulses[_impulseCount++] = new Vector4(position.x, position.z, radius, strength);
            }
        }

        /// <summary>
        /// Adds <paramref name="amount"/> of foam (1 is full foam) in a round patch of
        /// <paramref name="radius"/> metres at a world position, at the next step; it then fades.
        /// Dropped like <see cref="AddImpulse"/>, beyond <see cref="MaxFoamStamps"/> in one step.
        /// </summary>
        public void AddFoam(Vector3 position, float radius, float amount)
        {
            if (_foamCount < MaxFoamStamps)
            {
                _foamStamps[_foamCount++] = new Vector4(position.x, position.z, radius, amount);
            }
        }

        /// <summary>Keeps the ripples' square round <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target) => _follow = target;

        /// <summary>Configures the area and steps in code (tests); <paramref name="follow"/> may be null.</summary>
        internal void Configure(Material simulation, Vector2 centre, float size, int resolution, float stepsPerSecond,
            Transform follow = null)
        {
            _simulation = simulation;
            _centre = centre;
            _follow = follow;
            _size = size;
            _resolution = resolution;
            _stepsPerSecond = stepsPerSecond;
            Release();
            Create();
        }

        /// <summary>
        /// Runs <paramref name="steps"/> simulation steps now. The first moves the square onto the
        /// followed target, if any, and takes the queued impulses and foam.
        /// </summary>
        public void Step(int steps)
        {
            if (_current == null)
            {
                return;
            }
            Vector2 shift = Recentre();
            Vector4 rect = Rect;
            int impulses = ToSquare(_impulses, _impulseCount, _stepImpulses, rect);
            int foam = ToSquare(_foamStamps, _foamCount, _stepFoam, rect);
            _impulseCount = 0;
            _foamCount = 0;

            _simulation.SetVectorArray(ImpulsesId, _stepImpulses);
            _simulation.SetVectorArray(FoamStampsId, _stepFoam);
            _simulation.SetFloat(DampingId, _damping);
            _simulation.SetFloat(FoamFadeId, _foamFade);
            _simulation.SetFloat(FoamSpreadId, _foamSpread);
            for (int i = 0; i < steps; i++)
            {
                bool first = i == 0;
                _simulation.SetVector(ShiftId, first ? shift : Vector2.zero);
                _simulation.SetFloat(ImpulseCountId, first ? impulses : 0);
                _simulation.SetFloat(FoamCountId, first ? foam : 0);
                Graphics.Blit(_current, _next, _simulation);
                (_current, _next) = (_next, _current);
            }
            Shader.SetGlobalTexture(RipplesId, _current);
            Shader.SetGlobalVector(RippleRectId, rect);
        }

        /// <summary>
        /// Moves the centre toward the followed target by whole texels, so the field's texels stay on
        /// one world grid; returns the move in uv, for the step to read the field where it was.
        /// </summary>
        private Vector2 Recentre()
        {
            if (_follow == null)
            {
                return Vector2.zero;
            }
            float texel = _size / _resolution;
            Vector3 target = _follow.position;
            float dx = Mathf.Round((target.x - _centre.x) / texel);
            float dz = Mathf.Round((target.z - _centre.y) / texel);
            _centre += new Vector2(dx, dz) * texel;
            return new Vector2(dx, dz) / _resolution;
        }

        /// <summary>
        /// Copies the queued world-unit stamps into <paramref name="into"/> in the square's uv,
        /// dropping those outside it; returns how many were kept.
        /// </summary>
        private static int ToSquare(Vector4[] queued, int count, Vector4[] into, Vector4 rect)
        {
            int kept = 0;
            for (int i = 0; i < count; i++)
            {
                Vector4 q = queued[i];
                float u = (q.x - rect.x) / rect.z;
                float v = (q.y - rect.y) / rect.z;
                if (u < 0f || u > 1f || v < 0f || v > 1f)
                {
                    continue;
                }
                into[kept++] = new Vector4(u, v, q.z / rect.z, q.w);
            }
            return kept;
        }

        private void OnEnable()
        {
            Create();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Update()
        {
            if (_current == null)
            {
                return;
            }
            _stepDebt += Time.deltaTime * _stepsPerSecond;
            int steps = Mathf.FloorToInt(_stepDebt);
            if (steps <= 0)
            {
                return;
            }
            _stepDebt -= steps;
            Step(Mathf.Min(steps, MaxStepsPerFrame));
        }

        private void Create()
        {
            if (_simulation == null || _current != null)
            {
                return;
            }
            if (_follow != null)
            {
                _centre = new Vector2(_follow.position.x, _follow.position.z); // empty yet: nothing to shift
            }
            _current = MakeTexture("WaterRipplesA");
            _next = MakeTexture("WaterRipplesB");
            Shader.SetGlobalTexture(RipplesId, _current);
            Shader.SetGlobalVector(RippleRectId, Rect);
        }

        private RenderTexture MakeTexture(string textureName)
        {
            var texture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGBHalf)
            {
                name = textureName,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.Create();
            // Start flat: a new render texture's contents are undefined.
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
            return texture;
        }

        private void Release()
        {
            Shader.SetGlobalVector(RippleRectId, Vector4.zero);
            ReleaseTexture(_current);
            ReleaseTexture(_next);
            _current = null;
            _next = null;
        }

        private static void ReleaseTexture(RenderTexture texture)
        {
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }
        }
    }
}
