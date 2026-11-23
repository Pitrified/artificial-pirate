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

    /// <summary>
    /// Ships other than the player's. <see cref="ShipRespawn"/> with a ship built in code: a copy is
    /// made at the template's place when it starts; once the copy sinks a whole new one comes after
    /// the delay, and not while the player's ship is within the clearance. The Game scene's circling
    /// brig keeps its circle for a minute without running aground.
    /// </summary>
    public sealed class ShipsAtSeaTests
    {
        private const float Delay = 0.5f;
        private const float Clearance = 300f;
        private const int WaitFrames = 900;
        private static readonly Vector3 Far = new Vector3(4000f, 0f, 4000f);

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

        private static GameObject Template(SO_ShipType type)
        {
            var root = new GameObject("Template");
            root.SetActive(false);
            root.transform.position = Far;
            var hull = new GameObject("Hull");
            hull.transform.SetParent(root.transform, false);
            hull.layer = LayerMask.NameToLayer("Ship");
            BoxCollider box = hull.AddComponent<BoxCollider>();
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(type, box, 1 << LayerMask.NameToLayer("Ship"), 0.5f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            return root;
        }

        [UnityTest]
        public IEnumerator Respawn_ASunkShip_ComesBackWholeAfterTheDelayWhenThePlayerIsClear()
        {
            var type = ScriptableObject.CreateInstance<SO_ShipType>();
            typeof(SO_ShipType).GetField("_sinkSeconds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(type, 0.2f);
            GameObject template = Template(type);
            var player = new GameObject("Player").transform;
            player.position = Far + Vector3.right * 50f;
            var respawn = new GameObject("Respawn").AddComponent<ShipRespawn>();
            respawn.Configure(template, player, Delay, Clearance);
            yield return null;

            Assert.AreEqual(1, respawn.Made, "a ship made at the start");
            GameObject first = respawn.Current;
            Assert.IsTrue(first.activeSelf);
            Assert.AreEqual(Far, first.transform.position);

            first.GetComponent<ShipDamage>().Hit(first.transform.position, 1f);
            for (float t = 0f; t < 0.5f + 2f * Delay; t += Time.captureDeltaTime)
            {
                yield return null;
            }
            Assert.IsFalse(first.activeSelf, "sunk");
            Assert.AreEqual(1, respawn.Made, "not while the player is within the clearance");

            player.position = Far + Vector3.right * (Clearance + 10f);
            yield return null;
            yield return null;

            Assert.AreEqual(2, respawn.Made, "a new ship once the player is clear");
            Assert.AreEqual(1f, respawn.Current.GetComponent<ShipDamage>().Health, "whole");
            Assert.IsTrue(first == null, "the sunk ship removed");

            Object.Destroy(respawn.gameObject);
            Object.Destroy(player.gameObject);
            Object.Destroy(template);
            Object.Destroy(type);
        }

        [UnityTest]
        public IEnumerator GameScene_CirclingBrig_KeepsItsCircleWithoutRunningAground()
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

            CircleHelm helm = Object.FindAnyObjectByType<CircleHelm>();
            Assert.IsNotNull(helm, "the Game scene has a ship sailing a circle");
            Transform brig = helm.transform;
            float sailed = 0f;
            Vector3 last = brig.position;
            float worst = 0f;
            for (float t = 0f; t < 60f; t += Time.captureDeltaTime)
            {
                yield return null;
                sailed += Vector3.Distance(brig.position, last);
                last = brig.position;
                if (t > 20f)
                {
                    Vector3 from = brig.position - helm.Centre;
                    worst = Mathf.Max(worst, Mathf.Abs(new Vector2(from.x, from.z).magnitude - helm.Radius));
                }
            }

            Assert.That(worst, Is.LessThan(6f), "on its circle after the first lap's approach");
            Assert.That(sailed, Is.GreaterThan(150f), "still sailing: it did not run aground");
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
