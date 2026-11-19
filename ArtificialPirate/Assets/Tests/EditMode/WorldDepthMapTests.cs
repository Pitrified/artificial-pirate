namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay.Water;
    using Game.Gameplay.World;

    /// <summary>
    /// <see cref="WorldDepthMap"/> and the water following the ship: a new world's map is drawn at
    /// once, shallow over the start island and deep in the open sea; sailing far fills a new map
    /// over several frames before it is shown; another seed is drawn at once; the water grid moves
    /// under its target in whole quads.
    /// </summary>
    public sealed class WorldDepthMapTests
    {
        private const float Size = 512f;
        private const int Resolution = 256;

        private SO_WaterSettings _waterSettings;
        private SO_WorldSettings _worldSettings;
        private GameObject _waterObject;
        private WaterSurface _water;
        private GameObject _worldObject;
        private WorldStreamer _streamer;
        private WorldDepthMap _map;
        private GameObject _target;

        [SetUp]
        public void SetUp()
        {
            _waterSettings = ScriptableObject.CreateInstance<SO_WaterSettings>();
            _worldSettings = ScriptableObject.CreateInstance<SO_WorldSettings>();
            _waterObject = new GameObject("Water");
            _water = _waterObject.AddComponent<WaterSurface>();
            _water.UseSettings(_waterSettings);
            _target = new GameObject("Ship");
            _worldObject = new GameObject("World");
            _streamer = _worldObject.AddComponent<WorldStreamer>();
            _streamer.Configure(_worldSettings, _target.transform, null, null, 1);
            _streamer.Begin(1);
            _map = _worldObject.AddComponent<WorldDepthMap>();
            _map.Configure(_water, _streamer, _target.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_map.Shown);
            Object.DestroyImmediate(_worldObject);
            Object.DestroyImmediate(_waterObject);
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_waterSettings);
            Object.DestroyImmediate(_worldSettings);
        }

        private float DepthAt(float x, float z)
        {
            Vector4 rect = _water.DepthRect;
            int column = Mathf.FloorToInt((x - rect.x) / rect.z * Resolution);
            int row = Mathf.FloorToInt((z - rect.y) / rect.z * Resolution);
            return _map.Shown.GetPixel(column, row).r;
        }

        [Test]
        public void NewWorld_IsDrawnAtOnce_ShallowOverLand_DeepInTheOpen()
        {
            _target.transform.position = new Vector3(0f, 0f, 60f);

            _map.Tick();

            Vector4 rect = _water.DepthRect;
            Assert.AreEqual(-Size / 2f, rect.x, 1e-3f);
            Assert.AreEqual(60f - Size / 2f, rect.y, 1e-3f);
            Assert.AreEqual(0f, DepthAt(0f, 0f), 1e-3f, "the start island");
            Assert.AreEqual(1f, DepthAt(0f, 200f), 1e-3f, "the open sea");
        }

        [Test]
        public void MapEdge_ReadsDeep_EvenOverLand()
        {
            _target.transform.position = new Vector3(Size / 2f, 0f, 0f); // the map's left edge on the start island

            _map.Tick();

            Assert.AreEqual(1f, _map.Shown.GetPixel(0, Resolution / 2).r, 1e-3f);
            Assert.AreEqual(0f, _map.Shown.GetPixel(2, Resolution / 2).r, 1e-3f, "land just inside the edge");
        }

        [Test]
        public void SailingFar_FillsANewMapOverFrames_ThenShowsIt()
        {
            _map.Tick();
            Vector4 before = _water.DepthRect;
            _target.transform.position = new Vector3(300f, 0f, 0f);

            _map.Tick();
            Assert.IsTrue(_map.Filling);
            Assert.AreEqual(before, _water.DepthRect, "the old map stays until the new one is whole");

            for (int i = 0; i < Resolution && _map.Filling; i++)
            {
                _map.Tick();
            }

            Assert.IsFalse(_map.Filling);
            Assert.AreEqual(300f - Size / 2f, _water.DepthRect.x, 1e-3f);
        }

        [Test]
        public void AnotherWorld_IsDrawnAtOnce()
        {
            _map.Tick();
            _target.transform.position = new Vector3(300f, 0f, 0f);
            _map.Tick();
            Assert.IsTrue(_map.Filling);

            _streamer.Begin(2);
            _map.Tick();

            Assert.IsFalse(_map.Filling);
            Assert.AreEqual(300f - Size / 2f, _water.DepthRect.x, 1e-3f);
        }

        [Test]
        public void Water_FollowsItsTarget_InWholeQuads()
        {
            _water.UseFollow(_target.transform, 2f);
            _target.transform.position = new Vector3(3.1f, 5f, -4.9f);

            _water.Follow();

            Assert.AreEqual(new Vector3(4f, 0f, -4f), _waterObject.transform.position);
        }
    }
}
