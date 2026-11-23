namespace Game.Gameplay
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A ship's guns, fired on their own: each side (starboard, port) looks for the nearest ship of
    /// another <see cref="ShipSide"/> within the gunnery's range and arc of its beam, and when loaded
    /// fires a volley at it, one gun after another, then reloads. Each ball is aimed at a point along
    /// the target's hull, led by the target's velocity, on the low arc at the muzzle speed, kept
    /// within the arc and scattered more the farther it flies; <see cref="Cannonballs"/> flies it.
    /// The guns are the type's (<see cref="SO_ShipType.Gun"/>). A ship whose barrels are set turns
    /// each toward its side's target within the arc and raises it to the shot, fires from its mouth,
    /// and runs it in on firing and out again over the reload; with no target they ease back.
    /// A sinking ship does not fire. See docs/systems/combat.md.
    /// </summary>
    public sealed class Broadside : MonoBehaviour
    {
        private const int MaxFound = 8;
        private const int LeadIterations = 2;

        [Tooltip("The side this ship fights on.")]
        [SerializeField] private ShipSide _side = ShipSide.Pirates;

        [SerializeField] private SO_Gunnery _gunnery;
        [SerializeField] private HullContact _contact;
        [SerializeField] private ShipDamage _damage;

        [Tooltip("The scene's balls, which fly what this ship fires.")]
        [SerializeField] private Cannonballs _balls;

        [Tooltip("Seeds the scatter.")]
        [SerializeField] private int _seed = 1;

        [Tooltip("The barrels, starboard stern to bow then port stern to bow, each turning about its own origin; none, and the guns do not move.")]
        [SerializeField] private Transform[] _barrels = Array.Empty<Transform>();

        [Tooltip("How fast a barrel turns toward its aim (degrees per second).")]
        [SerializeField] private float _barrelTurnDeg = 30f;

        [Tooltip("How far a barrel runs in when it fires (m).")]
        [SerializeField] private float _recoil = 0.5f;

        [Tooltip("The share of the reload a barrel takes to run out again.")]
        [SerializeField] private float _runOutShare = 0.6f;

        private readonly Collider[] _found = new Collider[MaxFound];
        private readonly Battery _starboard = new Battery(true);
        private readonly Battery _port = new Battery(false);
        private System.Random _random;
        private Transform _transform;
        private Vector3 _lastPosition;
        private bool _hasLast;
        private Vector3 _velocity;
        private float _lookTimer;
        private Vector3[] _mouths;
        private Vector3[] _rests;
        private Vector3[] _restPositions;
        private float[] _runIn;

        /// <summary>The side this ship fights on.</summary>
        public ShipSide Side => _side;

        /// <summary>The ship's velocity over the last step, in m/s, from its root's movement.</summary>
        public Vector3 Velocity => _velocity;

        /// <summary>The ship's type.</summary>
        public SO_ShipType Type => _contact != null ? _contact.Type : null;

        /// <summary>The ship's hull, whose space the guns are in.</summary>
        public Transform Hull => _contact != null ? _contact.Hull : null;

        /// <summary>Whether the ship can fight: active and not sinking.</summary>
        public bool Afloat => isActiveAndEnabled && (_damage == null || !_damage.Sinking);

        /// <summary>The ship a side is firing at, or null.</summary>
        public Broadside TargetOf(bool starboard) => (starboard ? _starboard : _port).Target;

        /// <summary>Raised once per volley as its first gun fires: the side.</summary>
        public event Action<bool> VolleyFired;

        private sealed class Battery
        {
            public readonly bool Starboard;
            public Broadside Target;
            public bool Firing;
            public int NextGun;
            public float Timer;

            public Battery(bool starboard)
            {
                Starboard = starboard;
            }
        }

        private void Awake()
        {
            _transform = transform;
            _random = new System.Random(_seed);
            KeepBarrelsAtRest();
        }

        /// <summary>The barrels' places at rest, each mouth in its barrel's space; while they are unturned.</summary>
        private void KeepBarrelsAtRest()
        {
            int count = _barrels.Length;
            _mouths = new Vector3[count];
            _rests = new Vector3[count];
            _restPositions = new Vector3[count];
            _runIn = new float[count];
            SO_ShipType type = Type;
            if (type == null || Hull == null || count != 2 * type.GunsPerSide)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                bool starboard = i < type.GunsPerSide;
                Transform barrel = _barrels[i];
                _mouths[i] = barrel.InverseTransformPoint(Hull.TransformPoint(type.Gun(i % type.GunsPerSide, starboard)));
                _rests[i] = barrel.localRotation * Vector3.right * (starboard ? 1f : -1f);
                _restPositions[i] = barrel.localPosition;
            }
        }

        private bool HasBarrels => _barrels.Length > 0 && _barrels.Length == _mouths.Length && Type != null && _barrels.Length == 2 * Type.GunsPerSide;

        /// <summary>Wires the ship in code (tests).</summary>
        internal void Configure(ShipSide side, SO_Gunnery gunnery, HullContact contact, ShipDamage damage, Cannonballs balls, int seed,
            Transform[] barrels = null)
        {
            _side = side;
            _gunnery = gunnery;
            _contact = contact;
            _damage = damage;
            _balls = balls;
            _transform = transform;
            _random = new System.Random(seed);
            _barrels = barrels ?? Array.Empty<Transform>();
            KeepBarrelsAtRest();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>One step of <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (dt <= 0f || _gunnery == null || _contact == null || Type == null)
            {
                return;
            }
            Vector3 position = _transform.position;
            _velocity = _hasLast ? (position - _lastPosition) / dt : Vector3.zero;
            _lastPosition = position;
            _hasLast = true;

            if (!Afloat)
            {
                _starboard.Target = null;
                _port.Target = null;
                _starboard.Firing = false;
                _port.Firing = false;
                return;
            }
            _lookTimer -= dt;
            if (_lookTimer <= 0f)
            {
                _lookTimer = _gunnery.LookSeconds;
                Look();
            }
            Step(_starboard, dt);
            Step(_port, dt);
            if (HasBarrels)
            {
                Aim(_starboard, dt);
                Aim(_port, dt);
            }
        }

        /// <summary>Turns a side's barrels toward its target's centre on the solved arc, or back to rest, and runs them out.</summary>
        private void Aim(Battery battery, float dt)
        {
            int guns = Type.GunsPerSide;
            int first = battery.Starboard ? 0 : guns;
            float runOut = _recoil / Mathf.Max(_gunnery.ReloadSeconds * _runOutShare, Mathf.Epsilon);
            for (int i = first; i < first + guns; i++)
            {
                Transform barrel = _barrels[i];
                Vector3 aim = _rests[i];
                if (battery.Target != null)
                {
                    Vector3 mouth = barrel.TransformPoint(_mouths[i]);
                    Vector3 centre = battery.Target.Hull.TransformPoint(new Vector3(0f, battery.Target.Type.Freeboard * 0.5f, 0f));
                    if (Solve(mouth, centre, battery.Target._velocity, _gunnery.MuzzleSpeed, out Vector3 velocity))
                    {
                        aim = barrel.parent.InverseTransformDirection(WithinArc(velocity, battery.Starboard));
                    }
                }
                Quaternion wanted = Quaternion.FromToRotation(_rests[i], aim);
                barrel.localRotation = Quaternion.RotateTowards(barrel.localRotation, wanted, _barrelTurnDeg * dt);
                _runIn[i] = Mathf.MoveTowards(_runIn[i], 0f, runOut * dt);
                barrel.localPosition = _restPositions[i] - barrel.localRotation * _rests[i] * _runIn[i];
            }
        }

        private void Look()
        {
            _starboard.Target = null;
            _port.Target = null;
            float bestStarboard = float.MaxValue;
            float bestPort = float.MaxValue;
            int count = Physics.OverlapSphereNonAlloc(_transform.position, _gunnery.Range, _found, _contact.Ships, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Broadside other = _found[i].GetComponentInParent<Broadside>();
                if (other == null || other == this || other._side == _side || !other.Afloat)
                {
                    continue;
                }
                float distance = Flat(other._transform.position - _transform.position).magnitude;
                if (InArc(other, true) && distance < bestStarboard)
                {
                    bestStarboard = distance;
                    _starboard.Target = other;
                }
                else if (InArc(other, false) && distance < bestPort)
                {
                    bestPort = distance;
                    _port.Target = other;
                }
            }
        }

        /// <summary>Whether <paramref name="other"/>'s centre is within range and within the arc of one side's beam.</summary>
        internal bool InArc(Broadside other, bool starboard)
        {
            Vector3 to = Flat(other.transform.position - _transform.position);
            if (to.magnitude > _gunnery.Range)
            {
                return false;
            }
            return Vector3.Angle(Beam(starboard), to) <= _gunnery.ArcDeg;
        }

        private void Step(Battery battery, float dt)
        {
            battery.Timer -= dt;
            if (!battery.Firing)
            {
                if (battery.Timer <= 0f && battery.Target != null)
                {
                    battery.Firing = true;
                    battery.NextGun = 0;
                    battery.Timer = 0f;
                    VolleyFired?.Invoke(battery.Starboard);
                }
                else
                {
                    return;
                }
            }
            while (battery.Firing && battery.Timer <= 0f)
            {
                if (battery.Target == null || !battery.Target.Afloat || !InArc(battery.Target, battery.Starboard))
                {
                    EndVolley(battery);
                    return;
                }
                Fire(battery.NextGun, battery.Starboard, battery.Target);
                battery.NextGun++;
                battery.Timer += _gunnery.VolleyGap;
                if (battery.NextGun >= Type.GunsPerSide)
                {
                    EndVolley(battery);
                }
            }
        }

        private void EndVolley(Battery battery)
        {
            battery.Firing = false;
            battery.Timer = _gunnery.ReloadSeconds;
        }

        private void Fire(int gun, bool starboard, Broadside target)
        {
            Transform hull = Hull;
            int barrel = starboard ? gun : Type.GunsPerSide + gun;
            Vector3 muzzle = HasBarrels ? _barrels[barrel].TransformPoint(_mouths[barrel]) : hull.TransformPoint(Type.Gun(gun, starboard));
            if (HasBarrels)
            {
                _runIn[barrel] = _recoil;
            }
            Vector3 aim = AimPoint(target);
            if (!Solve(muzzle, aim, target._velocity, _gunnery.MuzzleSpeed, out Vector3 velocity))
            {
                return;
            }
            velocity = WithinArc(velocity, starboard);
            float stray = _gunnery.SpreadDeg * Mathf.Clamp01(Flat(aim - muzzle).magnitude / _gunnery.Range);
            Quaternion scatter = Quaternion.AngleAxis(Random(stray), Vector3.up) * Quaternion.AngleAxis(Random(stray), hull.right);
            if (_balls != null)
            {
                _balls.Launch(muzzle, scatter * velocity, this);
            }
        }

        /// <summary>A point along the target's hull at half its freeboard, picked at random along its length.</summary>
        private Vector3 AimPoint(Broadside target)
        {
            SO_ShipType type = target.Type;
            float along = Random(_gunnery.AimAlongLength) * type.HalfLength;
            return target.Hull.TransformPoint(new Vector3(0f, type.Freeboard * 0.5f, along));
        }

        /// <summary>
        /// The velocity that carries a ball fired at <paramref name="speed"/> from
        /// <paramref name="from"/> onto <paramref name="to"/>, moving at <paramref name="targetVelocity"/>,
        /// on the low arc under gravity; false when out of reach.
        /// </summary>
        internal static bool Solve(Vector3 from, Vector3 to, Vector3 targetVelocity, float speed, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            Vector3 lead = to;
            for (int i = 0; i <= LeadIterations; i++)
            {
                if (!SolveStill(from, lead, speed, out velocity, out float time))
                {
                    return false;
                }
                lead = to + targetVelocity * time;
            }
            return true;
        }

        private static bool SolveStill(Vector3 from, Vector3 to, float speed, out Vector3 velocity, out float time)
        {
            velocity = Vector3.zero;
            time = 0f;
            Vector3 flat = Flat(to - from);
            float d = flat.magnitude;
            float h = to.y - from.y;
            float g = -Physics.gravity.y;
            float v2 = speed * speed;
            float root = v2 * v2 - g * (g * d * d + 2f * h * v2);
            if (d < Mathf.Epsilon || root < 0f)
            {
                return false;
            }
            float angle = Mathf.Atan((v2 - Mathf.Sqrt(root)) / (g * d));
            float cos = Mathf.Cos(angle);
            time = d / (speed * cos);
            velocity = flat / d * (speed * cos) + Vector3.up * (speed * Mathf.Sin(angle));
            return true;
        }

        private Vector3 WithinArc(Vector3 velocity, bool starboard)
        {
            Vector3 flat = Flat(velocity);
            Vector3 beam = Beam(starboard);
            float yaw = Vector3.SignedAngle(beam, flat, Vector3.up);
            float clamped = Mathf.Clamp(yaw, -_gunnery.ArcDeg, _gunnery.ArcDeg);
            Vector3 heading = Quaternion.AngleAxis(clamped, Vector3.up) * beam * flat.magnitude;
            return heading + Vector3.up * velocity.y;
        }

        private Vector3 Beam(bool starboard)
        {
            Vector3 right = Flat(_transform.right).normalized;
            return starboard ? right : -right;
        }

        private float Random(float most) => most * (float)(_random.NextDouble() * 2.0 - 1.0);

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
