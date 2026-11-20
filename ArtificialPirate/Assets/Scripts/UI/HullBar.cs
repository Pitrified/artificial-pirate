namespace Game.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Gameplay;

    /// <summary>
    /// The HUD's hull bar: a fill whose right edge sits at the player's hull health, turning from the
    /// whole colour through the half one to the ruined one. It redraws when the hull is damaged, not
    /// each frame.
    /// See docs/systems/ui-system.md.
    /// </summary>
    public sealed class HullBar : MonoBehaviour
    {
        [Tooltip("The fill, stretched from the bar's left edge to the health.")]
        [SerializeField] private Image _fill;

        [SerializeField] private Color _whole = new Color(0.30f, 0.75f, 0.35f, 1f);
        [SerializeField] private Color _half = new Color(0.95f, 0.80f, 0.25f, 1f);
        [SerializeField] private Color _ruined = new Color(0.85f, 0.20f, 0.15f, 1f);

        private ShipDamage _damage;

        /// <summary>Wires the fill (bars built in code: tests).</summary>
        internal void Configure(Image fill)
        {
            _fill = fill;
        }

        /// <summary>Shows <paramref name="damage"/>'s health, and follows it as it changes.</summary>
        public void Show(ShipDamage damage)
        {
            Unbind();
            _damage = damage;
            if (_damage != null)
            {
                _damage.Damaged += OnDamaged;
            }
            Refresh();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void OnDamaged(Vector3 point, float amount) => Refresh();

        private void Refresh()
        {
            if (_fill == null)
            {
                return;
            }
            float health = _damage != null ? _damage.Health : 1f;
            RectTransform rect = _fill.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(health, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _fill.color = health >= 0.5f
                ? Color.Lerp(_half, _whole, 2f * health - 1f)
                : Color.Lerp(_ruined, _half, 2f * health);
        }

        private void Unbind()
        {
            if (_damage != null)
            {
                _damage.Damaged -= OnDamaged;
            }
        }
    }
}
