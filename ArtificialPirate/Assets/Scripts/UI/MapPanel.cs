namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The full map's view: a square window on the map picture that one finger drags and two
    /// pinch (<see cref="MapView"/>), the ship's arrow placed on the picture and turned by the
    /// heading, and buttons to centre on the ship, reset the zoom and close. Bound by
    /// <see cref="UISystem"/> to its document. See docs/systems/world.md.
    /// </summary>
    public sealed class MapPanel
    {
        private const string FrameName = "map-frame";
        private const string WindowName = "map-window";
        private const string ImageName = "map-image";
        private const string ShipName = "map-ship";
        private const string CloseName = "btn-close-map";
        private const string CentreName = "btn-map-ship";
        private const string ResetZoomName = "btn-map-zoom";
        private const float Percent = 100f;

        private readonly VisualElement _frame;
        private readonly VisualElement _window;
        private readonly VisualElement _image;
        private readonly VisualElement _ship;
        private readonly Texture2D _arrow;
        private readonly MapView _view = new MapView();
        private float _side;

        /// <summary>Raised when the close button is tapped.</summary>
        public event Action CloseRequested;

        /// <summary>Binds the panel to the map document's <paramref name="root"/>.</summary>
        public MapPanel(VisualElement root)
        {
            _frame = root.Q<VisualElement>(FrameName);
            _window = root.Q<VisualElement>(WindowName);
            _image = root.Q<VisualElement>(ImageName);
            _ship = root.Q<VisualElement>(ShipName);
            _arrow = MapArrow.Create();
            if (_ship != null)
            {
                _ship.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_arrow));
            }
            _frame?.RegisterCallback<GeometryChangedEvent>(_ => Layout());
            if (_window != null)
            {
                _window.RegisterCallback<PointerDownEvent>(OnPointerDown);
                _window.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _window.RegisterCallback<PointerUpEvent>(OnPointerUp);
                _window.RegisterCallback<PointerCancelEvent>(e => Release(e.pointerId));
                _window.RegisterCallback<PointerCaptureOutEvent>(e => _view.PointerUp(e.pointerId));
            }

            Bind(root, CloseName, () => CloseRequested?.Invoke());
            Bind(root, CentreName, () => { _view.CentreOnShip(); Place(); });
            Bind(root, ResetZoomName, () => { _view.ResetZoom(); Place(); });
        }

        /// <summary>The view's state, for tests.</summary>
        internal MapView View => _view;

        /// <summary>
        /// Shows <paramref name="map"/> with the ship at <paramref name="shipUv"/> (0 to 1 from the
        /// south-west corner) heading <paramref name="shipYawDeg"/> degrees clockwise from north,
        /// the view centred on the ship showing <paramref name="startView"/> of the map's side and
        /// zooming in to <paramref name="closestView"/>.
        /// </summary>
        public void Show(Texture2D map, Vector2 shipUv, float shipYawDeg, float startView, float closestView)
        {
            if (_image == null || _ship == null)
            {
                return;
            }
            _image.style.backgroundImage = new StyleBackground(Background.FromTexture2D(map));
            _ship.style.left = Length.Percent(shipUv.x * Percent);
            _ship.style.top = Length.Percent((1f - shipUv.y) * Percent);
            _ship.style.rotate = new Rotate(new Angle(shipYawDeg, AngleUnit.Degree));
            _view.Show(shipUv, startView, closestView);
            Layout();
        }

        /// <summary>Destroys the arrow texture.</summary>
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_arrow);
        }

        private static void Bind(VisualElement root, string name, Action action)
        {
            Button button = root.Q<Button>(name);
            if (button != null)
            {
                button.clicked += action;
            }
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            _window.CapturePointer(e.pointerId);
            _view.PointerDown(e.pointerId, _window.WorldToLocal(e.position));
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (_side <= 0f || !_window.HasPointerCapture(e.pointerId))
            {
                return;
            }
            _view.PointerMove(e.pointerId, _window.WorldToLocal(e.position), _side);
            Place();
        }

        private void OnPointerUp(PointerUpEvent e) => Release(e.pointerId);

        private void Release(int pointerId)
        {
            _view.PointerUp(pointerId);
            if (_window.HasPointerCapture(pointerId))
            {
                _window.ReleasePointer(pointerId);
            }
        }

        /// <summary>Keeps the window square in the space it has, then places the picture in it.</summary>
        private void Layout()
        {
            if (_frame == null || _window == null)
            {
                return;
            }
            Rect space = _frame.contentRect;
            float side = Mathf.Min(space.width, space.height);
            if (float.IsNaN(side) || side <= 0f)
            {
                return;
            }
            _side = side;
            _window.style.width = side;
            _window.style.height = side;
            Place();
        }

        private void Place()
        {
            if (_image == null || _side <= 0f)
            {
                return;
            }
            Rect rect = _view.ImageRect(_side);
            _image.style.left = rect.x;
            _image.style.top = rect.y;
            _image.style.width = rect.width;
            _image.style.height = rect.height;
        }
    }
}
