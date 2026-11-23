namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Sails its ship round a circle on its own, through the same <see cref="ShipController.SetInput"/>
    /// the stick uses: each step it heads for a point some way ahead along the circle from where the
    /// ship is, steering by how far that point is off the bow, so a ram or a drift that pushes it off
    /// puts it back on. A sinking ship no longer answers its helm. See docs/systems/combat.md.
    /// </summary>
    public sealed class CircleHelm : MonoBehaviour
    {
        [SerializeField] private ShipController _ship;

        [Tooltip("The circle's centre, in the world (m); its height is ignored.")]
        [SerializeField] private Vector3 _centre;

        [Tooltip("The circle's radius (m).")]
        [SerializeField] private float _radius = 45f;

        [Tooltip("Round the circle clockwise seen from above, turning to starboard; otherwise to port.")]
        [SerializeField] private bool _toStarboard = true;

        [Tooltip("The throttle it holds, 0 to 1.")]
        [SerializeField] private float _throttle = 0.5f;

        [Tooltip("How far ahead along the circle it heads for (degrees of the circle).")]
        [SerializeField] private float _lookAheadDeg = 20f;

        [Tooltip("The bearing off the bow at which it puts the helm hard over (degrees).")]
        [SerializeField] private float _hardOverDeg = 30f;

        /// <summary>The circle's centre, in the world.</summary>
        public Vector3 Centre => _centre;

        /// <summary>The circle's radius, in metres.</summary>
        public float Radius => _radius;

        /// <summary>Sets the ship and its circle in code (tests).</summary>
        internal void Configure(ShipController ship, Vector3 centre, float radius, bool toStarboard, float throttle)
        {
            _ship = ship;
            _centre = centre;
            _radius = radius;
            _toStarboard = toStarboard;
            _throttle = throttle;
        }

        private void Update()
        {
            Tick();
        }

        /// <summary>Sets the helm for this step. Exposed internally for tests.</summary>
        internal void Tick()
        {
            if (_ship == null)
            {
                return;
            }
            Transform ship = _ship.transform;
            Vector3 from = ship.position - _centre;
            float around = Mathf.Atan2(from.z, from.x) * Mathf.Rad2Deg;
            float ahead = (around + (_toStarboard ? -_lookAheadDeg : _lookAheadDeg)) * Mathf.Deg2Rad;
            Vector3 aim = _centre + new Vector3(Mathf.Cos(ahead), 0f, Mathf.Sin(ahead)) * _radius;
            Vector3 toAim = aim - ship.position;
            toAim.y = 0f;
            Vector3 forward = ship.forward;
            forward.y = 0f;
            float off = Vector3.SignedAngle(forward, toAim, Vector3.up);
            _ship.SetInput(_throttle, Mathf.Clamp(off / _hardOverDeg, -1f, 1f));
        }
    }
}
