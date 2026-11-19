namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;
    using Game.Gameplay.Water;

    /// <summary>
    /// <see cref="WaveRider"/> on water built in code with its clock pinned: level at the sea on
    /// flat water, nose up where the water rises toward the bow, no roll when the crests run across
    /// the ship, and eased rather than snapped after the first step. The root it rides on is never moved.
    /// </summary>
    public sealed class WaveRiderTests
    {
        private const float SeaLevel = -0.35f;
        private const float HalfLength = 1f;
        private const float HalfBeam = 0.5f;
        private const float Draft = 0.5f;
        private const float Freeboard = 0.5f;
        private const float Response = 4f;
        private const float Tolerance = 1e-3f;

        private GameObject _waterObject;
        private SO_WaterSettings _settings;
        private WaterSurface _water;
        private GameObject _ship;
        private Transform _hull;
        private WaveRider _rider;
        private SO_ShipType _type;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WaterSettings>();
            _waterObject = new GameObject("TestWater");
            _water = _waterObject.AddComponent<WaterSurface>();
            _ship = new GameObject("TestShip");
            _hull = new GameObject("Hull").transform;
            _hull.SetParent(_ship.transform, false);
            _rider = _ship.AddComponent<WaveRider>();
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            _type.SetHull(HalfLength, HalfBeam, Draft, Freeboard);
            _rider.Configure(_water, _hull, _type, Response);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ship);
            Object.DestroyImmediate(_waterObject);
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_type);
        }

        private void UseWaves(params GerstnerWave[] waves)
        {
            _settings.SetWaves(SeaLevel, waves);
            _water.UseSettings(_settings);
            _water.PinClock(1.3f);
        }

        private static GerstnerWave Wave(float directionDeg, float wavelength, float steepness) =>
            new GerstnerWave { directionDeg = directionDeg, wavelength = wavelength, steepness = steepness };

        [Test]
        public void FlatWater_HullLevelAtSeaLevel()
        {
            UseWaves();
            _ship.transform.SetPositionAndRotation(new Vector3(3f, 0f, -2f), Quaternion.Euler(0f, 40f, 0f));

            _rider.Tick(0.1f);

            Assert.AreEqual(SeaLevel, _hull.position.y, Tolerance);
            Assert.That(Quaternion.Angle(_hull.rotation, _ship.transform.rotation), Is.LessThan(0.01f), "level, heading with the ship");
            Assert.AreEqual(0f, _ship.transform.position.y, 0f, "the root is not moved");
        }

        [Test]
        public void WaterRisingTowardBow_PitchesNoseUp_NoRoll()
        {
            // One wave travelling along +x; the ship heads along +x, so crests run across it.
            UseWaves(Wave(0f, 20f, 0.3f));
            _ship.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            int checkedPoints = 0;
            for (int i = 0; i < 40; i++)
            {
                _ship.transform.position = new Vector3(i * 0.5f, 0f, 0f);
                _rider.Configure(_water, _hull, _type, Response); // each pose placed at once
                _rider.Tick(0.1f);

                Vector3 centre = _ship.transform.position;
                float bow = _water.HeightAt(centre + Vector3.right * HalfLength);
                float stern = _water.HeightAt(centre - Vector3.right * HalfLength);
                if (Mathf.Abs(bow - stern) < 0.05f)
                {
                    continue; // near a crest or a trough: too flat to call
                }
                checkedPoints++;
                Assert.AreEqual(bow > stern, _hull.forward.y > 0f, $"x {centre.x}: nose follows the slope");
                float expected = Mathf.Atan2(bow - stern, 2f * HalfLength);
                Assert.AreEqual(expected, Mathf.Asin(_hull.forward.y), 0.01f, $"x {centre.x}: pitch angle");
                Assert.AreEqual(0f, _hull.right.y, 0.01f, $"x {centre.x}: no roll");
                Assert.That(_hull.position.y, Is.InRange(Mathf.Min(bow, stern) - 0.2f, Mathf.Max(bow, stern) + 0.2f), "on the water");
            }
            Assert.That(checkedPoints, Is.GreaterThan(10), "enough sloped points checked");
        }

        [Test]
        public void CrestsAlongTheShip_Roll()
        {
            UseWaves(Wave(0f, 20f, 0.3f));
            _ship.transform.rotation = Quaternion.identity; // heading +z: the wave comes from port

            bool rolled = false;
            for (int i = 0; i < 20 && !rolled; i++)
            {
                _ship.transform.position = new Vector3(i * 0.5f, 0f, 0f);
                _rider.Configure(_water, _hull, _type, Response);
                _rider.Tick(0.1f);
                rolled = Mathf.Abs(_hull.right.y) > 0.05f;
                Assert.AreEqual(0f, _hull.forward.y, 0.01f, "no pitch");
            }
            Assert.IsTrue(rolled, "it rolls somewhere along the wave");
        }

        [Test]
        public void AfterTheFirstStep_EasesTowardTheWater()
        {
            UseWaves(Wave(0f, 20f, 0.3f));
            _rider.Tick(0.1f);
            float before = _hull.position.y;

            // The surface under the ship moves: a quarter period later.
            _water.PinClock(1.3f + 0.25f * Mathf.Sqrt(2f * Mathf.PI * 20f / 9.81f));
            // Heading +z at the origin: the mean of the water at bow, stern, starboard and port.
            float target = (_water.HeightAt(new Vector3(0f, 0f, HalfLength)) + _water.HeightAt(new Vector3(0f, 0f, -HalfLength))
                + _water.HeightAt(new Vector3(HalfBeam, 0f, 0f)) + _water.HeightAt(new Vector3(-HalfBeam, 0f, 0f))) * 0.25f;
            _rider.Tick(0.05f);
            float oneStep = _hull.position.y;
            for (int i = 0; i < 200; i++)
            {
                _rider.Tick(0.05f);
            }

            Assert.That(Mathf.Abs(oneStep - before), Is.LessThan(Mathf.Abs(target - before) * 0.5f), "one short step moves only part way");
            Assert.AreEqual(target, _hull.position.y, 0.05f, "and it gets there");
        }
    }
}
