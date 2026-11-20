namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Game.Systems;

    /// <summary>
    /// Manages the persistent UI root: full-screen loading overlay and pause overlay.
    /// Lives in the Persistent scene on the <c>UIRoot</c> GameObject; owned by GameManager.
    /// All overlays default to hidden. Call <see cref="ShowLoading"/> or
    /// <see cref="ShowScreen"/> to reveal them.
    /// </summary>
    public sealed class UISystem : MonoBehaviour, IUISystem
    {
        // --------------------------------------------------------------------- element names

        private const string SafeAreaContainerName  = "safe-area-container";
        private const string BtnResumeName          = "btn-resume";
        private const string BtnReturnToMenuName    = "btn-return-to-menu";
        private const string BtnDiagnosticsName     = "btn-diagnostics";

        // --------------------------------------------------------------------- serialized

        [SerializeField] private UIDocument _loadingDocument;
        [SerializeField] private UIDocument _pauseDocument;
        [SerializeField] private UIDocument _diagnosticsDocument;
        [SerializeField] private UIDocument _mapDocument;
        [SerializeField] private DiagnosticsRecorder _diagnosticsRecorder;

        // --------------------------------------------------------------------- private state

        private VisualElement _loadingRoot;
        private VisualElement _pauseRoot;
        private VisualElement _diagnosticsRoot;
        private VisualElement _mapRoot;
        private DiagnosticsPanel _diagnostics;
        private MapPanel _map;

        private bool _loadingVisible;
        private bool _pauseVisible;
        private bool _diagnosticsVisible;
        private bool _mapVisible;

        private Rect       _lastSafeArea;
        private Vector2Int _lastScreenSize;

        // --------------------------------------------------------------------- IUISystem

        /// <inheritdoc/>
        public event Action OnResumeRequested;

        /// <inheritdoc/>
        public event Action OnReturnToMenuRequested;

        /// <inheritdoc/>
        public event Action OnMapCloseRequested;

        /// <inheritdoc/>
        public bool IsAnyOverlayOpen => _loadingVisible || _pauseVisible || _diagnosticsVisible || _mapVisible;

        /// <summary>The diagnostics panel, for tests.</summary>
        internal DiagnosticsPanel Diagnostics => _diagnostics;

        /// <summary>The map document's root, for tests.</summary>
        internal VisualElement MapRoot => _mapRoot;

        /// <summary>The map's panel, for tests.</summary>
        internal MapPanel Map => _map;

        // --------------------------------------------------------------------- Unity lifecycle

        private void Start()
        {
            _loadingRoot = BindDocument(_loadingDocument, "Loading");
            _pauseRoot   = BindDocument(_pauseDocument,   "Pause");
            _diagnosticsRoot = BindDocument(_diagnosticsDocument, "Diagnostics");
            _mapRoot = BindDocument(_mapDocument, "Map");

            RegisterPauseButtons();
            if (_diagnosticsRoot != null)
            {
                _diagnostics = new DiagnosticsPanel(_diagnosticsRoot, _diagnosticsRecorder, new PlatformDiagnosticsExport());
                _diagnostics.CloseRequested += () => HideScreen(ScreenId.Diagnostics);
            }
            if (_mapRoot != null)
            {
                _map = new MapPanel(_mapRoot);
                _map.CloseRequested += () => OnMapCloseRequested?.Invoke();
            }

            SetVisible(_loadingRoot, false);
            SetVisible(_pauseRoot,   false);
            SetVisible(_diagnosticsRoot, false);
            SetVisible(_mapRoot, false);

            _lastSafeArea   = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            ApplySafeArea();

            Log.Info(LogCat.UI, "UISystem initialized.");
        }

        private void OnDestroy()
        {
            _map?.Dispose();
        }

        private void Update()
        {
            // Safe area should only change when the device rotates or keyboard appears;
            // checking every frame is cheap (two struct comparisons, no allocations).
            Rect       currentArea = Screen.safeArea;
            Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);

            if (currentArea != _lastSafeArea || currentSize != _lastScreenSize)
            {
                _lastSafeArea   = currentArea;
                _lastScreenSize = currentSize;
                ApplySafeArea();
            }
        }

        // --------------------------------------------------------------------- IUISystem implementation

        /// <inheritdoc/>
        public void ShowLoading()
        {
            SetVisible(_loadingRoot, true);
            _loadingVisible = true;
            Log.Info(LogCat.UI, "Loading overlay shown.");
        }

        /// <inheritdoc/>
        public void HideLoading()
        {
            SetVisible(_loadingRoot, false);
            _loadingVisible = false;
        }

        /// <inheritdoc/>
        public void ShowMap(Texture2D map, Vector2 shipUv, float shipYawDeg, float startView, float closestView)
        {
            if (_map == null)
            {
                Log.Warn(LogCat.UI, "ShowMap: the map document is not bound.");
                return;
            }
            SetVisible(_mapRoot, true);
            _mapVisible = true;
            _map.Show(map, shipUv, shipYawDeg, startView, closestView);
            Log.Info(LogCat.UI, "Map shown.");
        }

        /// <inheritdoc/>
        public void ShowScreen(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Pause:
                    SetVisible(_pauseRoot, true);
                    _pauseVisible = true;
                    Log.Info(LogCat.UI, "Pause screen shown.");
                    break;
                case ScreenId.Diagnostics:
                    _diagnostics?.Refresh();
                    SetVisible(_diagnosticsRoot, true);
                    _diagnosticsVisible = true;
                    break;
                default:
                    Log.Warn(LogCat.UI, "ShowScreen: ScreenId {0} is not handled by UISystem.", id);
                    break;
            }
        }

        /// <inheritdoc/>
        public void HideScreen(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Pause:
                    SetVisible(_pauseRoot, false);
                    _pauseVisible = false;
                    break;
                case ScreenId.Diagnostics:
                    SetVisible(_diagnosticsRoot, false);
                    _diagnosticsVisible = false;
                    break;
                case ScreenId.Map:
                    SetVisible(_mapRoot, false);
                    _mapVisible = false;
                    break;
                default:
                    Log.Warn(LogCat.UI, "HideScreen: ScreenId {0} is not handled by UISystem.", id);
                    break;
            }
        }

        // --------------------------------------------------------------------- private helpers

        private VisualElement BindDocument(UIDocument doc, string debugName)
        {
            if (doc == null)
            {
                Log.Error(LogCat.UI, "{0} UIDocument is not assigned on UISystem.", debugName);
                return null;
            }

            VisualElement root = doc.rootVisualElement;
            if (root == null)
            {
                Log.Error(
                    LogCat.UI,
                    "{0} UIDocument has no rootVisualElement; UXML asset may be missing.",
                    debugName);
                return null;
            }

            return root;
        }

        private void RegisterPauseButtons()
        {
            if (_pauseRoot == null)
                return;

            Button btnResume = _pauseRoot.Q<Button>(BtnResumeName);
            if (btnResume != null)
                btnResume.clicked += () => OnResumeRequested?.Invoke();

            Button btnMenu = _pauseRoot.Q<Button>(BtnReturnToMenuName);
            if (btnMenu != null)
                btnMenu.clicked += () => OnReturnToMenuRequested?.Invoke();

            Button btnDiagnostics = _pauseRoot.Q<Button>(BtnDiagnosticsName);
            if (btnDiagnostics != null)
                btnDiagnostics.clicked += () => ShowScreen(ScreenId.Diagnostics);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplySafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Rect  safe         = Screen.safeArea;
            float paddingLeft  = safe.xMin;
            float paddingRight = Screen.width  - safe.xMax;
            float paddingTop   = Screen.height - safe.yMax;
            float paddingBot   = safe.yMin;

            ApplySafeAreaInsets(_loadingRoot, paddingLeft, paddingRight, paddingTop, paddingBot);
            ApplySafeAreaInsets(_pauseRoot,   paddingLeft, paddingRight, paddingTop, paddingBot);
            ApplySafeAreaInsets(_mapRoot,     paddingLeft, paddingRight, paddingTop, paddingBot);
        }

        private static void ApplySafeAreaInsets(
            VisualElement root,
            float left, float right, float top, float bottom)
        {
            if (root == null)
                return;

            VisualElement container = root.Q<VisualElement>(SafeAreaContainerName);
            if (container == null)
                return;

            container.style.paddingLeft   = left;
            container.style.paddingRight  = right;
            container.style.paddingTop    = top;
            container.style.paddingBottom = bottom;
        }
    }
}
