namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Gameplay.Water;

    /// <summary>
    /// Makes a kinematic ship ride the waves without physics. Its object (the ship's root) keeps
    /// moving on the plane with yaw only, as <see cref="ShipController"/> drives it; each frame
    /// this sets the hull child's height, pitch and roll from the water under the bow, stern,
    /// port and starboard, eased toward them so a short wave does not shake the hull. A damaged
    /// hull (<see cref="ShipDamage"/>) settles lower and lists.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class WaveRider : MonoBehaviour
    {
        [SerializeField] private WaterSurface _water;

        [Tooltip("The child that carries the hull's mesh and collider; it heaves, pitches and rolls.")]
        [SerializeField] private Transform _hull;

        [Tooltip("The hull's size: the water is sampled at its bow, stern, port and starboard.")]
        [SerializeField] private SO_ShipType _type;

        [Tooltip("Height of the hull's centre above the mean water surface (m); negative sits it lower.")]
        [SerializeField] private float _waterlineOffset = 0f;

        [Tooltip("The hull's damage, which settles it lower and lists it; none, and it rides as built.")]
        [SerializeField] private ShipDamage _damage;

        [Tooltip("How fast the hull follows the water (1/s): higher follows short waves more closely.")]
        [SerializeField] private float _response = 4f;

        private Transform _transform;
        private float _heave;
        private float _pitchDeg;
        private float _rollDeg;
        private bool _started;

        /// <summary>The hull's world height, pitch and roll (degrees) as eased so far, for tests.</summary>
        internal Vector3 HeavePitchRoll => new Vector3(_heave, _pitchDeg, _rollDeg);

        /// <summary>Wires the water, the hull and its damage (ships built in code: tests).</summary>
        internal void Configure(WaterSurface water, Transform hull, SO_ShipType type, float response, ShipDamage damage = null)
        {
            _water = water;
            _hull = hull;
            _type = type;
            _response = response;
            _damage = damage;
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
            if (_water == null || _hull == null || _type == null)
            {
                return;
            }
            if (_transform == null)
            {
                _transform = transform;
            }

            Vector3 centre = _transform.position;
            float halfLength = _type.HalfLength;
            float halfBeam = _type.HalfBeam;
            Vector3 forward = _transform.forward * halfLength;
            Vector3 right = _transform.right * halfBeam;
            float bow = _water.HeightAt(centre + forward);
            float stern = _water.HeightAt(centre - forward);
            float starboard = _water.HeightAt(centre + right);
            float port = _water.HeightAt(centre - right);

            // Bow up is a negative rotation about the local x axis; starboard up a positive one about z.
            float targetHeave = (bow + stern + starboard + port) * 0.25f + _waterlineOffset;
            float targetPitch = Mathf.Atan2(stern - bow, 2f * halfLength) * Mathf.Rad2Deg;
            float targetRoll = Mathf.Atan2(starboard - port, 2f * halfBeam) * Mathf.Rad2Deg;
            if (_damage != null)
            {
                targetHeave -= _damage.Sink;
                targetRoll -= _damage.ListDeg;
            }

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
