namespace Game.Gameplay
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A ship's guns, fired on their own: each <see cref="Battery"/> (the broadsides, and the bow
    /// chasers and stern guns of a type that carries them) looks for the nearest ship of another
    /// <see cref="ShipSide"/> within the gunnery's range and its arc (the beam's, or the chase arc
    /// ahead or astern), and when loaded fires a volley at it, one gun after another along each row of
    /// guns, the rows together, then reloads.
    /// Each ball is aimed at a point along the target's hull, led by the target's velocity, on the
    /// low arc at the muzzle speed, kept within the arc and scattered more the farther it flies;
    /// <see cref="Cannonballs"/> flies it. The guns are the type's (<see cref="SO_ShipType.Gun"/>). A
    /// ship whose barrels are set turns each toward its battery's target within the arc and raises it
    /// to the shot, fires from its mouth, and runs it in on firing and out again over the reload; with
    /// no target they ease back.
    /// A sinking ship does not fire. See docs/systems/combat.md.
    /// </summary>
    public sealed class Broadside : MonoBehaviour
    {
        private const int MaxFound = 8;
        private const int LeadIterations = 2;
        private const float StarboardDeg = 90f;
        private const float PortDeg = 270f;
        private const float AheadDeg = 0f;
        private const float AsternDeg = 180f;

        [Tooltip("The side this ship fights on.")]
        [SerializeField] private ShipSide _side = ShipSide.Pirates;

        [SerializeField] private SO_Gunnery _gunnery;
        [SerializeField] private HullContact _contact;
        [SerializeField] private ShipDamage _damage;

        [Tooltip("The scene's balls, which fly what this ship fires.")]
        [SerializeField] private Cannonballs _balls;

        [Tooltip("Seeds the scatter.")]
        [SerializeField] private int _seed = 1;

        [Tooltip("The barrels, starboard stern to bow, port stern to bow, then the bow chasers and the stern guns as the type lists them, each turning about its own origin; none, and the guns do not move.")]
        [SerializeField] private Transform[] _barrels = Array.Empty<Transform>();

        [Tooltip("How fast a barrel turns toward its aim (degrees per second).")]
        [SerializeField] private float _barrelTurnDeg = 30f;

        [Tooltip("How far a barrel runs in when it fires (m).")]
        [SerializeField] private float _recoil = 0.5f;

        [Tooltip("The share of the reload a barrel takes to run out again.")]
        [SerializeField] private float _runOutShare = 0.6f;

        private readonly Collider[] _found = new Collider[MaxFound];
        private readonly Guns[] _batteries =
        {
            new Guns(Battery.Starboard, StarboardDeg),
            new Guns(Battery.Port, PortDeg),
            new Guns(Battery.Bow, AheadDeg),
            new Guns(Battery.Stern, AsternDeg),
        };
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

        /// <summary>The ship a battery is firing at, or null.</summary>
        public Broadside TargetOf(Battery battery) => _batteries[(int)battery].Target;

        /// <summary>Raised once per volley as its first gun fires: the battery.</summary>
        public event Action<Battery> VolleyFired;

        /// <summary>A battery's bearing off the bow, its first barrel and count, its rows, and its volley.</summary>
        private sealed class Guns
        {
            public readonly Battery Battery;
            public readonly float BearingDeg;
            public int First;
            public int Count;
            public int[] RowStarts = Array.Empty<int>();
            public int Steps;
            public Broadside Target;
            public float Best;
            public bool Firing;
            public int NextStep;
            public float Timer;

            public Guns(Battery battery, float bearingDeg)
            {
                Battery = battery;
                BearingDeg = bearingDeg;
            }
        }

        private void Awake()
        {
            _transform = transform;
            _random = new System.Random(_seed);
            KeepBarrelsAtRest();
        }

        /// <summary>
        /// Each battery's guns from the type, and the barrels' places at rest, each mouth in its
        /// barrel's space and its axis from the trunnions to the mouth; while they are unturned.
        /// </summary>
        private void KeepBarrelsAtRest()
        {
            SO_ShipType type = Type;
            int guns = 0;
            foreach (Guns battery in _batteries)
            {
                battery.First = guns;
                battery.Count = type != null ? type.GunCount(battery.Battery) : 0;
                KeepRows(type, battery);
                guns += battery.Count;
            }
            int count = _barrels.Length;
            _mouths = new Vector3[count];
            _rests = new Vector3[count];
            _restPositions = new Vector3[count];
            _runIn = new float[count];
            if (type == null || Hull == null || count != guns)
            {
                return;
            }
            foreach (Guns battery in _batteries)
            {
                for (int k = 0; k < battery.Count; k++)
                {
                    int i = battery.First + k;
                    Transform barrel = _barrels[i];
                    _mouths[i] = barrel.InverseTransformPoint(Hull.TransformPoint(type.Gun(battery.Battery, k)));
                    _rests[i] = (barrel.localRotation * _mouths[i]).normalized;
                    _restPositions[i] = barrel.localPosition;
                }
            }
        }

        /// <summary>
        /// Where each row of a battery's guns starts, the guns listed row after row, each stern to bow:
        /// a new row wherever the next gun is farther aft. A volley fires a gun of every row at each step.
        /// </summary>
        private static void KeepRows(SO_ShipType type, Guns battery)
        {
            int rows = battery.Count > 0 ? 1 : 0;
            for (int k = 1; k < battery.Count; k++)
            {
                if (NewRow(type, battery.Battery, k))
                {
                    rows++;
                }
            }
            battery.RowStarts = new int[rows];
            battery.Steps = 0;
            int row = 0;
            for (int k = 0; k < battery.Count; k++)
            {
                if (k > 0 && NewRow(type, battery.Battery, k))
                {
                    battery.Steps = Mathf.Max(battery.Steps, k - battery.RowStarts[row]);
                    row++;
                    battery.RowStarts[row] = k;
                }
            }
            if (rows > 0)
            {
                battery.Steps = Mathf.Max(battery.Steps, battery.Count - battery.RowStarts[row]);
            }
        }

        private static bool NewRow(SO_ShipType type, Battery battery, int gun) => type.Gun(battery, gun).z < type.Gun(battery, gun - 1).z;

        private bool HasBarrels => _barrels.Length > 0 && _barrels.Length == _mouths.Length;

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
                foreach (Guns battery in _batteries)
                {
                    battery.Target = null;
                    battery.Firing = false;
                }
                return;
            }
            _lookTimer -= dt;
            if (_lookTimer <= 0f)
            {
                _lookTimer = _gunnery.LookSeconds;
                Look();
            }
            foreach (Guns battery in _batteries)
            {
                Step(battery, dt);
                if (HasBarrels)
                {
                    Aim(battery, dt);
                }
            }
        }

        /// <summary>Turns a battery's barrels toward its target's centre on the solved arc, or back to rest, and runs them out.</summary>
        private void Aim(Guns battery, float dt)
        {
            float runOut = _recoil / Mathf.Max(_gunnery.ReloadSeconds * _runOutShare, Mathf.Epsilon);
            for (int i = battery.First; i < battery.First + battery.Count; i++)
            {
                Transform barrel = _barrels[i];
                Vector3 aim = _rests[i];
                if (battery.Target != null)
                {
                    Vector3 mouth = barrel.TransformPoint(_mouths[i]);
                    Vector3 centre = battery.Target.Hull.TransformPoint(new Vector3(0f, battery.Target.Type.Freeboard * 0.5f, 0f));
                    if (Solve(mouth, centre, battery.Target._velocity, _gunnery.MuzzleSpeed, out Vector3 velocity))
                    {
                        aim = barrel.parent.InverseTransformDirection(WithinArc(velocity, battery));
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
            foreach (Guns battery in _batteries)
            {
                battery.Target = null;
                battery.Best = float.MaxValue;
            }
            int count = Physics.OverlapSphereNonAlloc(_transform.position, _gunnery.Range, _found, _contact.Ships, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Broadside other = _found[i].GetComponentInParent<Broadside>();
                if (other == null || other == this || other._side == _side || !other.Afloat)
                {
                    continue;
                }
                float distance = Flat(other._transform.position - _transform.position).magnitude;
                foreach (Guns battery in _batteries)
                {
                    if (battery.Count > 0 && distance < battery.Best && InArc(other, battery))
                    {
                        battery.Best = distance;
                        battery.Target = other;
                    }
                }
            }
        }

        /// <summary>Whether <paramref name="other"/>'s centre is within range and within the arc of <paramref name="battery"/>.</summary>
        internal bool InArc(Broadside other, Battery battery) => InArc(other, _batteries[(int)battery]);

        private bool InArc(Broadside other, Guns battery)
        {
            Vector3 to = Flat(other.transform.position - _transform.position);
            if (to.magnitude > _gunnery.Range)
            {
                return false;
            }
            return Vector3.Angle(Bearing(battery), to) <= Arc(battery);
        }

        private void Step(Guns battery, float dt)
        {
            battery.Timer -= dt;
            if (!battery.Firing)
            {
                if (battery.Timer <= 0f && battery.Target != null)
                {
                    battery.Firing = true;
                    battery.NextStep = 0;
                    battery.Timer = 0f;
                    VolleyFired?.Invoke(battery.Battery);
                }
                else
                {
                    return;
                }
            }
            while (battery.Firing && battery.Timer <= 0f)
            {
                if (battery.Target == null || !battery.Target.Afloat || !InArc(battery.Target, battery))
                {
                    EndVolley(battery);
                    return;
                }
                FireStep(battery, battery.NextStep);
                battery.NextStep++;
                battery.Timer += _gunnery.VolleyGap;
                if (battery.NextStep >= battery.Steps)
                {
                    EndVolley(battery);
                }
            }
        }

        private void EndVolley(Guns battery)
        {
            battery.Firing = false;
            battery.Timer = _gunnery.ReloadSeconds;
        }

        /// <summary>Fires gun <paramref name="step"/> of each of the battery's rows that has one.</summary>
        private void FireStep(Guns battery, int step)
        {
            int[] starts = battery.RowStarts;
            for (int row = 0; row < starts.Length; row++)
            {
                int end = row + 1 < starts.Length ? starts[row + 1] : battery.Count;
                int gun = starts[row] + step;
                if (gun < end)
                {
                    Fire(battery, gun);
                }
            }
        }

        private void Fire(Guns battery, int gun)
        {
            Transform hull = Hull;
            Broadside target = battery.Target;
            int barrel = battery.First + gun;
            Vector3 muzzle = HasBarrels ? _barrels[barrel].TransformPoint(_mouths[barrel]) : hull.TransformPoint(Type.Gun(battery.Battery, gun));
            if (HasBarrels)
            {
                _runIn[barrel] = _recoil;
            }
            Vector3 aim = AimPoint(target);
            if (!Solve(muzzle, aim, target._velocity, _gunnery.MuzzleSpeed, out Vector3 velocity))
            {
                return;
            }
            velocity = WithinArc(velocity, battery);
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

        private Vector3 WithinArc(Vector3 velocity, Guns battery)
        {
            Vector3 flat = Flat(velocity);
            Vector3 bearing = Bearing(battery);
            float arc = Arc(battery);
            float yaw = Vector3.SignedAngle(bearing, flat, Vector3.up);
            float clamped = Mathf.Clamp(yaw, -arc, arc);
            Vector3 heading = Quaternion.AngleAxis(clamped, Vector3.up) * bearing * flat.magnitude;
            return heading + Vector3.up * velocity.y;
        }

        /// <summary>The way a battery bears, flat, from the ship's heading.</summary>
        private Vector3 Bearing(Guns battery)
        {
            Vector3 forward = Flat(_transform.forward).normalized;
            return Quaternion.AngleAxis(battery.BearingDeg, Vector3.up) * forward;
        }

        private float Arc(Guns battery) =>
            battery.Battery == Battery.Bow || battery.Battery == Battery.Stern ? _gunnery.ChaseArcDeg : _gunnery.ArcDeg;

        private float Random(float most) => most * (float)(_random.NextDouble() * 2.0 - 1.0);

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
