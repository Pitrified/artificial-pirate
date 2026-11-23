namespace Game.Tests.PlayMode
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Game.Core;
    using Game.Gameplay;
    using Game.Systems;
    using Game.UI;

    /// <summary>
    /// The settings screen from the menu: its camera buttons set the camera, Back hides it; the game
    /// then opens on that camera, and the HUD's camera button moves on to the next. The settings'
    /// keys are saved and put back around the test.
    /// </summary>
    public sealed class SettingsScreenTests
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
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        private static void Press(VisualElement root, string name)
        {
            Button button = root.Q<Button>(name);
            Assert.IsNotNull(button, name);
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = button;
                button.SendEvent(submit);
            }
        }

        [UnityTest]
        public IEnumerator Settings_SetTheCamera_TheGameOpensOnIt_TheHudButtonCyclesIt()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);
            GameManager gm = GameManager.Instance;
            UISystem ui = Object.FindAnyObjectByType<UISystem>();

            gm.UISystem.ShowScreen(ScreenId.Settings);
            Assert.AreEqual(DisplayStyle.Flex, ui.SettingsRoot.style.display.value);
            Press(ui.SettingsRoot, "btn-camera-helm");
            Assert.AreEqual(CameraView.Helm, gm.Settings.Camera);
            Assert.IsTrue(ui.SettingsRoot.Q<Button>("btn-camera-helm").ClassListContains("settings-btn--on"), "the chosen button lit");
            Press(ui.SettingsRoot, "btn-settings-close");
            Assert.AreEqual(DisplayStyle.None, ui.SettingsRoot.style.display.value);

            var start = gm.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            yield return WaitForState(GameState.Playing);
            yield return null;

            CameraController camera = Object.FindAnyObjectByType<CameraController>();
            Assert.AreEqual(CameraView.Helm, camera.View);
            GameObject button = GameObject.Find("BtnCamera");
            Assert.IsNotNull(button, "the HUD has a camera button");
            button.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.AreEqual(CameraView.Chase, camera.View, "from the last back to the first");
            Assert.AreEqual(CameraView.Chase, gm.Settings.Camera, "and remembered");
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
