namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// One kind of ship: its hull's size, how it handles, and where the chase camera sits.
    /// A ship prefab variant points its components at its type. The hull's numbers are the model's
    /// at the waterline (Tools/asset_builders/builders); see docs/systems/ship-controller.md.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pirate/Ship Type", fileName = "SO_Ship_")]
    public sealed class SO_ShipType : ScriptableObject
    {
        [Header("Hull")]
        [Tooltip("From the centre to the bow and to the stern at the waterline (m).")]
        [SerializeField] private float _halfLength = 1f;

        [Tooltip("From the centre to either side at the waterline (m).")]
        [SerializeField] private float _halfBeam = 0.5f;

        [Tooltip("Keel below the waterline (m).")]
        [SerializeField] private float _draft = 0.5f;

        [Tooltip("Rail above the waterline amidships (m).")]
        [SerializeField] private float _freeboard = 0.5f;

        [Header("Handling")]
        [SerializeField] private float _maxSpeed = 8f;
        [SerializeField] private float _acceleration = 4f;
        [SerializeField] private float _reverseFactor = 0.3f;
        [SerializeField] private float _yawRateDeg = 60f;
        [SerializeField] private float _steeringResponseLerp = 6f;

        [Header("Camera")]
        [Tooltip("The chase camera's position in the ship's local space (m).")]
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 3f, -6f);

        [Tooltip("The point the chase camera looks at, in the ship's local space (m): above the deck, so the sails stay in view.")]
        [SerializeField] private Vector3 _cameraLookAt = Vector3.zero;

        [Header("Damage")]
        [Tooltip("A blow closing slower than this does no harm (m/s).")]
        [SerializeField] private float _harmlessSpeed = 1.5f;

        [Tooltip("The closing speed of a blow against a shore that sinks the hull at once (m/s); a blow takes the square of its speed over the square of this.")]
        [SerializeField] private float _sinkingSpeed = 14f;

        [Tooltip("The share of the top speed left when the hull is ruined.")]
        [SerializeField] private float _damagedSpeedFloor = 0.4f;

        [Tooltip("The share of the freeboard the hull settles by when ruined.")]
        [SerializeField] private float _damagedSinkShare = 0.5f;

        [Tooltip("The list toward the damaged side when the hull is ruined with all its damage on one side (degrees).")]
        [SerializeField] private float _damagedListDeg = 8f;

        [Tooltip("How long a ruined hull takes to go under (s).")]
        [SerializeField] private float _sinkSeconds = 8f;

        [Tooltip("How deep a ruined hull goes before it is gone, masts and all (m).")]
        [SerializeField] private float _sinkDepth = 30f;

        [Tooltip("How far a sinking hull tips toward the end that was last hit (degrees).")]
        [SerializeField] private float _sinkPitchDeg = 25f;

        /// <summary>From the centre to the bow and to the stern at the waterline, in metres.</summary>
        public float HalfLength => _halfLength;

        /// <summary>From the centre to either side at the waterline, in metres.</summary>
        public float HalfBeam => _halfBeam;

        /// <summary>Keel below the waterline, in metres.</summary>
        public float Draft => _draft;

        /// <summary>Rail above the waterline amidships, in metres.</summary>
        public float Freeboard => _freeboard;

        /// <summary>Maximum forward speed in metres per second.</summary>
        public float MaxSpeed => _maxSpeed;

        /// <summary>Acceleration rate in metres per second squared.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Multiplier on MaxSpeed applied to reverse throttle.</summary>
        public float ReverseFactor => _reverseFactor;

        /// <summary>Yaw rotation rate in degrees per second at full steering input.</summary>
        public float YawRateDeg => _yawRateDeg;

        /// <summary>Per-second Lerp factor for smoothing the raw steering input.</summary>
        public float SteeringResponseLerp => _steeringResponseLerp;

        /// <summary>The chase camera's position in the ship's local space, in metres.</summary>
        public Vector3 CameraOffset => _cameraOffset;

        /// <summary>The point the chase camera looks at, in the ship's local space, in metres.</summary>
        public Vector3 CameraLookAt => _cameraLookAt;

        /// <summary>A blow closing slower than this does no harm, in m/s.</summary>
        public float HarmlessSpeed => _harmlessSpeed;

        /// <summary>The closing speed of a blow against a shore that sinks the hull at once, in m/s.</summary>
        public float SinkingSpeed => _sinkingSpeed;

        /// <summary>The share of the top speed left when the hull is ruined.</summary>
        public float DamagedSpeedFloor => _damagedSpeedFloor;

        /// <summary>The share of the freeboard the hull settles by when ruined.</summary>
        public float DamagedSinkShare => _damagedSinkShare;

        /// <summary>The list toward the damaged side when the hull is ruined all on one side, in degrees.</summary>
        public float DamagedListDeg => _damagedListDeg;

        /// <summary>How long a ruined hull takes to go under, in seconds.</summary>
        public float SinkSeconds => _sinkSeconds;

        /// <summary>How deep a ruined hull goes before it is gone, in metres.</summary>
        public float SinkDepth => _sinkDepth;

        /// <summary>How far a sinking hull tips toward the end that was last hit, in degrees.</summary>
        public float SinkPitchDeg => _sinkPitchDeg;

        /// <summary>Sets the hull's size (types made in code: tests).</summary>
        internal void SetHull(float halfLength, float halfBeam, float draft, float freeboard)
        {
            _halfLength = halfLength;
            _halfBeam = halfBeam;
            _draft = draft;
            _freeboard = freeboard;
        }
    }
}
