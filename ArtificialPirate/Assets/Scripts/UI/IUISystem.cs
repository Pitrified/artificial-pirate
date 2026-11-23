namespace Game.UI
{
    using System;
    using UnityEngine;
    using Game.Systems;

    /// <summary>Contract for the persistent UI root that shows and hides screen overlays.</summary>
    public interface IUISystem
    {
        /// <summary>Returns true when any overlay (loading, pause, diagnostics, map, game over or settings) is currently visible.</summary>
        bool IsAnyOverlayOpen { get; }

        /// <summary>Shows the full-screen loading overlay.</summary>
        void ShowLoading();

        /// <summary>Hides the full-screen loading overlay.</summary>
        void HideLoading();

        /// <summary>Shows the named screen overlay.</summary>
        void ShowScreen(ScreenId id);

        /// <summary>Hides the named screen overlay.</summary>
        void HideScreen(ScreenId id);

        /// <summary>
        /// Shows the full map: <paramref name="map"/>, with the ship at <paramref name="shipUv"/>
        /// (0 to 1 from its south-west corner) heading <paramref name="shipYawDeg"/> degrees
        /// clockwise from north. The view starts centred on the ship showing <paramref name="startView"/>
        /// of the map's side and zooms in to <paramref name="closestView"/>. Hidden with
        /// <see cref="HideScreen"/> and <see cref="ScreenId.Map"/>.
        /// </summary>
        void ShowMap(Texture2D map, Vector2 shipUv, float shipYawDeg, float startView, float closestView);

        /// <summary>Gives the settings screen the settings it shows and changes.</summary>
        void BindSettings(GameSettings settings);

        /// <summary>Fired when the player taps the Close button on the map.</summary>
        event Action OnMapCloseRequested;

        /// <summary>Fired when the player taps the Resume button on the pause overlay.</summary>
        event Action OnResumeRequested;

        /// <summary>Fired when the player taps the Main Menu button on the pause overlay or the game over screen.</summary>
        event Action OnReturnToMenuRequested;
    }
}
