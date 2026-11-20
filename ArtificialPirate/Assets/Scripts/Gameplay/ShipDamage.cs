namespace Game.Gameplay
{
    using System;
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// A ship's hull health, from 1 (whole) down to 0. Blows on its hull (<see cref="HullContact.Struck"/>:
    /// rams and groundings) and hits from anything else (<see cref="Hit"/>) lower it. A damaged ship
    /// is slower (<see cref="SpeedFactor"/>), sits lower (<see cref="Sink"/>) and lists toward the side
    /// that took the damage (<see cref="ListDeg"/>), by its type's numbers.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class ShipDamage : MonoBehaviour
    {
        [Tooltip("The ship's hull: its type, its model's transform, and the blows it takes.")]
        [SerializeField] private HullContact _contact;

        private float _health = 1f;
        private float _starboard;
        private float _port;

        /// <summary>The hull's health: 1 whole, 0 ruined.</summary>
        public float Health => _health;

        /// <summary>Raised on each hit that did harm: where (world) and how much health it took.</summary>
        public event Action<Vector3, float> Damaged;

        /// <summary>The share of the type's top speed the ship can still make.</summary>
        public float SpeedFactor
        {
            get
            {
                SO_ShipType type = Type;
                return type == null ? 1f : Mathf.Lerp(type.DamagedSpeedFloor, 1f, _health);
            }
        }

        /// <summary>How far the hull has settled below its waterline, in metres.</summary>
        public float Sink
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

        private SO_ShipType Type => _contact != null ? _contact.Type : null;

        /// <summary>Wires the hull (ships built in code: tests).</summary>
        internal void Configure(HullContact contact)
        {
            Unsubscribe();
            _contact = contact;
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

        /// <summary>
        /// Takes <paramref name="amount"/> of health (0 to 1) off the hull, struck at
        /// <paramref name="worldPoint"/>; the side it lands on sets the list.
        /// </summary>
        public void Hit(Vector3 worldPoint, float amount)
        {
            amount = Mathf.Min(amount, _health);
            if (amount <= 0f)
            {
                return;
            }
            _health -= amount;

            Transform hull = _contact != null ? _contact.Hull : null;
            bool starboard = hull == null || hull.InverseTransformPoint(worldPoint).x >= 0f;
            if (starboard)
            {
                _starboard += amount;
            }
            else
            {
                _port += amount;
            }

            Log.Info(LogCat.Gameplay, "ShipDamage: {0} hit for {1:0.000}, health {2:0.000}", name, amount, _health);
            Damaged?.Invoke(worldPoint, amount);
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
