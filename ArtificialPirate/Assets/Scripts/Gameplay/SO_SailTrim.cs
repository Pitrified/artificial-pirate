namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// The winds a ship's sailing states are set for, and when the sails change from one to the
    /// next. Shared by every <see cref="ShipSails"/>; the bands are the ship builder's
    /// <c>WIND_BANDS_DEG</c>, in the model's order. See docs/systems/ship-controller.md.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pirate/Sail Trim", fileName = "SO_SailTrim")]
    public sealed class SO_SailTrim : ScriptableObject
    {
        [Tooltip("Where the wind comes from for each sailing state, off the bow (degrees clockwise, positive on starboard), in the model's order.")]
        [SerializeField] private float[] _bandsDeg = { 60f, 90f, 135f, 180f, -135f, -90f, -60f };

        [Tooltip("How much nearer the wind must be to another band before the sails change to it (degrees).")]
        [SerializeField] private float _hysteresisDeg = 8f;

        /// <summary>How many sailing states each mast has.</summary>
        public int BandCount => _bandsDeg.Length;

        /// <summary>Where the wind comes from for sailing state <paramref name="band"/>, off the bow in degrees.</summary>
        public float BandDeg(int band) => _bandsDeg[band];

        /// <summary>How much nearer another band must be before the sails change to it, in degrees.</summary>
        public float HysteresisDeg => _hysteresisDeg;

        /// <summary>
        /// The band for a wind from <paramref name="offBowDeg"/> off the bow (positive on starboard):
        /// the nearest, unless <paramref name="current"/> (or -1 for none) is within the hysteresis of it.
        /// </summary>
        public int Band(float offBowDeg, int current)
        {
            int nearest = 0;
            float nearestOff = float.MaxValue;
            for (int i = 0; i < _bandsDeg.Length; i++)
            {
                float off = Mathf.Abs(Mathf.DeltaAngle(offBowDeg, _bandsDeg[i]));
                if (off < nearestOff)
                {
                    nearest = i;
                    nearestOff = off;
                }
            }
            if (current >= 0 && current < _bandsDeg.Length
                && Mathf.Abs(Mathf.DeltaAngle(offBowDeg, _bandsDeg[current])) - nearestOff <= _hysteresisDeg)
            {
                return current;
            }
            return nearest;
        }

        /// <summary>Sets the bands and the hysteresis (tests).</summary>
        internal void Configure(float[] bandsDeg, float hysteresisDeg)
        {
            _bandsDeg = bandsDeg;
            _hysteresisDeg = hysteresisDeg;
        }
    }
}
