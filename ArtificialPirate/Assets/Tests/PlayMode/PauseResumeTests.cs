// PlayMode test: pause → resume preserves ship transform.
//
// The ship spawns at zero speed and receives no input during the test.
// PauseGame() disables InputManager (returns default MovementInput = 0,0);
// with zero throttle and zero steering ShipController.Tick() produces no
// position or rotation change, so transform must be bitwise identical before
// and after the pause window.
//
// Run via Window > General > Test Runner > PlayMode.
namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay;
    using Game.UI;

    /// <summary>
    /// PlayMode tests verifying that the pause/resume cycle does not alter
    /// the ship's world transform.
    /// </summary>
    public sealed class PauseResumeTests
    {
        private const int WaitFrames   = 900;
        private const int PauseFrames  = 10;
        private const float Epsilon    = 0.001f;

        // ------------------------------------------------------------------ setup / teardown

        [SetUp]
        public void SetUp()
        {
            Time.captureDeltaTime = 0.016f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        /// <summary>Destroys the DontDestroyOnLoad GameManager so the next test boots fresh.</summary>
        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        // ------------------------------------------------------------------ tests

        /// <summary>
        /// Boots into the Game scene, pauses, advances <see cref="PauseFrames"/> frames,
        /// resumes, then asserts the ship's position and yaw are unchanged.
        /// Verifies that disabling input during pause is sufficient to keep the
        /// kinematic ship stationary.
        /// </summary>
        [UnityTest]
        public IEnumerator PauseResume_ShipTransformPreserved()
        {
            // ---- Arrange: boot to Playing ---- //

            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);

            var startTask = GameManager.Instance.StartNewGameAsync();
            while (!startTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Playing);

            // One extra frame so that Start() callbacks on Game scene objects have run
            // (GameSceneWiring.Start wires InputManager into the ship).
            yield return null;

            ShipController ship = GameManager.Instance.PlayerShip;
            Assert.IsNotNull(ship, "The Game scene registers the player's ship");
            Vector3 positionBefore = ship.transform.position;
            float   yawBefore      = ship.transform.eulerAngles.y;

            // ---- Act: pause, hold, resume ---- //

            GameManager.Instance.PauseGame();
            Assert.AreEqual(GameState.Paused, GameManager.Instance.CurrentState,
                "State must be Paused immediately after PauseGame()");

            for (int i = 0; i < PauseFrames; i++)
                yield return null;

            GameManager.Instance.ResumeGame();
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState,
                "State must be Playing immediately after ResumeGame()");

            // ---- Assert: transform unchanged ---- //

            float positionDelta = Vector3.Distance(ship.transform.position, positionBefore);
            float yawDelta      = Mathf.Abs(Mathf.DeltaAngle(ship.transform.eulerAngles.y, yawBefore));

            Assert.That(positionDelta, Is.LessThan(Epsilon),
                $"Ship position must be unchanged after pause/resume (delta={positionDelta:F6} m)");
            Assert.That(yawDelta, Is.LessThan(Epsilon),
                $"Ship yaw must be unchanged after pause/resume (delta={yawDelta:F6} deg)");
        }

        /// <summary>
        /// The HUD sorts above the screens shown over the paused game, so it shows only while
        /// playing: off when paused, on again after resuming.
        /// </summary>
        [UnityTest]
        public IEnumerator PauseResume_HudShownOnlyWhilePlaying()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            var startTask = GameManager.Instance.StartNewGameAsync();
            while (!startTask.IsCompleted)
                yield return null;
            yield return WaitForState(GameState.Playing);
            yield return null;

            Minimap minimap = Object.FindAnyObjectByType<Minimap>(FindObjectsInactive.Include);
            Assert.IsNotNull(minimap, "The HUD has a minimap");
            Assert.IsTrue(minimap.gameObject.activeInHierarchy, "The HUD shows while playing");

            GameManager.Instance.PauseGame();
            Assert.IsFalse(minimap.gameObject.activeInHierarchy, "The HUD is hidden while paused");

            GameManager.Instance.ResumeGame();
            Assert.IsTrue(minimap.gameObject.activeInHierarchy, "The HUD shows again after resuming");
        }

        // ------------------------------------------------------------------ helpers

        private static IEnumerator WaitForState(GameState target)
        {
            for (int frame = 0; frame < WaitFrames; frame++)
            {
                if (GameManager.Instance != null
                    && GameManager.Instance.CurrentState == target)
                    yield break;

                yield return null;
            }

            string actual = GameManager.Instance?.CurrentState.ToString()
                ?? "null (no GameManager instance)";
            Assert.Fail(
                $"Timed out waiting for GameState.{target} after {WaitFrames} frames. "
                + $"Actual state: {actual}");
        }
    }
}
