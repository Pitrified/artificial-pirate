namespace Game.Tests.PlayMode
{
    using System.Collections;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay;
    using Game.UI;

    /// <summary>
    /// A fight in the Game scene: the player's ship placed abeam of the anchored brig, each on the
    /// other's beam. Both fire and both take damage, and the brig has a bar over it. Sunk, the brig
    /// comes back whole once the player has sailed clear (the delay shortened).
    /// </summary>
    public sealed class FightTests
    {
        private const int WaitFrames = 900;
        private const float FightSeconds = 10f;
        private const float ComeBackDelay = 1f;

        [SetUp]
        public void SetUp()
        {
            Time.captureDeltaTime = 1f / 30f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator Abeam_BothFireAndTakeDamage_ThenTheSunkBrigComesBack()
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

            ShipRespawn anchored = null;
            foreach (ShipRespawn respawn in Object.FindObjectsByType<ShipRespawn>(FindObjectsInactive.Exclude))
            {
                anchored = respawn.Current != null && respawn.Current.GetComponent<CircleHelm>() == null ? respawn : anchored;
            }
            Assert.IsNotNull(anchored, "the anchored brig comes back through a respawn");
            Transform brig = anchored.Current.transform;
            ShipDamage brigDamage = brig.GetComponent<ShipDamage>();

            // On the brig's port beam, heading as it does: each has the other on its beam.
            ShipController player = GameManager.Instance.PlayerShip;
            Vector3 portBeam = -brig.right;
            player.Teleport(brig.position + portBeam * 50f, brig.eulerAngles.y);
            yield return null;

            for (float t = 0f; t < FightSeconds; t += Time.captureDeltaTime)
            {
                yield return null;
            }

            Assert.That(brigDamage.Health, Is.LessThan(1f), "the player's guns hit the brig");
            Assert.That(player.Damage.Health, Is.LessThan(1f), "the brig's guns hit the player");
            Assert.That(Object.FindAnyObjectByType<ShipBars>().Showing, Is.GreaterThan(0), "a bar over the brig");

            typeof(ShipRespawn).GetField("_delay", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(anchored, ComeBackDelay);
            int made = anchored.Made;
            brigDamage.Hit(brig.position, 1f);
            player.Teleport(brig.position + portBeam * 400f, brig.eulerAngles.y);
            for (int i = 0; i < WaitFrames && anchored.Made == made; i++)
            {
                yield return null;
            }

            Assert.AreEqual(made + 1, anchored.Made, "the sunk brig came back");
            Assert.AreEqual(1f, anchored.Current.GetComponent<ShipDamage>().Health, "whole");
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
