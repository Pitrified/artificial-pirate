namespace Game.Gameplay
{
    using System;
    using UnityEngine;

    /// <summary>
    /// What firing and hitting look like, one world-space particle system per kind for the whole
    /// scene: a flash and smoke at a muzzle when a ball is fired, splinters and a puff of dust where
    /// one strikes a hull, a splash where one falls in the water, dirt where one lands on the ground,
    /// and the smoke a badly damaged ship gives off (<see cref="DamageSmoke"/>). Each burst is
    /// emitted particle by particle, each with its own direction within a cone, so a system can be
    /// used anywhere; the particles' size, life, colour and fall are the systems' own.
    /// See docs/systems/combat.md.
    /// </summary>
    public sealed class CombatEffects : MonoBehaviour
    {
        [SerializeField] private Cannonballs _balls;

        [SerializeField] private ParticleSystem _flash;
        [SerializeField] private ParticleSystem _smoke;
        [SerializeField] private ParticleSystem _splinters;
        [SerializeField] private ParticleSystem _splash;
        [SerializeField] private ParticleSystem _dirt;

        [SerializeField] private Burst _muzzleFlash = new Burst { Count = 3, ConeDeg = 15f, MinSpeed = 4f, MaxSpeed = 8f, Ahead = 0.3f };
        [SerializeField] private Burst _muzzleSmoke = new Burst { Count = 8, ConeDeg = 30f, MinSpeed = 1.5f, MaxSpeed = 4f, Ahead = 0.6f };
        [SerializeField] private Burst _hullSplinters = new Burst { Count = 16, ConeDeg = 60f, MinSpeed = 4f, MaxSpeed = 9f };
        [SerializeField] private Burst _hullDust = new Burst { Count = 4, ConeDeg = 40f, MinSpeed = 0.5f, MaxSpeed = 1.5f, Colour = new Color(0.75f, 0.66f, 0.55f, 0.7f) };
        [SerializeField] private Burst _waterSplash = new Burst { Count = 20, ConeDeg = 20f, MinSpeed = 5f, MaxSpeed = 12f };
        [SerializeField] private Burst _groundDirt = new Burst { Count = 8, ConeDeg = 50f, MinSpeed = 2f, MaxSpeed = 5f };
        [SerializeField] private Burst _damageSmoke = new Burst { Count = 3, ConeDeg = 15f, MinSpeed = 1f, MaxSpeed = 2f, Colour = new Color(0.35f, 0.34f, 0.33f, 0.85f), Lifetime = 6f };

        [Tooltip("Seeds the directions and speeds.")]
        [SerializeField] private int _seed = 5;

        private System.Random _random;

        /// <summary>How many particles a burst emits and how: within a cone around its direction, at a speed between two, from a point some way ahead.</summary>
        [Serializable]
        public struct Burst
        {
            /// <summary>Particles per burst.</summary>
            public int Count;

            /// <summary>Half the cone's angle around the burst's direction (degrees).</summary>
            public float ConeDeg;

            /// <summary>The slowest a particle leaves (m/s).</summary>
            public float MinSpeed;

            /// <summary>The fastest a particle leaves (m/s).</summary>
            public float MaxSpeed;

            /// <summary>How far ahead of the point, along the direction, the burst starts (m).</summary>
            public float Ahead;

            /// <summary>The particles' colour in place of the system's; none (alpha 0) keeps the system's.</summary>
            public Color Colour;

            /// <summary>The particles' life in place of the system's (s); 0 keeps the system's.</summary>
            public float Lifetime;
        }

        private void Awake()
        {
            _random = new System.Random(_seed);
        }

        /// <summary>Wires the effects in code (tests).</summary>
        internal void Configure(Cannonballs balls, ParticleSystem flash, ParticleSystem smoke, ParticleSystem splinters, ParticleSystem splash, ParticleSystem dirt)
        {
            OnDisable();
            _balls = balls;
            _flash = flash;
            _smoke = smoke;
            _splinters = splinters;
            _splash = splash;
            _dirt = dirt;
            OnEnable();
        }

        private void OnEnable()
        {
            if (_balls == null)
            {
                return;
            }
            _balls.Launched += OnLaunched;
            _balls.HitHull += OnHitHull;
            _balls.HitWater += OnHitWater;
            _balls.HitGround += OnHitGround;
        }

        private void OnDisable()
        {
            if (_balls == null)
            {
                return;
            }
            _balls.Launched -= OnLaunched;
            _balls.HitHull -= OnHitHull;
            _balls.HitWater -= OnHitWater;
            _balls.HitGround -= OnHitGround;
        }

        /// <summary>Smoke rising from <paramref name="point"/>, <paramref name="times"/> the damage smoke's burst.</summary>
        public void Smoke(Vector3 point, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Emit(_smoke, _damageSmoke, point, Vector3.up);
            }
        }

        private void OnLaunched(Vector3 muzzle, Vector3 direction)
        {
            Emit(_flash, _muzzleFlash, muzzle, direction);
            Emit(_smoke, _muzzleSmoke, muzzle, direction);
        }

        private void OnHitHull(Vector3 point, Vector3 normal)
        {
            Emit(_splinters, _hullSplinters, point, normal);
            Emit(_smoke, _hullDust, point, normal);
        }

        private void OnHitWater(Vector3 point) => Emit(_splash, _waterSplash, point, Vector3.up);

        private void OnHitGround(Vector3 point) => Emit(_dirt, _groundDirt, point, Vector3.up);

        private void Emit(ParticleSystem system, Burst burst, Vector3 point, Vector3 direction)
        {
            if (system == null)
            {
                return;
            }
            _random ??= new System.Random(_seed);
            Vector3 axis = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.up;
            Vector3 side = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.99f ? Vector3.up : Vector3.right).normalized;
            Vector3 start = point + axis * burst.Ahead;
            var emit = new ParticleSystem.EmitParams { applyShapeToPosition = false };
            if (burst.Colour.a > 0f)
            {
                emit.startColor = burst.Colour;
            }
            if (burst.Lifetime > 0f)
            {
                emit.startLifetime = burst.Lifetime;
            }
            for (int i = 0; i < burst.Count; i++)
            {
                float tilt = burst.ConeDeg * Mathf.Sqrt(Next());
                float turn = 360f * Next();
                Vector3 heading = Quaternion.AngleAxis(turn, axis) * (Quaternion.AngleAxis(tilt, side) * axis);
                emit.position = start;
                emit.velocity = heading * Mathf.Lerp(burst.MinSpeed, burst.MaxSpeed, Next());
                system.Emit(emit, 1);
            }
        }

        private float Next() => (float)_random.NextDouble();
    }
}
