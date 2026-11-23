namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Gameplay;
    using Game.UI;

    /// <summary>
    /// <see cref="ShipBars"/> over ships built in code, seen by a camera: a bar for each other ship
    /// within reach, none for the player's own, one beyond reach or behind the camera; red for
    /// another side, green for the player's; the fill at the health. And the camera's shake:
    /// an offset that dies away to nothing, leaving the camera where it follows.
    /// </summary>
    public sealed class ShipBarsTests
    {
        private const float Reach = 150f;
        private static readonly Vector3 Far = new Vector3(-4000f, 0f, -4000f);

        private readonly List<Object> _made = new List<Object>();
        private LayerMask _ships;
        private Camera _camera;
        private ShipBars _bars;

        [SetUp]
        public void SetUp()
        {
            _ships = 1 << LayerMask.NameToLayer("Ship");
            var cameraObject = Make("Camera");
            cameraObject.transform.SetPositionAndRotation(Far + new Vector3(0f, 20f, -60f), Quaternion.Euler(10f, 0f, 0f));
            _camera = cameraObject.AddComponent<Camera>();

            var area = new GameObject("Bars", typeof(RectTransform));
            _made.Add(area);
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image));
            template.transform.SetParent(area.transform, false);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(template.transform, false);
            _bars = area.AddComponent<ShipBars>();
            _bars.Configure((RectTransform)template.transform, 4, Reach);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object made in _made)
            {
                Object.DestroyImmediate(made);
            }
            _made.Clear();
        }

        private GameObject Make(string name)
        {
            var go = new GameObject(name);
            _made.Add(go);
            return go;
        }

        private Broadside Ship(string name, ShipSide side, Vector3 offset)
        {
            var type = ScriptableObject.CreateInstance<SO_ShipType>();
            type.SetHull(10f, 3f, 2f, 2f);
            _made.Add(type);
            GameObject root = Make(name);
            root.transform.position = Far + offset;
            var hull = new GameObject("Hull");
            hull.layer = LayerMask.NameToLayer("Ship");
            hull.transform.SetParent(root.transform, false);
            BoxCollider box = hull.AddComponent<BoxCollider>();
            box.size = new Vector3(6f, 4f, 20f);
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(type, box, _ships, 0.5f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            Broadside broadside = root.AddComponent<Broadside>();
            broadside.Configure(side, null, contact, damage, null, 1);
            return broadside;
        }

        [Test]
        public void Bars_OverOtherShipsInReach_ColouredBySide_FilledToHealth()
        {
            Broadside player = Ship("Player", ShipSide.Player, Vector3.zero);
            Broadside enemy = Ship("Enemy", ShipSide.Pirates, new Vector3(-40f, 0f, 60f));
            Ship("Ally", ShipSide.Player, new Vector3(40f, 0f, 60f));
            Ship("Beyond", ShipSide.Pirates, new Vector3(0f, 0f, Reach + 40f));
            Ship("Behind", ShipSide.Pirates, new Vector3(0f, 0f, -100f));
            enemy.GetComponent<ShipDamage>().Hit(enemy.transform.position, 0.4f);
            Physics.SyncTransforms();
            _bars.Show(player, _camera);

            _bars.Tick();

            Assert.AreEqual(2, _bars.Showing, "the enemy and the ally; not ours, not beyond reach, not behind the camera");
            var colours = new List<Color> { _bars.FillOf(0).color, _bars.FillOf(1).color };
            Assert.That(colours.Exists(c => c.r > c.g), "a red bar for the enemy");
            Assert.That(colours.Exists(c => c.g > c.r), "a green bar for the ally");
            int enemyBar = _bars.FillOf(0).color.r > _bars.FillOf(0).color.g ? 0 : 1;
            Assert.AreEqual(0.6f, _bars.FillOf(enemyBar).rectTransform.anchorMax.x, 1e-4f);
            Assert.IsFalse(_bars.BarOf(2).gameObject.activeSelf);
        }

        [Test]
        public void Shake_DiesAway_LeavingTheCameraWhereItFollows()
        {
            var tuning = ScriptableObject.CreateInstance<SO_CameraTuning>();
            _made.Add(tuning);
            GameObject target = Make("Target");
            target.transform.position = Far;
            CameraController follow = _camera.gameObject.AddComponent<CameraController>();
            typeof(CameraController).GetField("_tuning", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(follow, tuning);
            follow.SetTarget(target.transform, new Vector3(0f, 3f, -6f), Vector3.zero);
            follow.SnapToTarget();
            Vector3 rest = _camera.transform.position;

            follow.Shake(10f);
            follow.Tick(1f / 60f);

            Assert.That(follow.ShakeOffset.magnitude, Is.GreaterThan(0f), "shaking");
            Assert.That(Mathf.Abs(follow.ShakeOffset.x), Is.LessThanOrEqualTo(tuning.ShakeMost), "never past the most");
            for (int i = 0; i < 180; i++)
            {
                follow.Tick(1f / 60f);
            }
            Assert.AreEqual(Vector3.zero, follow.ShakeOffset);
            Assert.That(Vector3.Distance(rest, _camera.transform.position), Is.LessThan(1e-3f));
        }
    }
}
