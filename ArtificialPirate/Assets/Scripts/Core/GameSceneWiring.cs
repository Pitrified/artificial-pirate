namespace Game.Core
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Systems;
    using Game.Gameplay;
    using Game.Gameplay.World;
    using Game.UI;

    /// <summary>
    /// Turns on the player's ship of the kind chosen in the settings and points what follows the
    /// player's ship at it; wires Game scene components (ShipController, CameraController) with
    /// Persistent scene systems (InputManager); hands the world to GameManager to begin its seed, then wires the
    /// ship, and the ships that come back when sunk, to the world's ground, opens the map when the minimap is tapped, shows the ship's hull on
    /// the hull bar once its save is restored, bars over the other ships near it, shakes the camera on
    /// its volleys and the hits it takes, follows the camera setting (and the HUD's camera button
    /// cycles it) but shows the chase while the ship sinks, and shows the HUD only
    /// while playing, since its canvas sorts above the screens shown over the paused game. Place this on
    /// any root object in Game.unity and assign the serialized references in the Inspector.
    /// </summary>
    public sealed class GameSceneWiring : MonoBehaviour
    {
        [Tooltip("The player's ships, one for each kind (Sloop, Brig), kept inactive in the scene; the chosen one is turned on.")]
        [SerializeField] private ShipController[] _ships = System.Array.Empty<ShipController>();
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldStreamer _world;
        [SerializeField] private WorldMap _worldMap;
        [SerializeField] private WorldDepthMap _depthMap;
        [SerializeField] private Game.Gameplay.Water.WaterSurface _water;
        [SerializeField] private Game.Gameplay.Water.WaterRipples _ripples;
        [SerializeField] private Minimap _minimap;
        [SerializeField] private HullBar _hullBar;
        [SerializeField] private ShipBars _shipBars;

        [Tooltip("The HUD's camera button, which cycles the cameras.")]
        [SerializeField] private Button _cameraButton;
        [SerializeField] private GameObject _hud;

        [Tooltip("The ships that come back when sunk, given the world's ground as the player's ship is.")]
        [SerializeField] private ShipRespawn[] _respawns = System.Array.Empty<ShipRespawn>();

        private GameManager _gameManager;
        private ShipController _ship;
        private Broadside _playerGuns;
        private ShipDamage _playerDamage;

        private void Awake()
        {
            ChooseShip();
        }

        /// <summary>
        /// Turns on the player's ship of the chosen kind (the sloop where the build offers no choice) and
        /// points everything that follows the player's ship at it.
        /// </summary>
        private void ChooseShip()
        {
            GameManager gm = GameManager.Instance;
            PlayerShipKind kind = gm != null && gm.Settings != null && GameSettings.ShipChoiceOffered ? gm.Settings.Ship : PlayerShipKind.Sloop;
            int chosen = Mathf.Clamp((int)kind, 0, _ships.Length - 1);
            if (_ships.Length == 0 || _ships[chosen] == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: no player's ship for {0}", kind);
                return;
            }
            _ship = _ships[chosen];
            _ship.gameObject.SetActive(true);
            Transform follow = _ship.transform;
            if (_world != null) _world.Follow(follow);
            if (_depthMap != null) _depthMap.Follow(follow);
            if (_water != null) _water.Follow(follow);
            if (_ripples != null) _ripples.Follow(follow);
            if (_minimap != null) _minimap.Follow(follow);
            foreach (ShipRespawn respawn in _respawns)
            {
                if (respawn != null) respawn.Follow(follow);
            }
            Log.Info(LogCat.Gameplay, "GameSceneWiring: the player sails the {0}", kind);
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.RegisterWorld(_world);
            }
            WireGround();
            if (_minimap != null)
            {
                _minimap.Clicked += OpenMap;
            }

            if (gm == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: GameManager.Instance is null - ship/camera not wired");
                return;
            }

            _gameManager = gm;
            gm.StateChanged += OnStateChanged;
            ShowHud(gm.CurrentState);

            if (_ship == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: ShipController not assigned");
            }
            else
            {
                _ship.Inject(gm.InputManager);
                gm.RegisterShip(_ship);
                Log.Info(LogCat.Gameplay, "GameSceneWiring: ShipController injected with InputManager");
                if (_hullBar != null)
                {
                    _hullBar.Show(_ship.Damage);
                }
            }

            if (_camera == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: CameraController not assigned");
            }
            else if (_ship != null && _ship.Type == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: the ship has no type - camera not wired");
            }
            else if (_ship != null)
            {
                Transform hull = _ship.TryGetComponent(out HullContact contact) ? contact.Hull : null;
                _camera.SetShip(_ship.transform, hull, _ship.Type, gm.Settings.Camera, gm.Settings.HelmRides);
                _camera.SnapToTarget();
                gm.Settings.Changed += ShowView;
                if (_cameraButton != null)
                {
                    _cameraButton.onClick.AddListener(gm.Settings.NextCamera);
                }
                Log.Info(LogCat.Gameplay, "GameSceneWiring: CameraController wired to ship");
                WireFight();
            }
        }

        private void WireFight()
        {
            _ship.TryGetComponent(out _playerGuns);
            _playerDamage = _ship.Damage;
            if (_playerGuns != null)
            {
                _playerGuns.VolleyFired += OnVolley;
            }
            if (_playerDamage != null)
            {
                _playerDamage.Damaged += OnPlayerHit;
                _playerDamage.SinkingStarted += ShowView;
                _playerDamage.Restored += ShowView;
            }
            if (_shipBars != null)
            {
                _shipBars.Show(_playerGuns, _camera.GetComponent<Camera>());
            }
        }

        /// <summary>Shows the chosen view, or the chase while the player's ship sinks.</summary>
        private void ShowView()
        {
            GameSettings settings = _gameManager.Settings;
            CameraView view = _playerDamage != null && _playerDamage.Sinking ? CameraView.Chase : settings.Camera;
            _camera.SetView(view, settings.HelmRides);
        }

        private void OnVolley(bool starboard) => _camera.Shake(_camera.Tuning.VolleyShake);

        private void OnPlayerHit(Vector3 point, float amount) => _camera.Shake(amount * _camera.Tuning.HitShakePerHealth);

        private void OnDestroy()
        {
            if (_minimap != null)
            {
                _minimap.Clicked -= OpenMap;
            }
            if (_gameManager != null)
            {
                _gameManager.StateChanged -= OnStateChanged;
                if (_gameManager.Settings != null)
                {
                    _gameManager.Settings.Changed -= ShowView;
                }
            }
            if (_cameraButton != null)
            {
                _cameraButton.onClick.RemoveAllListeners();
            }
            if (_playerGuns != null)
            {
                _playerGuns.VolleyFired -= OnVolley;
            }
            if (_playerDamage != null)
            {
                _playerDamage.Damaged -= OnPlayerHit;
                _playerDamage.SinkingStarted -= ShowView;
                _playerDamage.Restored -= ShowView;
            }
        }

        private void OnStateChanged(GameState from, GameState to) => ShowHud(to);

        private void ShowHud(GameState state)
        {
            if (_hud != null)
            {
                _hud.SetActive(state == GameState.Playing);
            }
        }

        private void OpenMap()
        {
            if (GameManager.Instance != null && _ship != null)
            {
                GameManager.Instance.OpenMap(_worldMap, _ship.transform);
            }
        }

        private void WireGround()
        {
            if (_world == null || _ship == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: WorldStreamer or ShipController not assigned - the ship cannot run aground");
                return;
            }
            if (_ship.TryGetComponent(out HullContact contact))
            {
                contact.UseGround(_world.World);
            }
            foreach (ShipRespawn respawn in _respawns)
            {
                if (respawn != null)
                {
                    respawn.UseGround(_world.World);
                }
            }
        }
    }
}
