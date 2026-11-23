namespace Game.Tests.EditMode
{
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="CircleHelm"/> steering a <see cref="ShipController"/> of the default type: from
    /// inside the circle and from outside it the ship comes onto the circle and keeps it, going round
    /// the way it was told; pushed off, it comes back.
    /// </summary>
    public sealed class CircleHelmTests
    {
        private const float Radius = 45f;
        private const float Step = 1f / 30f;
        private const float Band = 4f;
        private static readonly Vector3 Centre = new Vector3(2000f, 0f, -2000f);

        private GameObject _go;
        private SO_ShipType _type;
        private ShipController _ship;
        private CircleHelm _helm;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Ship");
            _ship = _go.AddComponent<ShipController>();
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            typeof(ShipController).GetField("_type", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_ship, _type);
            _helm = _go.AddComponent<CircleHelm>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_type);
        }

        private void Sail(float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                _helm.Tick();
                _ship.Tick(Step);
            }
        }

        private float FromCircle() => Mathf.Abs(Vector3.Distance(Flat(_go.transform.position), Flat(Centre)) - Radius);

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>The ship's angle round the centre, degrees, counter-clockwise from +x seen from above.</summary>
        private float Around()
        {
            Vector3 from = _go.transform.position - Centre;
            return Mathf.Atan2(from.z, from.x) * Mathf.Rad2Deg;
        }

        [TestCase(10f)]
        [TestCase(90f)]
        public void FromInsideOrOutside_ComesOntoTheCircleAndKeepsIt(float startDistance)
        {
            _go.transform.SetPositionAndRotation(Centre + Vector3.right * startDistance, Quaternion.identity);
            _helm.Configure(_ship, Centre, Radius, true, 0.5f);

            Sail(60f);
            float worst = 0f;
            for (int i = 0; i < 60; i++)
            {
                Sail(0.5f);
                worst = Mathf.Max(worst, FromCircle());
            }

            Assert.That(worst, Is.LessThan(Band), "within a few metres of the circle for half a minute");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GoesRoundTheWayItIsTold(bool toStarboard)
        {
            _go.transform.SetPositionAndRotation(Centre + Vector3.right * Radius, Quaternion.Euler(0f, toStarboard ? 180f : 0f, 0f));
            _helm.Configure(_ship, Centre, Radius, toStarboard, 0.5f);
            Sail(20f);
            float before = Around();

            Sail(2f);

            float turned = Mathf.DeltaAngle(before, Around());
            if (toStarboard)
            {
                Assert.That(turned, Is.LessThan(0f), "clockwise seen from above");
            }
            else
            {
                Assert.That(turned, Is.GreaterThan(0f), "counter-clockwise seen from above");
            }
        }

        [Test]
        public void PushedOff_ComesBack()
        {
            _go.transform.SetPositionAndRotation(Centre + Vector3.right * Radius, Quaternion.Euler(0f, 180f, 0f));
            _helm.Configure(_ship, Centre, Radius, true, 0.5f);
            Sail(30f);

            _go.transform.position += Flat(_go.transform.position - Centre).normalized * 25f;
            Sail(60f);

            Assert.That(FromCircle(), Is.LessThan(Band));
        }
    }
}
