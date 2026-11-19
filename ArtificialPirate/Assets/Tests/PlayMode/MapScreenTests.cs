namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Game.Core;
    using Game.UI;

    /// <summary>
    /// The map in the real scenes: tapping the minimap opens the map over the paused game, and its
    /// Close button resumes it.
    /// </summary>
    public sealed class MapScreenTests
    {
        private const int WaitFrames = 900;

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator TappingTheMinimap_OpensTheMap_CloseResumes()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
                yield return null;
            yield return WaitForState(GameState.Playing);
            yield return null;

            Minimap minimap = Object.FindAnyObjectByType<Minimap>();
            Assert.IsNotNull(minimap, "The HUD has a minimap");
            minimap.OnPointerClick(new PointerEventData(EventSystem.current));

            Assert.AreEqual(GameState.Paused, GameManager.Instance.CurrentState);
            UISystem ui = Object.FindAnyObjectByType<UISystem>();
            Assert.IsTrue(ui.IsAnyOverlayOpen, "The map is shown");
            Assert.AreEqual(DisplayStyle.Flex, ui.MapRoot.style.display.value);

            Button close = ui.MapRoot.Q<Button>("btn-close-map");
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = close;
                close.SendEvent(submit);
            }
            yield return null;

            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState);
            Assert.IsFalse(ui.IsAnyOverlayOpen, "The map is hidden");
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
