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
