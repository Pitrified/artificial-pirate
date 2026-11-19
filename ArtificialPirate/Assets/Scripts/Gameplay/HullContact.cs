namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Gameplay.Water;
    using Game.Gameplay.World;

    /// <summary>
    /// A ship's hull against other ships' hulls. A ship that moves calls <see cref="Collide"/> after
    /// each step: it is moved out of any hull it overlaps, along the water, and a ram shares its
    /// closing speed with the other ship by mass, as two hulls that stay together. The other ship
    /// takes its share as a drift, which carries its root and fades under the water's drag.
    /// Given the world's ground (<see cref="UseGround"/>), <see cref="Aground"/> tells a ship
    /// whose leading end would be over ground shallower than its draft.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class HullContact : MonoBehaviour
    {
        private const float BlockCoefficient = 0.45f; // a sailing hull's share of its length x beam x draft box
        private const int MaxContacts = 4;
        private const float RestSpeed = 1e-3f; // m/s: a slow, steady shove hands over less than 1 cm/s a frame
        private const float MinPush = 1e-4f; // metres

        [Tooltip("The hull's size, which sets the mass.")]
        [SerializeField] private SO_ShipType _type;

        [Tooltip("The hull's collider, on the Ship layer.")]
        [SerializeField] private BoxCollider _hull;

        [Tooltip("The layers that hold ships' hulls.")]
        [SerializeField] private LayerMask _ships;

        [Tooltip("How fast a drift fades (1/s): a drift of v glides about v / this metres.")]
        [SerializeField] private float _driftDamping = 0.5f;

        private readonly Collider[] _overlaps = new Collider[MaxContacts];
        private Transform _transform;
        private Vector3 _drift;
        private WorldGenerator _ground;

        /// <summary>The ship's displacement from its type's size, in kilograms.</summary>
        public float Mass
        {
            get
            {
                if (_type == null)
                {
                    return 0f;
                }
                float length = 2f * _type.HalfLength;
                float beam = 2f * _type.HalfBeam;
                return length * beam * _type.Draft * BlockCoefficient * Buoyancy.WaterDensity;
            }
        }

        /// <summary>The velocity a ram has given this ship, on the water plane (m/s).</summary>
        public Vector3 Drift => _drift;

        /// <summary>Wires the type, the hull and the layers (ships built in code: tests).</summary>
        internal void Configure(SO_ShipType type, BoxCollider hull, LayerMask ships, float driftDamping)
        {
            _type = type;
            _hull = hull;
            _ships = ships;
            _driftDamping = driftDamping;
        }

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>The world whose ground the ship can run onto; none, and it never does.</summary>
        public void UseGround(WorldGenerator ground)
        {
            _ground = ground;
        }

        /// <summary>
        /// Whether the end of the hull leading along <paramref name="velocity"/> is over ground
        /// shallower than the ship's draft: the bow going ahead, the stern going astern.
        /// </summary>
        public bool Aground(Vector3 velocity)
        {
            velocity.y = 0f;
            if (_ground == null || _type == null || velocity.sqrMagnitude < MinPush * MinPush)
            {
                return false;
            }
            if (_transform == null)
            {
                _transform = transform;
            }

            Vector3 lead = _transform.position + velocity.normalized * _type.HalfLength;
            return _ground.HeightAt(lead.x, lead.z) > -_type.Draft;
        }

        /// <summary>Adds <paramref name="velocity"/>, flattened to the water plane, to the drift.</summary>
        public void Push(Vector3 velocity)
        {
            velocity.y = 0f;
            _drift += velocity;
        }

        /// <summary>
        /// Moves this ship out of every hull it overlaps and shares a ram's closing speed with each,
        /// by mass. Returns <paramref name="velocity"/> (the ship's own, steered) less what it gave up.
        /// </summary>
        public Vector3 Collide(Vector3 velocity)
        {
            if (_hull == null || _type == null)
            {
                return velocity;
            }
            if (_transform == null)
            {
                _transform = transform;
            }

            Transform hull = _hull.transform;
            Vector3 halfExtents = Vector3.Scale(_hull.size, hull.lossyScale) * 0.5f;
            int count = Physics.OverlapBoxNonAlloc(
                hull.TransformPoint(_hull.center), halfExtents, _overlaps, hull.rotation, _ships, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider other = _overlaps[i];
                _overlaps[i] = null;
                if (other == _hull)
                {
                    continue;
                }

                Transform otherTransform = other.transform;
                if (!Physics.ComputePenetration(
                        _hull, hull.position, hull.rotation,
                        other, otherTransform.position, otherTransform.rotation,
                        out Vector3 direction, out float distance))
                {
                    continue;
                }

                Vector3 push = direction * distance;
                push.y = 0f;
                if (push.sqrMagnitude < MinPush * MinPush)
                {
                    continue;
                }
                _transform.position += push;
                Vector3 normal = push.normalized;

                HullContact otherShip = other.GetComponentInParent<HullContact>();
                Vector3 otherDrift = otherShip == null ? Vector3.zero : otherShip.Drift;
                float closing = Vector3.Dot(otherDrift - velocity, normal);
                if (closing <= 0f)
                {
                    continue;
                }

                // Against a hull that cannot move this ship loses all of it; against a ship, the
                // other's share of the two masses.
                float ownShare = otherShip == null ? 1f : otherShip.Mass / (Mass + otherShip.Mass);
                velocity += normal * (closing * ownShare);
                if (otherShip != null)
                {
                    otherShip.Push(-normal * (closing * (1f - ownShare)));
                }
            }
            return velocity;
        }

        /// <summary>Carries the root by the drift and fades it. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (_drift == Vector3.zero)
            {
                return;
            }
            if (_transform == null)
            {
                _transform = transform;
            }

            _transform.position += _drift * dt;
            _drift *= Mathf.Exp(-_driftDamping * dt);
            if (_drift.sqrMagnitude < RestSpeed * RestSpeed)
            {
                _drift = Vector3.zero;
            }
        }
    }
}
