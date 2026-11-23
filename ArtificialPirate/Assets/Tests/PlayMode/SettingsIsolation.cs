namespace Game.Tests.PlayMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// Runs the PlayMode tests on the default settings whatever the editor's <c>PlayerPrefs</c> hold
    /// (a developer may have chosen the brig): the keys are cleared before the run and put back after.
    /// </summary>
    [SetUpFixture]
    public sealed class SettingsIsolation
    {
        private readonly Dictionary<string, int> _saved = new Dictionary<string, int>();

        [OneTimeSetUp]
        public void Clear()
        {
            foreach (string key in GameSettings.Keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    _saved[key] = PlayerPrefs.GetInt(key);
                }
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        [OneTimeTearDown]
        public void Restore()
        {
            foreach (string key in GameSettings.Keys)
            {
                PlayerPrefs.DeleteKey(key);
                if (_saved.TryGetValue(key, out int value))
                {
                    PlayerPrefs.SetInt(key, value);
                }
            }
            PlayerPrefs.Save();
        }
    }
}
