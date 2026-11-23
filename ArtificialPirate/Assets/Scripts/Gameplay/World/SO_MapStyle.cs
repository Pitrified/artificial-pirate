namespace Game.Gameplay.World
{
    using UnityEngine;

    /// <summary>How the maps draw the world: a tile's size and the colours by height. See docs/systems/world.md.</summary>
    [CreateAssetMenu(menuName = "artificial-pirate/Map Style", fileName = "SO_MapStyle")]
    public sealed class SO_MapStyle : ScriptableObject
    {
        [Tooltip("Pixels along a cell's side.")]
        [SerializeField] private int _tileSize = 48;

        [Tooltip("The open sea, and the water over a shore, shading between them over Shallow Depth.")]
        [SerializeField] private Color32 _deep = new Color32(22, 62, 92, 255);
        [SerializeField] private Color32 _shallow = new Color32(70, 150, 160, 255);

        [Tooltip("Depth over which shallow water turns deep (m).")]
        [SerializeField] private float _shallowDepth = 8f;

        [Tooltip("Land from the waterline up to Sand Top, then grass, then rock above Rock Above.")]
        [SerializeField] private Color32 _sand = new Color32(214, 196, 150, 255);
        [SerializeField] private Color32 _grass = new Color32(96, 128, 82, 255);
        [SerializeField] private Color32 _rock = new Color32(140, 128, 112, 255);

        [Tooltip("Height up to which land is beach (m).")]
        [SerializeField] private float _sandTop = 0.8f;

        [Tooltip("Height above which land is rock (m).")]
        [SerializeField] private float _rockAbove = 20f;

        [Tooltip("Cells across the full map's view when it opens.")]
        [SerializeField] private float _viewCells = 7f;

        [Tooltip("Cells across the full map's view zoomed in as far as it goes.")]
        [SerializeField] private float _closestCells = 2f;

        [Tooltip("A cell not yet explored.")]
        [SerializeField] private Color32 _unexplored = new Color32(28, 30, 36, 255);

        [Header("Ships")]
        [Tooltip("The player's ship's icon.")]
        [SerializeField] private Color _playerTint = Color.white;

        [Tooltip("A ship of another side.")]
        [SerializeField] private Color _enemyTint = new Color(0.92f, 0.28f, 0.22f, 1f);

        [Tooltip("A ship of the player's side.")]
        [SerializeField] private Color _allyTint = new Color(0.4f, 0.85f, 0.45f, 1f);

        [Tooltip("An icon's pixels per metre of its ship's length, on the minimap.")]
        [SerializeField] private float _minimapPixelsPerMetre = 0.5f;

        [Tooltip("An icon's pixels per metre of its ship's length, on the full map.")]
        [SerializeField] private float _fullMapPixelsPerMetre = 1f;

        [Tooltip("The smallest an icon is drawn, in pixels.")]
        [SerializeField] private float _iconLeastPixels = 8f;

        /// <summary>Pixels along a cell's side.</summary>
        public int TileSize => _tileSize;

        /// <summary>Cells across the full map's view when it opens.</summary>
        public float ViewCells => _viewCells;

        /// <summary>Cells across the full map's view zoomed in as far as it goes.</summary>
        public float ClosestCells => _closestCells;

        /// <summary>A cell not yet explored.</summary>
        public Color32 Unexplored => _unexplored;

        /// <summary>A ship's icon's tint: the player's own, a ship of its side, or of another.</summary>
        public Color Tint(bool players, bool sameSide) => players ? _playerTint : sameSide ? _allyTint : _enemyTint;

        /// <summary>The side of a ship's icon in pixels on the minimap, from the ship's length in metres.</summary>
        public float MinimapIconPixels(float length) => Mathf.Max(_iconLeastPixels, length * _minimapPixelsPerMetre);

        /// <summary>The side of a ship's icon in pixels on the full map, from the ship's length in metres.</summary>
        public float FullMapIconPixels(float length) => Mathf.Max(_iconLeastPixels, length * _fullMapPixelsPerMetre);

        /// <summary>The colour of ground at <paramref name="height"/> metres (the waterline is 0).</summary>
        public Color32 ColourAt(float height)
        {
            if (height < 0f)
            {
                return Color32.Lerp(_shallow, _deep, -height / _shallowDepth);
            }
            if (height < _sandTop)
            {
                return _sand;
            }
            return height < _rockAbove ? _grass : _rock;
        }
    }
}
