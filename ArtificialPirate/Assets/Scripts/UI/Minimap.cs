namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Game.Gameplay.World;

    /// <summary>
    /// The HUD's minimap, north up: the 3 x 3 cells around the ship drawn from the
    /// <see cref="WorldMap"/> into a texture, redrawn when the ship enters a cell or more is
    /// explored, painting at most <see cref="_paintPerFrame"/> new tiles a frame. The image shows
    /// a window <see cref="_viewCells"/> wide centred on the ship, and the ship's arrow turns
    /// with its heading. Tapping it raises <see cref="Clicked"/>. See docs/systems/world.md.
    /// </summary>
    public sealed class Minimap : MonoBehaviour, IPointerClickHandler
    {
        private const int Cells = 3;

        [SerializeField] private WorldMap _map;

        [Tooltip("What the minimap is centred on: the player's ship.")]
        [SerializeField] private Transform _target;

        [SerializeField] private RawImage _image;

        [Tooltip("The ship's arrow, at the image's centre.")]
        [SerializeField] private RawImage _ship;

        [Tooltip("Cells shown across the minimap, at most 2.")]
        [SerializeField] private float _viewCells = 2f;

        [Tooltip("New map tiles painted a frame.")]
        [SerializeField] private int _paintPerFrame = 1;

        private Texture2D _texture;
        private Texture2D _arrow;
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

        /// <summary>Centres the minimap on <paramref name="target"/>: the player's ship.</summary>
        public void Follow(Transform target) => _target = target;

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
            _arrow = MapArrow.Create();
            if (_image != null)
            {
                _image.texture = _texture;
            }
            if (_ship != null)
            {
                _ship.texture = _arrow;
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
            if (_arrow != null)
            {
                Destroy(_arrow);
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
            }
        }
    }
}
