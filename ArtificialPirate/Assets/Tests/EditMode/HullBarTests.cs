namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Gameplay;
    using Game.UI;

    /// <summary>
    /// <see cref="HullBar"/>: its fill reaches as far as the hull's health and turns from the whole
    /// colour to the ruined one as the hull is damaged.
    /// </summary>
    public sealed class HullBarTests
    {
        private const float Tolerance = 1e-4f;

        private GameObject _ship;
        private GameObject _bar;
        private SO_ShipType _type;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ship);
            Object.DestroyImmediate(_bar);
            Object.DestroyImmediate(_type);
        }

        [Test]
        public void Fill_FollowsTheHealth_GreenToRed()
        {
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            _ship = new GameObject("Ship");
            var hull = new GameObject("Hull");
            hull.transform.SetParent(_ship.transform, false);
            HullContact contact = _ship.AddComponent<HullContact>();
            contact.Configure(_type, hull.AddComponent<BoxCollider>(), 0, 0f);
            ShipDamage damage = _ship.AddComponent<ShipDamage>();
            damage.Configure(contact);

            _bar = new GameObject("HullBar", typeof(RectTransform));
            Image fill = new GameObject("Fill", typeof(RectTransform)).AddComponent<Image>();
            fill.transform.SetParent(_bar.transform, false);
            HullBar bar = _bar.AddComponent<HullBar>();
            bar.Configure(fill);

            bar.Show(damage);
            Assert.AreEqual(1f, fill.rectTransform.anchorMax.x, Tolerance, "whole: full");
            Color whole = fill.color;

            damage.Hit(Vector3.zero, 0.75f);

            Assert.AreEqual(0.25f, fill.rectTransform.anchorMax.x, Tolerance, "a quarter left");
            Assert.That(fill.color.r, Is.GreaterThan(whole.r), "redder");
            Assert.That(fill.color.g, Is.LessThan(whole.g), "less green");
        }
    }
}
