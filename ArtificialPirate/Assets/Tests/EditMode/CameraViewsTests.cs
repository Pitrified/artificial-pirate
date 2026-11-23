namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;
    using Game.Systems;

    /// <summary>
    /// <see cref="GameSettings"/> kept in <c>PlayerPrefs</c> (the keys saved and put back around each
    /// test), and the camera's views over a still ship: each view's pose from the type's rig, a
    /// change easing into the new view, the helm held fast, riding the hull's roll or keeping the
    /// horizon level.
    /// </summary>
    public sealed class CameraViewsTests
    {
        private const float Step = 1f / 60f;
        private static readonly CameraRig Chase = new CameraRig(new Vector3(0f, 10f, -30f), new Vector3(0f, 5f, 10f));
        private static readonly CameraRig BirdsEye = new CameraRig(new Vector3(0f, 45f, -22f), new Vector3(0f, 0f, 14f));
        private static readonly CameraRig Helm = new CameraRig(new Vector3(0f, 3.5f, -7f), new Vector3(0f, 3.5f, 53f));

        private readonly Dictionary<string, int> _saved = new Dictionary<string, int>();
        private readonly List<Object> _made = new List<Object>();
        private SO_CameraTuning _tuning;
        private SO_ShipType _type;
        private Transform _root;
        private Transform _hull;
        private CameraController _camera;

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
            _tuning = ScriptableObject.CreateInstance<SO_CameraTuning>();
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            _type.SetRigs(Chase, BirdsEye, Helm);
            _made.Add(_tuning);
            _made.Add(_type);
            var root = new GameObject("Ship");
            _made.Add(root);
            _root = root.transform;
            _root.SetPositionAndRotation(new Vector3(500f, 0f, -500f), Quaternion.Euler(0f, 30f, 0f));
            _hull = new GameObject("Hull").transform;
            _hull.SetParent(_root, false);
            var cameraObject = new GameObject("Camera");
            _made.Add(cameraObject);
            _camera = cameraObject.AddComponent<CameraController>();
            typeof(CameraController).GetField("_tuning", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_camera, _tuning);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object made in _made)
            {
                Object.DestroyImmediate(made);
            }
            _made.Clear();
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

        [Test]
        public void Settings_DefaultsThenKept_AndEachChangeRaised()
        {
            var first = new GameSettings();
            Assert.AreEqual(CameraView.Chase, first.Camera);
            Assert.IsFalse(first.HelmRides);
            Assert.AreEqual(PlayerShipKind.Sloop, first.Ship);
            int changes = 0;
            first.Changed += () => changes++;

            first.NextCamera();
            first.NextCamera();
            first.SetHelmRides(true);
            first.SetShip(PlayerShipKind.Brig);

            var second = new GameSettings();
            Assert.AreEqual(CameraView.Helm, second.Camera);
            Assert.IsTrue(second.HelmRides);
            Assert.AreEqual(PlayerShipKind.Brig, second.Ship);
            Assert.AreEqual(4, changes);
            second.NextCamera();
            Assert.AreEqual(CameraView.Chase, second.Camera, "from the last back to the first");
        }

        private void Settle(float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                _camera.Tick(Step);
            }
        }

        private void AssertAt(CameraRig rig, float tolerance)
        {
            Vector3 place = _root.TransformPoint(rig.Offset);
            Assert.That(Vector3.Distance(place, _camera.transform.position), Is.LessThan(tolerance), $"at the rig's place {place}");
            Vector3 look = (_root.TransformPoint(rig.LookAt) - place).normalized;
            Assert.That(Vector3.Angle(look, _camera.transform.forward), Is.LessThan(1f), "looking at the rig's point");
        }

        [TestCase(CameraView.Chase)]
        [TestCase(CameraView.BirdsEye)]
        [TestCase(CameraView.Helm)]
        public void SetShip_ThenSnap_StandsAtTheViewsRig(CameraView view)
        {
            _camera.SetShip(_root, _hull, _type, view, false);
            _camera.SnapToTarget();

            AssertAt(_type.Rig(view), 1e-3f);
        }

        [Test]
        public void SetView_EasesIntoTheNewView()
        {
            _camera.SetShip(_root, _hull, _type, CameraView.Chase, false);
            _camera.SnapToTarget();

            _camera.SetView(CameraView.BirdsEye, false);
            _camera.Tick(Step);
            Vector3 place = _root.TransformPoint(BirdsEye.Offset);
            Assert.That(Vector3.Distance(place, _camera.transform.position), Is.GreaterThan(5f), "not there at once");

            Settle(_tuning.SwitchSeconds + 0.1f);
            AssertAt(BirdsEye, 1e-2f);
        }

        [Test]
        public void Helm_Riding_RollsWithTheHull_Steady_KeepsTheHorizon()
        {
            _hull.localRotation = Quaternion.Euler(0f, 0f, 10f);
            _camera.SetShip(_root, _hull, _type, CameraView.Helm, false);
            _camera.SnapToTarget();
            Settle(0.1f);
            Assert.That(Vector3.Angle(_camera.transform.up, Vector3.up), Is.LessThan(5f), "steady: no roll");

            _camera.SetView(CameraView.Helm, true);
            Settle(_tuning.SwitchSeconds + 0.1f);
            Vector3 right = _camera.transform.right;
            Assert.That(Mathf.Abs(right.y), Is.GreaterThan(Mathf.Sin(8f * Mathf.Deg2Rad)), "riding: rolled with the hull");
        }
    }
}
