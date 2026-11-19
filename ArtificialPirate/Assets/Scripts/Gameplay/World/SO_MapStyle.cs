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

        [Tooltip("A cell not yet explored.")]
        [SerializeField] private Color32 _unexplored = new Color32(28, 30, 36, 255);

        /// <summary>Pixels along a cell's side.</summary>
        public int TileSize => _tileSize;

        /// <summary>A cell not yet explored.</summary>
        public Color32 Unexplored => _unexplored;

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
