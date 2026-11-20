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
    /// The map in the real scenes: tapping the minimap opens the map over the paused game, its
    /// Ship and Reset zoom buttons move the view, and its Close button resumes it.
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

            // The cells explored at the start are fewer than the view's, so it shows all of them.
            MapView view = ui.Map.View;
            Assert.AreEqual(1f, view.Share, 1e-4f);

            view.ZoomAbout(Vector2.zero, 4f, 100f);
            Assert.Greater(Vector2.Distance(view.Ship, view.Centre), 0.1f, "zoomed in about a corner, away from the ship");
            Tap(ui, "btn-map-ship");
            yield return null;
            Assert.AreEqual(view.Ship.x, view.Centre.x, 1e-4f, "Ship centres on the ship");
            Assert.AreEqual(view.Ship.y, view.Centre.y, 1e-4f);
            Assert.Less(view.Share, 1f, "and keeps the zoom");
            Tap(ui, "btn-map-zoom");
            yield return null;
            Assert.AreEqual(1f, view.Share, 1e-4f, "Reset zoom goes back to the whole map");

            Tap(ui, "btn-close-map");
            yield return null;

            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState);
            Assert.IsFalse(ui.IsAnyOverlayOpen, "The map is hidden");
        }

        private static void Tap(UISystem ui, string button)
        {
            Button target = ui.MapRoot.Q<Button>(button);
            Assert.IsNotNull(target, button);
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = target;
                target.SendEvent(submit);
            }
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
