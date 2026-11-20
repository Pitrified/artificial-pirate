namespace Game.Gameplay
{
    using System;
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// A ship's hull health, from 1 (whole) down to 0. Blows on its hull (<see cref="HullContact.Struck"/>:
    /// rams and groundings) and hits from anything else (<see cref="Hit"/>) lower it. A damaged ship
    /// is slower (<see cref="SpeedFactor"/>, less again for each mast its <see cref="ShipMasts"/> has lost), settles lower (<see cref="Settle"/>) and lists toward the
    /// side that took the damage (<see cref="ListDeg"/>), by its type's numbers. At 0 it sinks: it
    /// goes under over the type's time, tipping toward the end last hit, then raises
    /// <see cref="Sunk"/> and turns its ship off. It keeps the latest hits in the hull's local space,
    /// and saves to and restores from <see cref="DamageSaveData"/>. See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class ShipDamage : MonoBehaviour
    {
        /// <summary>How many of the latest hits are kept; an older one is dropped.</summary>
        public const int KeptHits = 16;

        [Tooltip("The ship's hull: its type, its model's transform, and the blows it takes.")]
        [SerializeField] private HullContact _contact;

        [Tooltip("The ship's masts, if it has any that can break.")]
        [SerializeField] private ShipMasts _masts;

        private float _health = 1f;
        private float _starboard;
        private float _port;
        private bool _bowLastHit;
        private bool _sinking;
        private float _sinkProgress;
        private readonly HullHit[] _hits = new HullHit[KeptHits];
        private int _hitCount;
        private int _nextHit;

        /// <summary>The hull's health: 1 whole, 0 ruined.</summary>
        public float Health => _health;

        /// <summary>Whether the hull is ruined and going under.</summary>
        public bool Sinking => _sinking;

        /// <summary>How many hits are kept, at most <see cref="KeptHits"/>.</summary>
        public int HitCount => _hitCount;

        /// <summary>Raised on each hit that did harm: where (world) and how much health it took.</summary>
        public event Action<Vector3, float> Damaged;

        /// <summary>Raised once, when a sinking ship has gone under; the ship is then turned off.</summary>
        public event Action Sunk;

        /// <summary>Raised when saved damage has been put back.</summary>
        public event Action Restored;

        /// <summary>
        /// The share of the type's top speed the ship can still make: down with the health, and an
        /// equal share of what is above the floor off for each mast lost, never below the floor.
        /// </summary>
        public float SpeedFactor
        {
            get
            {
                SO_ShipType type = Type;
                if (type == null)
                {
                    return 1f;
                }
                float floor = type.DamagedSpeedFloor;
                float sails = _masts != null ? _masts.LostShare : 0f;
                return Mathf.Max(floor, Mathf.Lerp(floor, 1f, _health) - (1f - floor) * sails);
            }
        }

        /// <summary>How far the hull has settled below its waterline from damage, in metres.</summary>
        public float Settle
        {
            get
            {
                SO_ShipType type = Type;
                return type == null ? 0f : (1f - _health) * type.DamagedSinkShare * type.Freeboard;
            }
        }

        /// <summary>The list toward starboard (negative: toward port), in degrees.</summary>
        public float ListDeg
        {
            get
            {
                SO_ShipType type = Type;
                return type == null ? 0f : (_starboard - _port) * type.DamagedListDeg;
            }
        }

        /// <summary>How far below the water a sinking hull has gone, in metres; 0 afloat.</summary>
        public float SinkDepth
        {
            get
            {
                SO_ShipType type = Type;
                return type == null ? 0f : _sinkProgress * _sinkProgress * type.SinkDepth;
            }
        }

        /// <summary>How far a sinking hull has tipped, in degrees: positive bow down, as the hull's pitch.</summary>
        public float SinkPitchDeg
        {
            get
            {
                SO_ShipType type = Type;
                if (type == null)
                {
                    return 0f;
                }
                float pitch = _sinkProgress * type.SinkPitchDeg;
                return _bowLastHit ? pitch : -pitch;
            }
        }

        /// <summary>The ship's type, from its hull; null when not wired.</summary>
        public SO_ShipType Type => _contact != null ? _contact.Type : null;

        /// <summary>The hull's transform, the space the hits are kept in; null when not wired.</summary>
        public Transform Hull => _contact != null ? _contact.Hull : null;

        /// <summary>The kept hit <paramref name="index"/>, oldest first.</summary>
        public HullHit GetHit(int index)
        {
            if (index < 0 || index >= _hitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "No such hit kept.");
            }
            return _hits[(_nextHit - _hitCount + index + KeptHits) % KeptHits];
        }

        /// <summary>Wires the hull, and the masts if any (ships built in code: tests).</summary>
        internal void Configure(HullContact contact, ShipMasts masts = null)
        {
            Unsubscribe();
            _contact = contact;
            _masts = masts;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// Takes <paramref name="amount"/> of health (0 to 1) off the hull, struck at
        /// <paramref name="worldPoint"/>; the side it lands on sets the list, and the end it lands
        /// on the end that goes down first.
        /// </summary>
        public void Hit(Vector3 worldPoint, float amount)
        {
            amount = Mathf.Min(amount, _health);
            if (amount <= 0f)
            {
                return;
            }
            _health -= amount;

            Transform hull = Hull;
            Vector3 local = hull != null ? hull.InverseTransformPoint(worldPoint) : Vector3.zero;
            if (local.x >= 0f)
            {
                _starboard += amount;
            }
            else
            {
                _port += amount;
            }
            _bowLastHit = local.z >= 0f;
            Keep(new HullHit(local, amount));

            Log.Info(LogCat.Gameplay, "ShipDamage: {0} hit for {1:0.000}, health {2:0.000}", name, amount, _health);
            Damaged?.Invoke(worldPoint, amount);
            if (_health <= 0f)
            {
                _sinking = true;
                Log.Info(LogCat.Gameplay, "ShipDamage: {0} is sinking", name);
            }
        }

        /// <summary>The hull's damage, to save.</summary>
        public DamageSaveData ToSaveData()
        {
            var hits = new HitSaveData[_hitCount];
            for (int i = 0; i < _hitCount; i++)
            {
                HullHit hit = GetHit(i);
                hits[i] = new HitSaveData { x = hit.Point.x, y = hit.Point.y, z = hit.Point.z, amount = hit.Amount };
            }
            return new DamageSaveData
            {
                lost = 1f - _health,
                starboard = _starboard,
                port = _port,
                hits = hits,
                brokenMasts = _masts != null ? _masts.BrokenIndices() : Array.Empty<int>(),
            };
        }

        /// <summary>
        /// Puts back saved damage; a ruined hull starts going under again, toward the end of its
        /// last hit.
        /// </summary>
        public void Restore(DamageSaveData data)
        {
            _starboard = Mathf.Clamp01(data.starboard);
            _port = Mathf.Clamp01(data.port);
            _health = 1f - Mathf.Clamp01(data.lost);
            _hitCount = 0;
            _nextHit = 0;
            if (data.hits != null)
            {
                foreach (HitSaveData hit in data.hits)
                {
                    Keep(new HullHit(new Vector3(hit.x, hit.y, hit.z), hit.amount));
                }
            }
            _bowLastHit = _hitCount > 0 && GetHit(_hitCount - 1).Point.z >= 0f;
            _sinking = _health <= 0f;
            _sinkProgress = 0f;
            if (_masts != null)
            {
                _masts.Restore(data.brokenMasts);
            }
            Log.Info(LogCat.Gameplay, "ShipDamage: {0} restored, health {1:0.000}, {2} hits", name, _health, _hitCount);
            Restored?.Invoke();
        }

        private void Keep(HullHit hit)
        {
            _hits[_nextHit] = hit;
            _nextHit = (_nextHit + 1) % KeptHits;
            _hitCount = Mathf.Min(_hitCount + 1, KeptHits);
        }

        /// <summary>Carries a sinking ship under by <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (!_sinking || _sinkProgress >= 1f)
            {
                return;
            }
            SO_ShipType type = Type;
            float seconds = type != null ? type.SinkSeconds : 0f;
            _sinkProgress = seconds > 0f ? Mathf.Min(1f, _sinkProgress + dt / seconds) : 1f;
            if (_sinkProgress >= 1f)
            {
                Log.Info(LogCat.Gameplay, "ShipDamage: {0} has sunk", name);
                Sunk?.Invoke();
                gameObject.SetActive(false);
            }
        }

        /// <summary>A blow closing at <paramref name="speed"/>, of which this hull takes <paramref name="share"/>.</summary>
        private void OnStruck(Vector3 point, float speed, float share)
        {
            SO_ShipType type = Type;
            if (type == null || speed <= type.HarmlessSpeed)
            {
                return;
            }
            float harmless = type.HarmlessSpeed;
            float sinking = type.SinkingSpeed;
            Hit(point, share * (speed * speed - harmless * harmless) / (sinking * sinking));
        }

        private void Subscribe()
        {
            if (_contact != null)
            {
                _contact.Struck -= OnStruck;
                _contact.Struck += OnStruck;
            }
        }

        private void Unsubscribe()
        {
            if (_contact != null)
            {
                _contact.Struck -= OnStruck;
            }
        }
    }
}
