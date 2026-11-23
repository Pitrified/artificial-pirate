namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Smoke from a badly damaged ship: below a health it sends a puff up from the rail above its
    /// heaviest kept hit every interval, more of them while it sinks, into the scene's
    /// <see cref="CombatEffects"/>. See docs/systems/combat.md.
    /// </summary>
    public sealed class DamageSmoke : MonoBehaviour
    {
        [SerializeField] private ShipDamage _damage;

        [Tooltip("The scene's effects, whose smoke this sends up.")]
        [SerializeField] private CombatEffects _effects;

        [Tooltip("The health under which the ship smokes.")]
        [SerializeField] private float _belowHealth = 0.25f;

        [Tooltip("Between puffs (s).")]
        [SerializeField] private float _interval = 0.3f;

        [Tooltip("Puffs each time while it sinks.")]
        [SerializeField] private int _sinkingPuffs = 3;

        private float _timer;

        /// <summary>Wires the ship in code (tests).</summary>
        internal void Configure(ShipDamage damage, CombatEffects effects)
        {
            _damage = damage;
            _effects = effects;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>One step of <paramref name="dt"/> seconds; true when it sent smoke up. Exposed internally for tests.</summary>
        internal bool Tick(float dt)
        {
            if (_damage == null || _effects == null || _damage.Health >= _belowHealth || _damage.HitCount == 0 || _damage.Hull == null)
            {
                return false;
            }
            _timer -= dt;
            if (_timer > 0f)
            {
                return false;
            }
            _timer = _interval;
            HullHit heaviest = _damage.GetHit(0);
            for (int i = 1; i < _damage.HitCount; i++)
            {
                HullHit hit = _damage.GetHit(i);
                if (hit.Amount > heaviest.Amount)
                {
                    heaviest = hit;
                }
            }
            Vector3 rail = new Vector3(heaviest.Point.x, _damage.Type != null ? _damage.Type.Freeboard : heaviest.Point.y, heaviest.Point.z);
            _effects.Smoke(_damage.Hull.TransformPoint(rail), _damage.Sinking ? _sinkingPuffs : 1);
            return true;
        }
    }
}
