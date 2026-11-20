namespace Game.Gameplay
{
    using System;
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// A ship's masts, which hits can bring down. Each hit on the hull may break the standing mast
    /// nearest it: the chance rises from the type's whole-hull chance to its ruined-hull one as the
    /// health drops, and a small hit rolls only a share of it. A broken mast's objects go over the
    /// side in a <see cref="FallingMast"/> and its stump shows. <see cref="ShipDamage"/> takes an
    /// equal share of the top speed off for each mast lost, and saves which are broken.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class ShipMasts : MonoBehaviour
    {
        [Tooltip("The hull whose hits may break the masts.")]
        [SerializeField] private ShipDamage _damage;

        [Tooltip("The model's masts, in the builder's order.")]
        [SerializeField] private MastParts[] _masts = Array.Empty<MastParts>();

        private bool[] _broken = Array.Empty<bool>();
        private int _lost;
        private System.Random _random = new System.Random();

        /// <summary>How many masts the ship has.</summary>
        public int Count => _masts.Length;

        /// <summary>How many have been brought down.</summary>
        public int Lost => _lost;

        /// <summary>The share of the masts brought down, 0 to 1.</summary>
        public float LostShare => _masts.Length == 0 ? 0f : (float)_lost / _masts.Length;

        /// <summary>Raised when mast <c>index</c> breaks.</summary>
        public event Action<int> Broke;

        /// <summary>Whether mast <paramref name="index"/> has been brought down.</summary>
        public bool IsBroken(int index) => _broken[index];

        /// <summary>Wires the hull and the masts, rolling from <paramref name="seed"/> (masts built in code: tests).</summary>
        internal void Configure(ShipDamage damage, MastParts[] masts, int seed)
        {
            Unsubscribe();
            _damage = damage;
            _masts = masts;
            _random = new System.Random(seed);
            Awake();
            Subscribe();
        }

        private void Awake()
        {
            if (_broken.Length != _masts.Length)
            {
                _broken = new bool[_masts.Length];
            }
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
        /// The chance a hit taking <paramref name="amount"/> brings down a mast, the hull at
        /// <paramref name="health"/> after it.
        /// </summary>
        public float BreakChance(float health, float amount)
        {
            SO_ShipType type = _damage != null ? _damage.Type : null;
            if (type == null || type.MastBreakingHit <= 0f)
            {
                return 0f;
            }
            float chance = Mathf.Lerp(type.MastBreakChanceRuined, type.MastBreakChanceWhole, health);
            return chance * Mathf.Clamp01(amount / type.MastBreakingHit);
        }

        /// <summary>
        /// Brings mast <paramref name="index"/> down: its objects tip over the side away from
        /// <paramref name="side"/> (+1 a blow on starboard, -1 on port) and sink; its stump shows.
        /// </summary>
        public void Break(int index, float side)
        {
            if (_broken[index])
            {
                return;
            }
            MarkBroken(index);
            MastParts mast = _masts[index];
            SO_ShipType type = _damage != null ? _damage.Type : null;
            Transform hull = _damage != null ? _damage.Hull : null;
            if (type != null && hull != null)
            {
                var fallen = new GameObject($"{name} mast {index}");
                fallen.transform.SetPositionAndRotation(hull.TransformPoint(new Vector3(0f, mast.FootY, mast.AlongZ)), hull.rotation);
                foreach (GameObject part in mast.Parts)
                {
                    if (part != null && part.activeSelf)
                    {
                        part.transform.SetParent(fallen.transform, true);
                    }
                }
                float away = side >= 0f ? 1f : -1f;
                fallen.AddComponent<FallingMast>().Begin(hull.forward, away * type.MastFallDeg,
                    type.MastFallSeconds, type.MastSinkSpeed, type.MastSinkSeconds);
            }
            Log.Info(LogCat.Gameplay, "ShipMasts: {0} lost mast {1}", name, index);
            Broke?.Invoke(index);
        }

        /// <summary>The masts brought down, to save.</summary>
        public int[] BrokenIndices()
        {
            var indices = new int[_lost];
            int k = 0;
            for (int i = 0; i < _broken.Length; i++)
            {
                if (_broken[i])
                {
                    indices[k++] = i;
                }
            }
            return indices;
        }

        /// <summary>Puts back saved broken masts on a ship whose masts all stand: gone, their stumps showing.</summary>
        public void Restore(int[] broken)
        {
            if (broken == null)
            {
                return;
            }
            foreach (int index in broken)
            {
                if (index < 0 || index >= _masts.Length || _broken[index])
                {
                    continue;
                }
                MarkBroken(index);
                foreach (GameObject part in _masts[index].Parts)
                {
                    if (part != null)
                    {
                        part.SetActive(false);
                    }
                }
            }
        }

        private void MarkBroken(int index)
        {
            _broken[index] = true;
            _lost++;
            if (_masts[index].Stump != null)
            {
                _masts[index].Stump.SetActive(true);
            }
        }

        private void OnDamaged(Vector3 point, float amount)
        {
            if (_lost == _masts.Length || _damage.HitCount == 0)
            {
                return;
            }
            HullHit hit = _damage.GetHit(_damage.HitCount - 1);
            int nearest = -1;
            float distance = float.MaxValue;
            for (int i = 0; i < _masts.Length; i++)
            {
                float d = Mathf.Abs(_masts[i].AlongZ - hit.Point.z);
                if (!_broken[i] && d < distance)
                {
                    nearest = i;
                    distance = d;
                }
            }
            if (_random.NextDouble() < BreakChance(_damage.Health, amount))
            {
                Break(nearest, hit.Point.x);
            }
        }

        private void Subscribe()
        {
            if (_damage != null)
            {
                Unsubscribe();
                _damage.Damaged += OnDamaged;
            }
        }

        private void Unsubscribe()
        {
            if (_damage != null)
            {
                _damage.Damaged -= OnDamaged;
            }
        }
    }
}
