namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Gameplay.Water;

    /// <summary>
    /// Makes a kinematic ship ride the waves without physics. Its object (the ship's root) keeps
    /// moving on the plane with yaw only, as <see cref="ShipController"/> drives it; each frame
    /// this sets the hull child's height, pitch and roll from the water under the bow, stern,
    /// port and starboard, eased toward them so a short wave does not shake the hull.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class WaveRider : MonoBehaviour
    {
        [SerializeField] private WaterSurface _water;

        [Tooltip("The child that carries the hull's mesh and collider; it heaves, pitches and rolls.")]
        [SerializeField] private Transform _hull;

        [Tooltip("Distance from the centre to the bow and to the stern sample points (m).")]
        [SerializeField] private float _halfLength = 1f;

        [Tooltip("Distance from the centre to the port and starboard sample points (m).")]
        [SerializeField] private float _halfBeam = 0.5f;

        [Tooltip("Height of the hull's centre above the mean water surface (m); negative sits it lower.")]
        [SerializeField] private float _waterlineOffset = 0f;

        [Tooltip("How fast the hull follows the water (1/s): higher follows short waves more closely.")]
        [SerializeField] private float _response = 4f;

        private Transform _transform;
        private float _heave;
        private float _pitchDeg;
        private float _rollDeg;
        private bool _started;

        /// <summary>The hull's world height, pitch and roll (degrees) as eased so far, for tests.</summary>
        internal Vector3 HeavePitchRoll => new Vector3(_heave, _pitchDeg, _rollDeg);

        /// <summary>Wires the water and the hull (ships built in code: tests).</summary>
        internal void Configure(WaterSurface water, Transform hull, float halfLength, float halfBeam, float response)
        {
            _water = water;
            _hull = hull;
            _halfLength = halfLength;
            _halfBeam = halfBeam;
            _response = response;
            _started = false;
        }

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            Tick(0f); // on the water before the first frame
        }

        private void LateUpdate()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// One step of <paramref name="dt"/> seconds: samples the water and eases the hull toward it.
        /// The first step places it at once. Exposed internally for tests.
        /// </summary>
        internal void Tick(float dt)
        {
            if (_water == null || _hull == null)
            {
                return;
            }
            if (_transform == null)
            {
                _transform = transform;
            }

            Vector3 centre = _transform.position;
            Vector3 forward = _transform.forward * _halfLength;
            Vector3 right = _transform.right * _halfBeam;
            float bow = _water.HeightAt(centre + forward);
            float stern = _water.HeightAt(centre - forward);
            float starboard = _water.HeightAt(centre + right);
            float port = _water.HeightAt(centre - right);

            // Bow up is a negative rotation about the local x axis; starboard up a positive one about z.
            float targetHeave = (bow + stern + starboard + port) * 0.25f + _waterlineOffset;
            float targetPitch = Mathf.Atan2(stern - bow, 2f * _halfLength) * Mathf.Rad2Deg;
            float targetRoll = Mathf.Atan2(starboard - port, 2f * _halfBeam) * Mathf.Rad2Deg;

            float k = _started ? 1f - Mathf.Exp(-_response * dt) : 1f;
            _started = true;
            _heave += (targetHeave - _heave) * k;
            _pitchDeg += (targetPitch - _pitchDeg) * k;
            _rollDeg += (targetRoll - _rollDeg) * k;

            _hull.localPosition = new Vector3(0f, _heave - centre.y, 0f);
            _hull.localRotation = Quaternion.Euler(_pitchDeg, 0f, _rollDeg);
        }
    }
}
