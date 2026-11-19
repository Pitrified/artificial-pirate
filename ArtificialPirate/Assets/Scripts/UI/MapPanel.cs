namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The full map's view: a map picture kept square in the space it has, the ship's arrow placed
    /// on it and turned by the heading, and a close button. Bound by <see cref="UISystem"/> to its
    /// document. See docs/systems/world.md.
    /// </summary>
    public sealed class MapPanel
    {
        private const string FrameName = "map-frame";
        private const string ImageName = "map-image";
        private const string ShipName = "map-ship";
        private const string CloseName = "btn-close-map";
        private const float Percent = 100f;

        private readonly VisualElement _frame;
        private readonly VisualElement _image;
        private readonly VisualElement _ship;
        private readonly Texture2D _arrow;

        /// <summary>Raised when the close button is tapped.</summary>
        public event Action CloseRequested;

        /// <summary>Binds the panel to the map document's <paramref name="root"/>.</summary>
        public MapPanel(VisualElement root)
        {
            _frame = root.Q<VisualElement>(FrameName);
            _image = root.Q<VisualElement>(ImageName);
            _ship = root.Q<VisualElement>(ShipName);
            _arrow = MapArrow.Create();
            if (_ship != null)
            {
                _ship.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_arrow));
            }
            _frame?.RegisterCallback<GeometryChangedEvent>(_ => Square());

            Button close = root.Q<Button>(CloseName);
            if (close != null)
            {
                close.clicked += () => CloseRequested?.Invoke();
            }
        }

        /// <summary>
        /// Shows <paramref name="map"/> with the ship at <paramref name="shipUv"/> (0 to 1 from the
        /// south-west corner) heading <paramref name="shipYawDeg"/> degrees clockwise from north.
        /// </summary>
        public void Show(Texture2D map, Vector2 shipUv, float shipYawDeg)
        {
            if (_image == null || _ship == null)
            {
                return;
            }
            _image.style.backgroundImage = new StyleBackground(Background.FromTexture2D(map));
            _ship.style.left = Length.Percent(shipUv.x * Percent);
            _ship.style.top = Length.Percent((1f - shipUv.y) * Percent);
            _ship.style.rotate = new Rotate(new Angle(shipYawDeg, AngleUnit.Degree));
            Square();
        }

        /// <summary>Destroys the arrow texture.</summary>
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_arrow);
        }

        private void Square()
        {
            if (_frame == null || _image == null)
            {
                return;
            }
            Rect space = _frame.contentRect;
            float side = Mathf.Min(space.width, space.height);
            if (float.IsNaN(side) || side <= 0f)
            {
                return;
            }
            _image.style.width = side;
            _image.style.height = side;
        }
    }
}
