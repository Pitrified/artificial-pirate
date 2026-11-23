namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Game.Gameplay;
    using Game.Gameplay.World;

    /// <summary>
    /// The HUD's minimap, north up: the 3 x 3 cells around the ship drawn from the
    /// <see cref="WorldMap"/> into a texture, redrawn when the ship enters a cell or more is
    /// explored, painting at most <see cref="_paintPerFrame"/> new tiles a frame. The image shows
    /// a window <see cref="_viewCells"/> wide centred on the ship. The ships in the window show as their
    /// types' icons (<see cref="SO_ShipType.MapIcon"/>), turned with their headings, tinted by side
    /// and sized by length (<see cref="SO_MapStyle"/>): the player's at the centre, the others from a
    /// fixed pool. Tapping it raises <see cref="Clicked"/>. See docs/systems/world.md.
    /// </summary>
    public sealed class Minimap : MonoBehaviour, IPointerClickHandler
    {
        private const int Cells = 3;
        private const int MaxOthers = 8;

        [SerializeField] private WorldMap _map;

        [Tooltip("What the minimap is centred on: the player's ship.")]
        [SerializeField] private Transform _target;

        [SerializeField] private RawImage _image;

        [Tooltip("The player's ship's icon, at the image's centre; the other ships' icons are copies of it.")]
        [SerializeField] private RawImage _ship;

        [Tooltip("Cells shown across the minimap, at most 2.")]
        [SerializeField] private float _viewCells = 2f;

        [Tooltip("New map tiles painted a frame.")]
        [SerializeField] private int _paintPerFrame = 1;

        private Texture2D _texture;
        private readonly Collider[] _found = new Collider[MaxOthers * 2];
        private RawImage[] _others;
        private Broadside _player;
        private LayerMask _ships;
        private Color32[] _pixels;
        private WorldGenerator _drawnWorld;
        private CellCoord _centre;
        private int _drawnVersion = -1;
        private bool _complete;

        /// <summary>Raised when the minimap is tapped.</summary>
        public event Action Clicked;

        /// <summary>The minimap's picture, for tests.</summary>
        internal Texture2D Texture => _texture;

        /// <summary>Whether every cell around the ship is drawn, for tests.</summary>
        internal bool Complete => _complete;

        /// <summary>How many other ships' icons are showing, for tests.</summary>
        internal int OthersShowing { get; private set; }

        /// <summary>The icon of another ship, for tests.</summary>
        internal RawImage OtherIcon(int index) => _others[index];

        /// <summary>Centres the minimap on <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target)
        {
            _target = target;
            ShowPlayer();
        }

        /// <summary>Wires the map, the target and the images (minimaps built in code: tests).</summary>
        internal void Configure(WorldMap map, Transform target, RawImage image, RawImage ship)
        {
            _map = map;
            _target = target;
            _image = image;
            _ship = ship;
            Awake();
        }

        private void Awake()
        {
            if (_map == null || _texture != null)
            {
                return;
            }
            int size = Cells * _map.TileSize;
            _pixels = new Color32[size * size];
            _texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Minimap",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            if (_image != null)
            {
                _image.texture = _texture;
            }
            if (_ship != null)
            {
                _others = new RawImage[MaxOthers];
                for (int i = 0; i < MaxOthers; i++)
                {
                    _others[i] = Instantiate(_ship, _ship.transform.parent);
                    _others[i].name = $"Ship{i}";
                    _others[i].gameObject.SetActive(false);
                }
                _ship.transform.SetAsLastSibling();
            }
            ShowPlayer();
        }

        /// <summary>The player's ship's icon, tint and size; and what the other ships are found by.</summary>
        private void ShowPlayer()
        {
            if (_target == null || _ship == null || _map == null || !_target.TryGetComponent(out HullContact contact))
            {
                return;
            }
            _target.TryGetComponent(out _player);
            _ships = contact.Ships;
            Dress(_ship, contact.Type, _map.Style.Tint(true, true));
        }

        private void Dress(RawImage icon, SO_ShipType type, Color tint)
        {
            if (type != null && type.MapIcon != null)
            {
                icon.texture = type.MapIcon;
            }
            icon.color = tint;
            float side = _map.Style.MinimapIconPixels(type != null ? 2f * type.HalfLength : 0f);
            icon.rectTransform.sizeDelta = new Vector2(side, side);
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        private void Update()
        {
            Tick();
        }

        /// <inheritdoc/>
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }

        /// <summary>One frame: redraws what changed, then places the window and the arrow. Exposed internally for tests.</summary>
        internal void Tick()
        {
            WorldGenerator world = _map == null ? null : _map.World;
            if (world == null || _target == null || _texture == null)
            {
                return;
            }

            Vector3 at = _target.position;
            CellCoord cell = world.CellAt(at.x, at.z);
            int version = _map.Explored.Version;
            if (world != _drawnWorld || !cell.Equals(_centre) || version != _drawnVersion)
            {
                _drawnWorld = world;
                _centre = cell;
                _drawnVersion = version;
                _complete = false;
            }
            if (!_complete)
            {
                _complete = _map.Compose(new CellCoord(cell.X - 1, cell.Z - 1), Cells, Cells, _pixels, _paintPerFrame);
                _texture.SetPixels32(_pixels);
                _texture.Apply(false);
            }

            float cellSize = world.Settings.CellSize;
            Vector2 offset = (new Vector2(at.x, at.z) - world.CellCentre(cell)) / (cellSize * Cells);
            float window = _viewCells / Cells;
            if (_image != null)
            {
                _image.uvRect = new Rect(0.5f + offset.x - window * 0.5f, 0.5f + offset.y - window * 0.5f, window, window);
            }
            if (_ship != null)
            {
                _ship.rectTransform.localEulerAngles = new Vector3(0f, 0f, -_target.eulerAngles.y);
                PlaceOthers(at, _viewCells * cellSize);
            }
        }

        /// <summary>The other ships within the window of <paramref name="windowMetres"/> round <paramref name="at"/>, each at its place on the image.</summary>
        private void PlaceOthers(Vector3 at, float windowMetres)
        {
            int shown = 0;
            if (_player != null && _image != null)
            {
                float half = windowMetres * 0.5f;
                float pixelsPerMetre = _image.rectTransform.rect.width / windowMetres;
                int count = Physics.OverlapSphereNonAlloc(at, half * Mathf.Sqrt(2f), _found, _ships, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count && shown < _others.Length; i++)
                {
                    Broadside other = _found[i].GetComponentInParent<Broadside>();
                    if (other == null || other == _player || !other.Afloat)
                    {
                        continue;
                    }
                    Vector3 offset = other.transform.position - at;
                    if (Mathf.Abs(offset.x) > half || Mathf.Abs(offset.z) > half)
                    {
                        continue;
                    }
                    RawImage icon = _others[shown++];
                    Dress(icon, other.Type, _map.Style.Tint(false, other.Side == _player.Side));
                    icon.rectTransform.anchoredPosition = _ship.rectTransform.anchoredPosition + new Vector2(offset.x, offset.z) * pixelsPerMetre;
                    icon.rectTransform.localEulerAngles = new Vector3(0f, 0f, -other.transform.eulerAngles.y);
                    if (!icon.gameObject.activeSelf)
                    {
                        icon.gameObject.SetActive(true);
                    }
                }
            }
            for (int i = shown; _others != null && i < _others.Length; i++)
            {
                if (_others[i].gameObject.activeSelf)
                {
                    _others[i].gameObject.SetActive(false);
                }
            }
            OthersShowing = shown;
        }
    }
}
