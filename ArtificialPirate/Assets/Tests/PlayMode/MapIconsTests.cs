namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay.World;
    using Game.UI;

    /// <summary>
    /// The ships on the maps in the Game scene: the minimap shows the anchored brig near the start as
    /// an enemy's icon, and the full map both brigs beside the player's ship.
    /// </summary>
    public sealed class MapIconsTests
    {
        private const int WaitFrames = 900;

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator Maps_ShowTheOtherShips()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            yield return WaitForState(GameState.Playing);
            yield return null;
            yield return null;

            Minimap minimap = Object.FindAnyObjectByType<Minimap>();
            Assert.That(minimap.OthersShowing, Is.GreaterThanOrEqualTo(1), "the anchored brig on the minimap");
            Color tint = minimap.OtherIcon(0).color;
            Assert.That(tint.r, Is.GreaterThan(tint.g), "an enemy's red");

            GameManager.Instance.OpenMap(Object.FindAnyObjectByType<WorldMap>(), GameManager.Instance.PlayerShip.transform);
            UISystem ui = Object.FindAnyObjectByType<UISystem>();
            Assert.AreEqual(2, ui.Map.OthersShowing, "both brigs on the full map");
        }

        private static IEnumerator WaitForState(GameState state)
        {
            for (int i = 0; i < WaitFrames && (GameManager.Instance == null || GameManager.Instance.CurrentState != state); i++)
            {
                yield return null;
            }
            Assert.AreEqual(state, GameManager.Instance.CurrentState);
        }
    }
}
