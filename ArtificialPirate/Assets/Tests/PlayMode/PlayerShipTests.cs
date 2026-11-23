namespace Game.Tests.PlayMode
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay;
    using Game.Systems;

    /// <summary>
    /// The ship chosen in the settings is the one sailed: with the brig chosen a new game sails the
    /// brig, the sloop off and the brig on its side; back to the sloop for the next game. The
    /// settings' keys are saved and put back around the test.
    /// </summary>
    public sealed class PlayerShipTests
    {
        private const int WaitFrames = 900;
        private readonly Dictionary<string, int> _saved = new Dictionary<string, int>();

        [SetUp]
        public void SetUp()
        {
            foreach (string key in GameSettings.Keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    _saved[key] = PlayerPrefs.GetInt(key);
                }
                PlayerPrefs.DeleteKey(key);
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string key in GameSettings.Keys)
            {
                PlayerPrefs.DeleteKey(key);
                if (_saved.TryGetValue(key, out int value))
                {
                    PlayerPrefs.SetInt(key, value);
                }
            }
            _saved.Clear();
            PlayerPrefs.Save(); // the settings save as they change, so the restore must too
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator ChosenShip_IsTheOneSailed()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            GameManager gm = GameManager.Instance;

            gm.Settings.SetShip(PlayerShipKind.Brig);
            yield return NewGame();
            ShipController brig = gm.PlayerShip;
            Assert.AreEqual("SO_Ship_Brig", brig.Type.name, "the brig sails");
            Assert.IsTrue(brig.gameObject.activeInHierarchy);
            Assert.AreEqual(ShipSide.Player, brig.GetComponent<Broadside>().Side);
            Assert.IsTrue(brig.GetComponentInChildren<Transform>(true).Find("Hull") != null);
            foreach (ShipController ship in Object.FindObjectsByType<ShipController>(FindObjectsInactive.Include))
            {
                if (ship != brig && ship.GetComponent<Broadside>().Side == ShipSide.Player)
                {
                    Assert.IsFalse(ship.gameObject.activeSelf, "the sloop is off");
                }
            }

            var back = gm.ReturnToMenuAsync();
            while (!back.IsCompleted)
            {
                yield return null;
            }
            yield return WaitForState(GameState.Menu);
            gm.Settings.SetShip(PlayerShipKind.Sloop);
            yield return NewGame();
            Assert.AreEqual("SO_Ship_Sloop", gm.PlayerShip.Type.name, "the sloop sails again");
        }

        private static IEnumerator NewGame()
        {
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            yield return WaitForState(GameState.Playing);
            yield return null;
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
