namespace Game.Core
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Systems;
    using Game.Gameplay;
    using Game.Gameplay.World;
    using Game.UI;

    /// <summary>
    /// Wires Game scene components (ShipController, CameraController) with Persistent scene
    /// systems (InputManager); hands the world to GameManager to begin its seed, then wires the
    /// ship, and the ships that come back when sunk, to the world's ground, opens the map when the minimap is tapped, shows the ship's hull on
    /// the hull bar once its save is restored, bars over the other ships near it, shakes the camera on
    /// its volleys and the hits it takes, follows the camera setting (and the HUD's camera button
    /// cycles it), and shows the HUD only
    /// while playing, since its canvas sorts above the screens shown over the paused game. Place this on
    /// any root object in Game.unity and assign the serialized references in the Inspector.
    /// </summary>
    public sealed class GameSceneWiring : MonoBehaviour
    {
        [SerializeField] private ShipController _ship;
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldStreamer _world;
        [SerializeField] private WorldMap _worldMap;
        [SerializeField] private Minimap _minimap;
        [SerializeField] private HullBar _hullBar;
        [SerializeField] private ShipBars _shipBars;

        [Tooltip("The HUD's camera button, which cycles the cameras.")]
        [SerializeField] private Button _cameraButton;
        [SerializeField] private GameObject _hud;

        [Tooltip("The ships that come back when sunk, given the world's ground as the player's ship is.")]
        [SerializeField] private ShipRespawn[] _respawns = System.Array.Empty<ShipRespawn>();

        private GameManager _gameManager;
        private Broadside _playerGuns;
        private ShipDamage _playerDamage;

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
                gm.Settings.Changed += OnSettingsChanged;
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
            }
            if (_shipBars != null)
            {
                _shipBars.Show(_playerGuns, _camera.GetComponent<Camera>());
            }
        }

        private void OnSettingsChanged() => _camera.SetView(_gameManager.Settings.Camera, _gameManager.Settings.HelmRides);

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
                    _gameManager.Settings.Changed -= OnSettingsChanged;
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
