namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// The wake of a hull moving through the water: every frame it moves, a small dent in the
    /// ripples at the hull's centre and foam at the stern and both bow shoulders, both in
    /// proportion to its speed and to the frame's time, so the wake is the same at any frame rate.
    /// Dents stamped every step merge instead of spreading as separate rings; faster than the
    /// ripples travel, they form a V behind the hull. Speed comes from the change in position on
    /// the plane, so it sits on the ship's root, which a kinematic controller moves.
    /// See docs/systems/water.md.
    /// </summary>
    public sealed class WakeSource : MonoBehaviour
    {
        // Faster than this between two frames is a teleport (a loaded save), not a movement.
        private const float TeleportSpeed = 40f;

        // Where the foam goes, in half beams (radius) and half lengths (the shoulders ahead of the centre).
        private const float SternFoamRadius = 1.2f;
        private const float ShoulderFoamRadius = 0.6f;
        private const float ShoulderAhead = 0.5f;

        [SerializeField] private WaterRipples _ripples;

        [Tooltip("The hull's size, which sets where the foam goes and how wide the dent is.")]
        [SerializeField] private SO_ShipType _type;

        [Tooltip("Speed in m/s under which it leaves no wake.")]
        [SerializeField] private float _minSpeed = 0.5f;

        [Tooltip("Radius of the dent it pushes into the ripples, in half beams.")]
        [SerializeField] private float _dentRadiusHalfBeams = 1.2f;

        [Tooltip("Dent depth per m/s of speed, per second.")]
        [SerializeField] private float _dentStrength = 0.03f;

        [Tooltip("Foam stamped per m/s of speed, per second (1 is full foam).")]
        [SerializeField] private float _foamRate = 1f;

        private Transform _transform;
        private Vector3 _last;
        private bool _hasLast;

        /// <summary>Wires the ripples and the hull's size (sources made at runtime: tests).</summary>
        internal void Configure(WaterRipples ripples, SO_ShipType type)
        {
            _ripples = ripples;
            _type = type;
            _hasLast = false;
        }

        private void Awake()
        {
            _transform = transform;
        }

        private void OnEnable()
        {
            _hasLast = false;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>One frame of <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (_ripples == null || _type == null || dt <= 0f)
            {
                return;
            }
            if (_transform == null)
            {
                _transform = transform;
            }
            Vector3 position = _transform.position;
            if (!_hasLast)
            {
                _last = position;
                _hasLast = true;
                return;
            }
            Vector3 moved = position - _last;
            moved.y = 0f;
            _last = position;
            float speed = moved.magnitude / dt;
            if (speed < _minSpeed || speed > TeleportSpeed)
            {
                return;
            }

            float halfBeam = _type.HalfBeam;
            Vector3 forward = _transform.forward * _type.HalfLength;
            Vector3 right = _transform.right * halfBeam;
            _ripples.AddImpulse(position, halfBeam * _dentRadiusHalfBeams, -speed * _dentStrength * dt);

            float foam = speed * _foamRate * dt;
            _ripples.AddFoam(position - forward, halfBeam * SternFoamRadius, foam);
            Vector3 shoulder = position + forward * ShoulderAhead;
            _ripples.AddFoam(shoulder + right, halfBeam * ShoulderFoamRadius, foam);
            _ripples.AddFoam(shoulder - right, halfBeam * ShoulderFoamRadius, foam);
        }
    }
}
