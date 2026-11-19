namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay.World;

    /// <summary>
    /// <see cref="IslandMeshBuilder"/> and <see cref="WorldStreamer"/>: a mesh follows the height
    /// function, the streamer builds the islands around its target one a frame and drops the ones
    /// left behind when the target sails on, and it marks the cells around its target explored.
    /// </summary>
    public sealed class WorldStreamerTests
    {
        private const int Seed = 4321;
        private const int LoadRadius = 1;
        private const int Frames = 20;

        private SO_WorldSettings _settings;
        private GameObject _target;
        private GameObject _streamerObject;
        private WorldStreamer _streamer;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WorldSettings>();
            _target = new GameObject("Target");
            _streamerObject = new GameObject("World");
            _streamer = _streamerObject.AddComponent<WorldStreamer>();
            _streamer.Configure(_settings, _target.transform, null, null, LoadRadius);
            _streamer.Begin(Seed);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_streamerObject);
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_settings);
        }

        private int IslandsAround(CellCoord centre)
        {
            int count = 0;
            for (int dx = -LoadRadius; dx <= LoadRadius; dx++)
            {
                for (int dz = -LoadRadius; dz <= LoadRadius; dz++)
                {
                    if (_streamer.World.TryGetIsland(new CellCoord(centre.X + dx, centre.Z + dz), out _))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private void RunFrames()
        {
            for (int i = 0; i < Frames; i++)
            {
                _streamer.Tick();
            }
        }

        [Test]
        public void IslandMesh_FollowsTheHeightFunction()
        {
            var world = new WorldGenerator(Seed, _settings);
            Assert.IsTrue(world.TryGetIsland(new CellCoord(0, 0), out IslandSite site));
            var mesh = new Mesh();

            new IslandMeshBuilder().Build(world, site, mesh);

            Assert.AreEqual(2, mesh.subMeshCount);
            Assert.That(mesh.GetIndexCount(IslandMeshBuilder.GrassSubmesh), Is.GreaterThan(0), "it has land");
            Assert.That(mesh.triangles.Length / 3, Is.LessThanOrEqualTo(2 * IslandMeshBuilder.Quads * IslandMeshBuilder.Quads));
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i += 97)
            {
                Vector3 v = vertices[i];
                Assert.AreEqual(world.HeightAt(site.Centre.x + v.x, site.Centre.y + v.z), v.y, 1e-4f);
            }
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Streamer_BuildsTheIslandsAroundItsTarget_OneAFrame()
        {
            int expected = IslandsAround(new CellCoord(0, 0));

            _streamer.Tick();
            Assert.AreEqual(1, _streamer.LoadedCount, "one island the first frame");
            Assert.IsTrue(_streamer.IsLoaded(new CellCoord(0, 0)), "the nearest first: the start island");

            RunFrames();
            Assert.AreEqual(expected, _streamer.LoadedCount);
            Assert.AreEqual(0, _streamer.QueuedCount);
        }

        [Test]
        public void Streamer_DropsTheIslandsLeftBehind()
        {
            RunFrames();
            float far = _settings.CellSize * 5f;
            _target.transform.position = new Vector3(far, 0f, 0f);
            RunFrames();

            CellCoord centre = _streamer.World.CellAt(far, 0f);
            Assert.IsFalse(_streamer.IsLoaded(new CellCoord(0, 0)));
            Assert.AreEqual(IslandsAround(centre), _streamer.LoadedCount);
        }

        [Test]
        public void ExploredCells_RevealASquare_AndCountOnlyNewCells()
        {
            var explored = new ExploredCells();

            explored.Reveal(new CellCoord(0, 0), 1);
            Assert.AreEqual(9, explored.Count);
            Assert.AreEqual(1, explored.Version);

            explored.Reveal(new CellCoord(0, 0), 1);
            Assert.AreEqual(1, explored.Version, "nothing new, no new version");

            explored.Reveal(new CellCoord(1, 0), 1);
            Assert.AreEqual(12, explored.Count);
            Assert.IsTrue(explored.Contains(new CellCoord(2, -1)));
            Assert.IsFalse(explored.Contains(new CellCoord(3, 0)));
        }

        [Test]
        public void Streamer_MarksTheCellsAroundItsTargetExplored()
        {
            _streamer.Tick();
            Assert.AreEqual(9, _streamer.Explored.Count);

            _target.transform.position = new Vector3(_settings.CellSize * 2f, 0f, 0f);
            _streamer.Tick();
            Assert.AreEqual(15, _streamer.Explored.Count, "a column shared with the first square");
            Assert.IsTrue(_streamer.Explored.Contains(new CellCoord(3, 1)));
        }

        [Test]
        public void Begin_AnotherSeed_StartsAgain()
        {
            RunFrames();

            _streamer.Begin(Seed + 1);

            Assert.AreEqual(0, _streamer.LoadedCount);
            Assert.AreEqual(0, _streamer.Explored.Count);
            Assert.AreEqual(Seed + 1, _streamer.World.Seed);
        }
    }
}
