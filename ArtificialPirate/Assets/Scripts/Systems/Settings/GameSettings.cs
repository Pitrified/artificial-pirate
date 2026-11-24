namespace Game.Systems
{
    using System;
    using UnityEngine;

    /// <summary>Which camera follows the player's ship.</summary>
    public enum CameraView
    {
        /// <summary>Behind and above the ship, following it.</summary>
        Chase,

        /// <summary>High and a little astern, looking down toward the bow.</summary>
        BirdsEye,

        /// <summary>The helmsman's eyes, looking over the bow.</summary>
        Helm,
    }

    /// <summary>The kind of ship the player sails.</summary>
    public enum PlayerShipKind
    {
        /// <summary>The sloop.</summary>
        Sloop,

        /// <summary>The brig.</summary>
        Brig,

        /// <summary>The frigate.</summary>
        Frigate,
    }

    /// <summary>
    /// The player's settings, kept in <c>PlayerPrefs</c> as the volumes are: the camera, whether the
    /// helm camera rides the hull's pitch and roll, and the ship sailed (a choice in development
    /// builds). Read when made; each change is written at once and raised as <see cref="Changed"/>.
    /// See docs/systems/game-manager.md.
    /// </summary>
    public sealed class GameSettings
    {
        /// <summary>The <c>PlayerPrefs</c> keys, for tests that clear them.</summary>
        public static readonly string[] Keys = { CameraKey, HelmRidesKey, ShipKey };

        private const string CameraKey = "settings.camera";
        private const string HelmRidesKey = "settings.helmRides";
        private const string ShipKey = "settings.ship";
        private const int CameraViews = 3;
        private static readonly int ShipKinds = System.Enum.GetValues(typeof(PlayerShipKind)).Length;

        /// <summary>Reads the settings, each at its default when it was never set.</summary>
        public GameSettings()
        {
            Camera = (CameraView)Mathf.Clamp(PlayerPrefs.GetInt(CameraKey, (int)CameraView.Chase), 0, CameraViews - 1);
            HelmRides = PlayerPrefs.GetInt(HelmRidesKey, 0) != 0;
            Ship = (PlayerShipKind)Mathf.Clamp(PlayerPrefs.GetInt(ShipKey, (int)PlayerShipKind.Sloop), 0, ShipKinds - 1);
        }

        /// <summary>Raised after any setting changes.</summary>
        public event Action Changed;

        /// <summary>The camera that follows the player's ship.</summary>
        public CameraView Camera { get; private set; }

        /// <summary>Whether the helm camera pitches and rolls with the hull; otherwise it keeps the horizon level.</summary>
        public bool HelmRides { get; private set; }

        /// <summary>The ship the player sails from the next game; only development builds offer a choice.</summary>
        public PlayerShipKind Ship { get; private set; }

        /// <summary>Whether this build offers the choice of ship.</summary>
        public static bool ShipChoiceOffered => Debug.isDebugBuild;

        /// <summary>Sets the camera.</summary>
        public void SetCamera(CameraView view)
        {
            Camera = view;
            Write(CameraKey, (int)view);
        }

        /// <summary>The next camera, from the last back to the first.</summary>
        public void NextCamera() => SetCamera((CameraView)(((int)Camera + 1) % CameraViews));

        /// <summary>Sets whether the helm camera rides the hull.</summary>
        public void SetHelmRides(bool rides)
        {
            HelmRides = rides;
            Write(HelmRidesKey, rides ? 1 : 0);
        }

        /// <summary>Sets the ship sailed from the next game.</summary>
        public void SetShip(PlayerShipKind ship)
        {
            Ship = ship;
            Write(ShipKey, (int)ship);
        }

        private void Write(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
