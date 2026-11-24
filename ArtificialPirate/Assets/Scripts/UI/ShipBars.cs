namespace Game.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Gameplay;

    /// <summary>
    /// Health bars over the other ships near the player's, on the HUD: each frame the ships within
    /// reach of the player's (by their hulls on the ships' layers) get a bar from a fixed pool, placed
    /// on the screen over the ship, its fill the ship's health, red for another side and green for
    /// the player's. A ship behind the camera, sinking, or beyond reach has none.
    /// See docs/systems/ui-system.md.
    /// </summary>
    public sealed class ShipBars : MonoBehaviour
    {
        private const int MaxFound = 16;

        [Tooltip("A bar to copy: a background whose child Image is the fill. Kept inactive.")]
        [SerializeField] private RectTransform _template;

        [Tooltip("How many ships can have a bar at once.")]
        [SerializeField] private int _poolSize = 4;

        [Tooltip("How far from the player's ship a ship gets a bar (m).")]
        [SerializeField] private float _reach = 180f;

        [Tooltip("How high over the ship's rail amidships (its type's freeboard) the bar sits (m).")]
        [SerializeField] private float _overRail = 6f;

        [SerializeField] private Color _enemy = new Color(0.85f, 0.2f, 0.15f, 1f);
        [SerializeField] private Color _ally = new Color(0.3f, 0.75f, 0.35f, 1f);

        private readonly Collider[] _found = new Collider[MaxFound];
        private RectTransform[] _bars;
        private Image[] _fills;
        private Broadside _player;
        private LayerMask _ships;
        private Camera _camera;
        private RectTransform _area;

        /// <summary>How many bars are showing.</summary>
        public int Showing { get; private set; }

        /// <summary>The fill of bar <paramref name="index"/>, for tests.</summary>
        internal Image FillOf(int index) => _fills[index];

        /// <summary>The bar <paramref name="index"/>, for tests.</summary>
        internal RectTransform BarOf(int index) => _bars[index];

        /// <summary>Wires the bars in code (tests).</summary>
        internal void Configure(RectTransform template, int poolSize, float reach)
        {
            _template = template;
            _poolSize = poolSize;
            _reach = reach;
            Build();
        }

        /// <summary>Shows bars over the ships near <paramref name="player"/>, placed through <paramref name="view"/>.</summary>
        public void Show(Broadside player, Camera view)
        {
            _player = player;
            _camera = view;
            _ships = player != null && player.TryGetComponent(out HullContact contact) ? contact.Ships : (LayerMask)0;
        }

        private void Awake()
        {
            if (_bars == null)
            {
                Build();
            }
        }

        private void Build()
        {
            _area = (RectTransform)transform;
            _bars = new RectTransform[_poolSize];
            _fills = new Image[_poolSize];
            for (int i = 0; i < _poolSize; i++)
            {
                RectTransform bar = Instantiate(_template, _area);
                bar.name = $"Bar{i}";
                bar.gameObject.SetActive(false);
                _bars[i] = bar;
                _fills[i] = bar.GetChild(0).GetComponent<Image>();
            }
            _template.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            Tick();
        }

        /// <summary>Places the bars for this frame. Exposed internally for tests.</summary>
        internal void Tick()
        {
            int shown = 0;
            if (_player != null && _camera != null && _player.isActiveAndEnabled)
            {
                int count = Physics.OverlapSphereNonAlloc(_player.transform.position, _reach, _found, _ships, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count && shown < _bars.Length; i++)
                {
                    Broadside other = _found[i].GetComponentInParent<Broadside>();
                    if (other != null && other != _player && other.Afloat && Place(_bars[shown], other))
                    {
                        Fill(_fills[shown], other);
                        shown++;
                    }
                }
            }
            for (int i = 0; i < _bars.Length; i++)
            {
                bool on = i < shown;
                if (_bars[i].gameObject.activeSelf != on)
                {
                    _bars[i].gameObject.SetActive(on);
                }
            }
            Showing = shown;
        }

        private bool Place(RectTransform bar, Broadside ship)
        {
            Vector3 over = ship.Hull.TransformPoint(new Vector3(0f, ship.Type.Freeboard + _overRail, 0f));
            Vector3 screen = _camera.WorldToScreenPoint(over);
            if (screen.z <= 0f || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local))
            {
                return false;
            }
            bar.anchoredPosition = local;
            return true;
        }

        private void Fill(Image fill, Broadside ship)
        {
            ShipDamage damage = ship.GetComponent<ShipDamage>();
            float health = damage != null ? damage.Health : 1f;
            RectTransform rect = fill.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(health, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            fill.color = ship.Side == _player.Side ? _ally : _enemy;
        }
    }
}
