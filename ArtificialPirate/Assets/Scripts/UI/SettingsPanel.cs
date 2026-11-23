namespace Game.UI
{
    using System;
    using UnityEngine.UIElements;
    using Game.Systems;

    /// <summary>
    /// The settings screen's buttons over <see cref="GameSettings"/>: a row for the camera, one for the
    /// helm camera's motion and, in development builds, one for the ship; the chosen button of each
    /// row lit. Back raises <see cref="CloseRequested"/>. See docs/systems/ui-system.md.
    /// </summary>
    public sealed class SettingsPanel
    {
        private const string On = "settings-btn--on";

        private readonly GameSettings _settings;
        private readonly Button[] _cameras;
        private readonly Button _steady;
        private readonly Button _riding;
        private readonly Button _sloop;
        private readonly Button _brig;

        /// <summary>Raised when Back is tapped.</summary>
        public event Action CloseRequested;

        /// <summary>Binds the buttons under <paramref name="root"/> to <paramref name="settings"/>.</summary>
        public SettingsPanel(VisualElement root, GameSettings settings, bool offerShip)
        {
            _settings = settings;
            _cameras = new[]
            {
                root.Q<Button>("btn-camera-chase"),
                root.Q<Button>("btn-camera-birdseye"),
                root.Q<Button>("btn-camera-helm"),
            };
            for (int i = 0; i < _cameras.Length; i++)
            {
                var view = (CameraView)i;
                _cameras[i].clicked += () => _settings.SetCamera(view);
            }
            _steady = root.Q<Button>("btn-helm-steady");
            _riding = root.Q<Button>("btn-helm-riding");
            _steady.clicked += () => _settings.SetHelmRides(false);
            _riding.clicked += () => _settings.SetHelmRides(true);
            _sloop = root.Q<Button>("btn-ship-sloop");
            _brig = root.Q<Button>("btn-ship-brig");
            _sloop.clicked += () => _settings.SetShip(PlayerShipKind.Sloop);
            _brig.clicked += () => _settings.SetShip(PlayerShipKind.Brig);
            root.Q<VisualElement>("settings-ship").style.display = offerShip ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<Button>("btn-settings-close").clicked += () => CloseRequested?.Invoke();
            _settings.Changed += Refresh;
            Refresh();
        }

        /// <summary>Lights the chosen button of each row.</summary>
        public void Refresh()
        {
            for (int i = 0; i < _cameras.Length; i++)
            {
                _cameras[i].EnableInClassList(On, (int)_settings.Camera == i);
            }
            _steady.EnableInClassList(On, !_settings.HelmRides);
            _riding.EnableInClassList(On, _settings.HelmRides);
            _sloop.EnableInClassList(On, _settings.Ship == PlayerShipKind.Sloop);
            _brig.EnableInClassList(On, _settings.Ship == PlayerShipKind.Brig);
        }
    }
}
