namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Game.Core;
    using Game.Gameplay;
    using Game.UI;

    /// <summary>
    /// The player's ship sinking in the real scenes: a ruined hull goes under, the game over screen
    /// shows over a hidden HUD, the save is gone, and its Main Menu button leads to the menu.
    /// </summary>
    public sealed class GameOverTests
    {
        private const int WaitFrames = 900;

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator RuinedShip_Sinks_GameOver_MainMenuLeadsToTheMenu()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
                yield return null;
            yield return WaitForState(GameState.Playing);
            yield return null;

            ShipController ship = GameManager.Instance.PlayerShip;
            ShipDamage damage = ship.Damage;
            Assert.IsNotNull(damage, "The player's ship takes damage");
            Minimap minimap = Object.FindAnyObjectByType<Minimap>();

            damage.Hit(ship.transform.position, 1f);
            Assert.IsTrue(damage.Sinking);
            damage.Tick(ship.Type.SinkSeconds);

            Assert.AreEqual(GameState.GameOver, GameManager.Instance.CurrentState);
            Assert.IsFalse(ship.gameObject.activeSelf, "The ship has gone under");
            Assert.IsFalse(minimap.gameObject.activeInHierarchy, "The HUD is hidden");
            Assert.IsFalse(GameManager.Instance.SaveSystem.HasSave, "No Continue");
            UISystem ui = Object.FindAnyObjectByType<UISystem>();
            Assert.AreEqual(DisplayStyle.Flex, ui.GameOverRoot.style.display.value);

            Button menu = ui.GameOverRoot.Q<Button>("btn-game-over-menu");
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = menu;
                menu.SendEvent(submit);
            }
            yield return WaitForState(GameState.Menu);

            Assert.AreEqual(DisplayStyle.None, ui.GameOverRoot.style.display.value);
            Assert.IsFalse(GameManager.Instance.SaveSystem.HasSave, "Still no Continue");
        }

        private static IEnumerator WaitForState(GameState state)
        {
            for (int i = 0; i < WaitFrames; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == state)
                {
                    yield break;
                }
                yield return null;
            }
            Assert.Fail($"Timed out waiting for {state}");
        }
    }
}
