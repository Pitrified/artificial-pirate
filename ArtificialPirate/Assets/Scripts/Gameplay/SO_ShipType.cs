namespace Game.Gameplay
{
    using System;
    using UnityEngine;
    using Game.Systems;

    /// <summary>Where a camera sits and what it looks at, both in the ship's space (m).</summary>
    [Serializable]
    public struct CameraRig
    {
        /// <summary>The camera's place.</summary>
        public Vector3 Offset;

        /// <summary>The point it looks at.</summary>
        public Vector3 LookAt;

        /// <summary>A rig from its two points.</summary>
        public CameraRig(Vector3 offset, Vector3 lookAt)
        {
            Offset = offset;
            LookAt = lookAt;
        }
    }

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

        [Tooltip("The hull's side above the water along its length, as a share of Half Beam, from -1 (the stern) to 1 (the bow) of Half Length; the builder's half_beam.")]
        [SerializeField] private AnimationCurve _beamAlongLength = AnimationCurve.Constant(-1f, 1f, 1f);

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

        [Tooltip("The bird's eye camera: high and a little astern, looking down toward the bow.")]
        [SerializeField] private CameraRig _birdsEye = new CameraRig(new Vector3(0f, 45f, -22f), new Vector3(0f, 0f, 14f));

        [Tooltip("The helm camera: the helmsman's eyes (the builder's report prints them), looking ahead over the bow.")]
        [SerializeField] private CameraRig _helm = new CameraRig(new Vector3(0f, 3f, -7f), new Vector3(0f, 3f, 60f));

        [Header("Map")]
        [Tooltip("The ship seen from above, bow up, white on clear (textures.py map_icons); the maps tint and size it.")]
        [SerializeField] private Texture2D _mapIcon;

        [Header("Damage")]
        [Tooltip("A blow closing slower than this does no harm (m/s).")]
        [SerializeField] private float _harmlessSpeed = 1.5f;

        [Tooltip("The closing speed of a blow against a shore that sinks the hull at once (m/s); a blow takes the square of its speed over the square of this.")]
        [SerializeField] private float _sinkingSpeed = 14f;

        [Tooltip("The share of the top speed left when the hull is ruined.")]
        [SerializeField] private float _damagedSpeedFloor = 0.6f;

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

        [Header("Masts")]
        [Tooltip("The chance a hit of Mast Breaking Hit or more brings down the mast nearest it, with the hull whole after the hit.")]
        [SerializeField] private float _mastBreakChanceWhole = 0.05f;

        [Tooltip("The same chance with the hull ruined; between them it follows the health.")]
        [SerializeField] private float _mastBreakChanceRuined = 0.5f;

        [Tooltip("A hit taking this much health or more rolls the full chance; a smaller one a share of it.")]
        [SerializeField] private float _mastBreakingHit = 0.15f;

        [Tooltip("How long a broken mast takes to fall over the side (s).")]
        [SerializeField] private float _mastFallSeconds = 2.5f;

        [Tooltip("How far a broken mast tips as it falls (degrees).")]
        [SerializeField] private float _mastFallDeg = 80f;

        [Tooltip("How fast a fallen mast sinks (m/s).")]
        [SerializeField] private float _mastSinkSpeed = 1.5f;

        [Tooltip("How long a fallen mast sinks before it is gone (s).")]
        [SerializeField] private float _mastSinkSeconds = 8f;

        [Header("Guns")]
        [Tooltip("The starboard guns' muzzles in the hull's space (m), stern to bow; the port guns mirror them. The builder's report prints them.")]
        [SerializeField] private Vector3[] _guns = System.Array.Empty<Vector3>();

        [Tooltip("The health one of this ship's balls takes off a hull of strength 1.")]
        [SerializeField] private float _ballDamage = 0.06f;

        [Tooltip("What a ball's damage is divided by on this hull.")]
        [SerializeField] private float _hullStrength = 1f;

        /// <summary>From the centre to the bow and to the stern at the waterline, in metres.</summary>
        public float HalfLength => _halfLength;

        /// <summary>From the centre to either side at the waterline, in metres.</summary>
        public float HalfBeam => _halfBeam;

        /// <summary>Keel below the waterline, in metres.</summary>
        public float Draft => _draft;

        /// <summary>Rail above the waterline amidships, in metres.</summary>
        public float Freeboard => _freeboard;

        /// <summary>
        /// From the centreline to the hull's side above the water, <paramref name="z"/> metres
        /// toward the bow from the centre (negative: toward the stern), in metres.
        /// </summary>
        public float HalfBeamAt(float z)
        {
            float along = _halfLength > 0f ? Mathf.Clamp(z / _halfLength, -1f, 1f) : 0f;
            return _halfBeam * _beamAlongLength.Evaluate(along);
        }

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

        /// <summary>The ship seen from above, bow up, for the maps.</summary>
        public Texture2D MapIcon => _mapIcon;

        /// <summary>The rig of a camera on this ship.</summary>
        public CameraRig Rig(CameraView view)
        {
            switch (view)
            {
                case CameraView.BirdsEye:
                    return _birdsEye;
                case CameraView.Helm:
                    return _helm;
                default:
                    return new CameraRig(_cameraOffset, _cameraLookAt);
            }
        }

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

        /// <summary>The chance a hit of <see cref="MastBreakingHit"/> or more brings down a mast, the hull whole after it.</summary>
        public float MastBreakChanceWhole => _mastBreakChanceWhole;

        /// <summary>The chance a hit of <see cref="MastBreakingHit"/> or more brings down a mast, the hull ruined.</summary>
        public float MastBreakChanceRuined => _mastBreakChanceRuined;

        /// <summary>The health a hit takes to roll the full chance of breaking a mast.</summary>
        public float MastBreakingHit => _mastBreakingHit;

        /// <summary>How long a broken mast takes to fall over the side, in seconds.</summary>
        public float MastFallSeconds => _mastFallSeconds;

        /// <summary>How far a broken mast tips as it falls, in degrees.</summary>
        public float MastFallDeg => _mastFallDeg;

        /// <summary>How fast a fallen mast sinks, in m/s.</summary>
        public float MastSinkSpeed => _mastSinkSpeed;

        /// <summary>How long a fallen mast sinks before it is gone, in seconds.</summary>
        public float MastSinkSeconds => _mastSinkSeconds;

        /// <summary>How many guns each side carries.</summary>
        public int GunsPerSide => _guns.Length;

        /// <summary>The muzzle of gun <paramref name="index"/> (stern to bow) on one side, in the hull's space, in metres.</summary>
        public Vector3 Gun(int index, bool starboard)
        {
            Vector3 muzzle = _guns[index];
            return starboard ? muzzle : new Vector3(-muzzle.x, muzzle.y, muzzle.z);
        }

        /// <summary>The health one of this ship's balls takes off a hull of strength 1.</summary>
        public float BallDamage => _ballDamage;

        /// <summary>What a ball's damage is divided by on this hull.</summary>
        public float HullStrength => _hullStrength;

        /// <summary>Sets the guns and the balls' damage (types made in code: tests).</summary>
        internal void SetGuns(Vector3[] starboardMuzzles, float ballDamage, float hullStrength)
        {
            _guns = starboardMuzzles;
            _ballDamage = ballDamage;
            _hullStrength = hullStrength;
        }

        /// <summary>Sets the chances of breaking a mast (types made in code: tests).</summary>
        internal void SetMastBreaking(float whole, float ruined, float breakingHit)
        {
            _mastBreakChanceWhole = whole;
            _mastBreakChanceRuined = ruined;
            _mastBreakingHit = breakingHit;
        }

        /// <summary>Sets the hull's side along its length (types made in code: tests).</summary>
        internal void SetBeamAlongLength(AnimationCurve shares)
        {
            _beamAlongLength = shares;
        }

        /// <summary>Sets the cameras' rigs (types made in code: tests).</summary>
        internal void SetRigs(CameraRig chase, CameraRig birdsEye, CameraRig helm)
        {
            _cameraOffset = chase.Offset;
            _cameraLookAt = chase.LookAt;
            _birdsEye = birdsEye;
            _helm = helm;
        }

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
