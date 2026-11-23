namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// How every ship's guns work: how far and how wide they reach, how fast a ball flies and
    /// scatters, and the rhythm of a volley. Shared by every <see cref="Broadside"/>; what differs
    /// by ship (the guns' places, a ball's damage, the hull's strength) is in its
    /// <see cref="SO_ShipType"/>. See docs/systems/combat.md.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pirate/Gunnery", fileName = "SO_Gunnery")]
    public sealed class SO_Gunnery : ScriptableObject
    {
        [Tooltip("The farthest a target can be from the ship (m).")]
        [SerializeField] private float _range = 90f;

        [Tooltip("How far toward the bow or the stern from the beam the guns can bear (degrees).")]
        [SerializeField] private float _arcDeg = 20f;

        [Tooltip("A ball's speed out of the muzzle (m/s).")]
        [SerializeField] private float _muzzleSpeed = 60f;

        [Tooltip("The most a ball strays from its aim at full range, each way (degrees); nearer, less.")]
        [SerializeField] private float _spreadDeg = 2.5f;

        [Tooltip("How much of the target's half length the aim is spread over, from its centre.")]
        [SerializeField] private float _aimAlongLength = 0.6f;

        [Tooltip("Between one gun and the next in a volley (s).")]
        [SerializeField] private float _volleyGap = 0.12f;

        [Tooltip("From the end of a volley until the side can fire again (s).")]
        [SerializeField] private float _reloadSeconds = 5f;

        [Tooltip("How often a side looks for a target (s).")]
        [SerializeField] private float _lookSeconds = 0.25f;

        /// <summary>The farthest a target can be, in metres.</summary>
        public float Range => _range;

        /// <summary>How far from the beam the guns bear, in degrees.</summary>
        public float ArcDeg => _arcDeg;

        /// <summary>A ball's speed out of the muzzle, in m/s.</summary>
        public float MuzzleSpeed => _muzzleSpeed;

        /// <summary>The most a ball strays at full range, each way, in degrees.</summary>
        public float SpreadDeg => _spreadDeg;

        /// <summary>The share of the target's half length the aim is spread over.</summary>
        public float AimAlongLength => _aimAlongLength;

        /// <summary>Between one gun and the next in a volley, in seconds.</summary>
        public float VolleyGap => _volleyGap;

        /// <summary>From the end of a volley until the side can fire again, in seconds.</summary>
        public float ReloadSeconds => _reloadSeconds;

        /// <summary>How often a side looks for a target, in seconds.</summary>
        public float LookSeconds => _lookSeconds;

        /// <summary>Sets every number (settings made in code: tests).</summary>
        internal void Set(float range, float arcDeg, float muzzleSpeed, float spreadDeg, float volleyGap, float reloadSeconds, float lookSeconds)
        {
            _range = range;
            _arcDeg = arcDeg;
            _muzzleSpeed = muzzleSpeed;
            _spreadDeg = spreadDeg;
            _volleyGap = volleyGap;
            _reloadSeconds = reloadSeconds;
            _lookSeconds = lookSeconds;
        }
    }
}
