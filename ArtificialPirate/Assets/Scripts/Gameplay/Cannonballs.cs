namespace Game.Gameplay
{
    using System;
    using UnityEngine;
    using Game.Gameplay.Water;
    using Game.Gameplay.World;
    using Game.Systems;

    /// <summary>
    /// The scene's cannonballs, a fixed pool every <see cref="Broadside"/> fires from. A ball flies on
    /// its arc under gravity, with no physics body; each step a ray from where it was to where it is
    /// finds the first hull on the ships' layers that is not its own ship's, and the hull's
    /// <see cref="ShipDamage"/> takes the firer type's ball damage over its own type's strength at the
    /// point struck. A ball that reaches the ground (the world's height) or the water ends there,
    /// a splash on the water. Each end is raised as an event, for the effects.
    /// See docs/systems/combat.md.
    /// </summary>
    public sealed class Cannonballs : MonoBehaviour
    {
        private const int MaxRayHits = 8;

        [SerializeField] private WaterSurface _water;
        [SerializeField] private WaterRipples _ripples;
        [SerializeField] private WorldStreamer _world;

        [Tooltip("The layers that hold ships' hulls.")]
        [SerializeField] private LayerMask _ships;

        [Tooltip("The ball's mesh and material; none in code-made pools (tests).")]
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        [Tooltip("How many balls can be in the air at once.")]
        [SerializeField] private int _poolSize = 32;

        [Tooltip("The ball's radius (m).")]
        [SerializeField] private float _radius = 0.12f;

        [Tooltip("A ball still flying after this long is dropped (s).")]
        [SerializeField] private float _maxSeconds = 6f;

        [Tooltip("The splash's ring: its radius (m) and depth (m).")]
        [SerializeField] private float _splashRadius = 1.2f;
        [SerializeField] private float _splashDepth = 0.35f;

        [Tooltip("The foam a splash leaves: its radius (m) and amount (1 full foam).")]
        [SerializeField] private float _foamRadius = 1.5f;
        [SerializeField] private float _foamAmount = 0.8f;

        private readonly RaycastHit[] _rayHits = new RaycastHit[MaxRayHits];
        private Ball[] _balls;
        private bool _warnedFull;

        private struct Ball
        {
            public Transform View;
            public Broadside Firer;
            public Vector3 Start;
            public Vector3 Velocity;
            public Vector3 Last;
            public float Age;
            public bool Live;
        }

        /// <summary>A ball was fired: the muzzle and its direction.</summary>
        public event Action<Vector3, Vector3> Launched;

        /// <summary>A ball struck a hull: the point and the hull's outward normal there.</summary>
        public event Action<Vector3, Vector3> HitHull;

        /// <summary>A ball fell into the water: the point on the surface.</summary>
        public event Action<Vector3> HitWater;

        /// <summary>A ball struck the ground: the point.</summary>
        public event Action<Vector3> HitGround;

        /// <summary>How many balls are in the air.</summary>
        public int Flying
        {
            get
            {
                int count = 0;
                for (int i = 0; _balls != null && i < _balls.Length; i++)
                {
                    count += _balls[i].Live ? 1 : 0;
                }
                return count;
            }
        }

        private void Awake()
        {
            if (_balls == null)
            {
                Build();
            }
        }

        /// <summary>Wires and builds the pool in code (tests).</summary>
        internal void Configure(WaterSurface water, LayerMask ships, int poolSize)
        {
            _water = water;
            _ships = ships;
            _poolSize = poolSize;
            Build();
        }

        private void Build()
        {
            _balls = new Ball[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                if (_mesh == null || _material == null)
                {
                    continue;
                }
                var view = new GameObject($"Cannonball{i}");
                view.transform.SetParent(transform, false);
                view.transform.localScale = Vector3.one * (2f * _radius);
                view.AddComponent<MeshFilter>().sharedMesh = _mesh;
                view.AddComponent<MeshRenderer>().sharedMaterial = _material;
                view.SetActive(false);
                _balls[i].View = view.transform;
            }
        }

        /// <summary>
        /// Fires a ball from <paramref name="muzzle"/> at <paramref name="velocity"/> for
        /// <paramref name="firer"/>, whose own hull it passes through; false when every ball is flying.
        /// </summary>
        public bool Launch(Vector3 muzzle, Vector3 velocity, Broadside firer)
        {
            for (int i = 0; i < _balls.Length; i++)
            {
                if (_balls[i].Live)
                {
                    continue;
                }
                ref Ball ball = ref _balls[i];
                ball.Firer = firer;
                ball.Start = muzzle;
                ball.Velocity = velocity;
                ball.Last = muzzle;
                ball.Age = 0f;
                ball.Live = true;
                Show(ref ball, muzzle);
                Launched?.Invoke(muzzle, velocity.normalized);
                return true;
            }
            if (!_warnedFull)
            {
                _warnedFull = true;
                Log.Warn(LogCat.Gameplay, "Cannonballs: all {0} balls are flying; a shot is dropped", _balls.Length);
            }
            return false;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>Moves every flying ball by <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (_balls == null || dt <= 0f)
            {
                return;
            }
            for (int i = 0; i < _balls.Length; i++)
            {
                if (_balls[i].Live)
                {
                    Step(ref _balls[i], dt);
                }
            }
        }

        private void Step(ref Ball ball, float dt)
        {
            ball.Age += dt;
            Vector3 now = ball.Start + ball.Velocity * ball.Age + 0.5f * ball.Age * ball.Age * Physics.gravity;
            if (StrikeHull(ref ball, now) || StrikeGround(ref ball, now) || StrikeWater(ref ball, now))
            {
                return;
            }
            if (ball.Age > _maxSeconds)
            {
                End(ref ball);
                return;
            }
            ball.Last = now;
            Show(ref ball, now);
        }

        private bool StrikeHull(ref Ball ball, Vector3 now)
        {
            Vector3 path = now - ball.Last;
            float length = path.magnitude;
            if (length < Mathf.Epsilon)
            {
                return false;
            }
            int count = Physics.RaycastNonAlloc(ball.Last, path / length, _rayHits, length, _ships, QueryTriggerInteraction.Ignore);
            int nearest = -1;
            for (int i = 0; i < count; i++)
            {
                bool own = ball.Firer != null && _rayHits[i].collider.transform.IsChildOf(ball.Firer.transform);
                if (!own && (nearest < 0 || _rayHits[i].distance < _rayHits[nearest].distance))
                {
                    nearest = i;
                }
            }
            if (nearest < 0)
            {
                return false;
            }
            RaycastHit hit = _rayHits[nearest];
            ShipDamage damage = hit.collider.GetComponentInParent<ShipDamage>();
            if (damage != null && damage.Type != null && ball.Firer != null && ball.Firer.Type != null)
            {
                damage.Hit(hit.point, ball.Firer.Type.BallDamage / Mathf.Max(damage.Type.HullStrength, Mathf.Epsilon));
            }
            HitHull?.Invoke(hit.point, hit.normal);
            End(ref ball);
            return true;
        }

        private bool StrikeGround(ref Ball ball, Vector3 now)
        {
            WorldGenerator ground = _world != null ? _world.World : null;
            if (ground == null)
            {
                return false;
            }
            float height = ground.HeightAt(now.x, now.z);
            float sea = _water != null ? _water.SeaLevel : 0f;
            if (height <= sea || now.y > height)
            {
                return false;
            }
            HitGround?.Invoke(new Vector3(now.x, height, now.z));
            End(ref ball);
            return true;
        }

        private bool StrikeWater(ref Ball ball, Vector3 now)
        {
            if (_water == null)
            {
                return false;
            }
            float surface = _water.HeightAt(now);
            if (now.y > surface)
            {
                return false;
            }
            var point = new Vector3(now.x, surface, now.z);
            if (_ripples != null)
            {
                _ripples.AddImpulse(point, _splashRadius, -_splashDepth);
                _ripples.AddFoam(point, _foamRadius, _foamAmount);
            }
            HitWater?.Invoke(point);
            End(ref ball);
            return true;
        }

        private static void Show(ref Ball ball, Vector3 position)
        {
            if (ball.View == null)
            {
                return;
            }
            ball.View.position = position;
            if (!ball.View.gameObject.activeSelf)
            {
                ball.View.gameObject.SetActive(true);
            }
        }

        private static void End(ref Ball ball)
        {
            ball.Live = false;
            ball.Firer = null;
            if (ball.View != null)
            {
                ball.View.gameObject.SetActive(false);
            }
        }
    }
}
