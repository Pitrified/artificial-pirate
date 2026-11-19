namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay.World;

    /// <summary>
    /// <see cref="WorldGenerator"/> with the default settings: a seed always gives the same world
    /// and another seed another one, islands reach no further than the next cell and are seabed
    /// outside their bounds, about the asked share of cells has one, and the start island is at
    /// the origin in every world, in open water, with the scene's props on land and in water where
    /// they belong.
    /// </summary>
    public sealed class WorldGeneratorTests
    {
        private const int Seed = 12345;
        private const int Span = 20; // cells each way from the origin
        private const float Tolerance = 1e-4f;

        private SO_WorldSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WorldSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void SameSeed_GivesTheSameWorld_InAnyOrder()
        {
            var first = new WorldGenerator(Seed, _settings);
            var second = new WorldGenerator(Seed, _settings);

            for (int x = Span; x >= -Span; x--)
            {
                for (int z = -Span; z <= Span; z++)
                {
                    var cell = new CellCoord(x, z);
                    bool a = first.TryGetIsland(cell, out IslandSite siteA);
                    bool b = second.TryGetIsland(new CellCoord(x, z), out IslandSite siteB);
                    Assert.AreEqual(a, b, $"cell {cell}");
                    Assert.AreEqual(siteA.Centre, siteB.Centre, $"cell {cell}");
                    Assert.AreEqual(siteA.Radius, siteB.Radius, $"cell {cell}");
                    Assert.AreEqual(siteA.Seed, siteB.Seed, $"cell {cell}");
                }
            }

            for (float x = -1000f; x < 1000f; x += 37f)
            {
                Assert.AreEqual(first.HeightAt(x, x * 0.7f), second.HeightAt(x, x * 0.7f));
            }
        }

        [Test]
        public void AnotherSeed_GivesAnotherWorld()
        {
            var first = new WorldGenerator(Seed, _settings);
            var other = new WorldGenerator(Seed + 1, _settings);

            int differing = 0;
            for (int x = -Span; x <= Span; x++)
            {
                var cell = new CellCoord(x, 3);
                bool a = first.TryGetIsland(cell, out IslandSite siteA);
                bool b = other.TryGetIsland(cell, out IslandSite siteB);
                if (a != b || siteA.Centre != siteB.Centre)
                {
                    differing++;
                }
            }

            Assert.That(differing, Is.GreaterThan(Span));
        }

        [Test]
        public void Islands_ReachNoFurtherThanTheNextCell()
        {
            var world = new WorldGenerator(Seed, _settings);
            float cell = _settings.CellSize;

            for (int x = -Span; x <= Span; x++)
            {
                for (int z = -Span; z <= Span; z++)
                {
                    var coord = new CellCoord(x, z);
                    if (!world.TryGetIsland(coord, out IslandSite site))
                    {
                        continue;
                    }
                    Assert.AreEqual(coord, world.CellAt(site.Centre.x, site.Centre.y), "centred in its own cell");
                    Vector2 centre = world.CellCentre(coord);
                    Rect bounds = site.Bounds;
                    Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(centre.x - 1.5f * cell - Tolerance), $"cell {coord}");
                    Assert.That(bounds.xMax, Is.LessThanOrEqualTo(centre.x + 1.5f * cell + Tolerance), $"cell {coord}");
                    Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(centre.y - 1.5f * cell - Tolerance), $"cell {coord}");
                    Assert.That(bounds.yMax, Is.LessThanOrEqualTo(centre.y + 1.5f * cell + Tolerance), $"cell {coord}");
                    if (x != 0 || z != 0) // the start island has its own size
                    {
                        Assert.That(site.Radius, Is.LessThanOrEqualTo(_settings.Radius.y), $"cell {coord}");
                        Assert.That(site.Peak, Is.InRange(_settings.Peak.x, _settings.Peak.y), $"cell {coord}");
                        Assert.That(site.Stretch, Is.InRange(1f, _settings.Stretch), $"cell {coord}");
                    }
                }
            }
        }

        [Test]
        public void OutsideItsBounds_AnIslandIsTheSeabed()
        {
            var world = new WorldGenerator(Seed, _settings);
            const int Around = 64;

            for (int x = -6; x <= 6; x++)
            {
                for (int z = -6; z <= 6; z++)
                {
                    if (!world.TryGetIsland(new CellCoord(x, z), out IslandSite site))
                    {
                        continue;
                    }
                    Rect b = site.Bounds;
                    for (int i = 0; i < Around; i++)
                    {
                        float t = (float)i / Around;
                        Assert.AreEqual(_settings.Seabed, world.IslandHeight(site, Mathf.Lerp(b.xMin, b.xMax, t), b.yMin), Tolerance);
                        Assert.AreEqual(_settings.Seabed, world.IslandHeight(site, Mathf.Lerp(b.xMin, b.xMax, t), b.yMax), Tolerance);
                        Assert.AreEqual(_settings.Seabed, world.IslandHeight(site, b.xMin, Mathf.Lerp(b.yMin, b.yMax, t)), Tolerance);
                        Assert.AreEqual(_settings.Seabed, world.IslandHeight(site, b.xMax, Mathf.Lerp(b.yMin, b.yMax, t)), Tolerance);
                    }
                }
            }
        }

        [Test]
        public void StartClearing_IsOpenWater([Values(1, 777, -42, 2024)] int seed)
        {
            var world = new WorldGenerator(seed, _settings);
            const int Around = 72;

            for (float r = 80f; r < _settings.StartClearing; r += 10f)
            {
                for (int i = 0; i < Around; i++)
                {
                    float angle = i * Mathf.PI * 2f / Around;
                    Assert.AreEqual(_settings.Seabed, world.HeightAt(r * Mathf.Cos(angle), r * Mathf.Sin(angle)), Tolerance, $"at {r} m");
                }
            }
        }

        [Test]
        public void IslandChance_IsTheShareOfCellsWithAnIsland()
        {
            var world = new WorldGenerator(Seed, _settings);
            int islands = 0;
            int cells = 0;
            for (int x = -Span; x <= Span; x++)
            {
                for (int z = -Span; z <= Span; z++)
                {
                    cells++;
                    if (world.TryGetIsland(new CellCoord(x, z), out _))
                    {
                        islands++;
                    }
                }
            }

            Assert.That((float)islands / cells, Is.EqualTo(_settings.IslandChance).Within(0.05f));
        }

        [Test]
        public void Heights_StayBetweenTheSeabedAndThePeak()
        {
            var world = new WorldGenerator(Seed, _settings);
            for (float x = -2000f; x < 2000f; x += 13f)
            {
                for (float z = -600f; z < 600f; z += 29f)
                {
                    float height = world.HeightAt(x, z);
                    Assert.That(height, Is.InRange(_settings.Seabed - Tolerance, _settings.Peak.y + Tolerance));
                }
            }
        }

        [Test]
        public void StartIsland_IsAtTheOrigin_WithThePropsWhereTheyBelong([Values(1, 777, -42)] int seed)
        {
            var world = new WorldGenerator(seed, _settings);

            Assert.IsTrue(world.TryGetIsland(new CellCoord(0, 0), out IslandSite site));
            Assert.AreEqual(Vector2.zero, site.Centre);
            Assert.That(world.HeightAt(0f, 0f), Is.GreaterThan(0f), "land at the origin");
            Assert.That(world.HeightAt(2f, 6.5f), Is.GreaterThan(0f), "the deck props stand on land");
            Assert.That(world.HeightAt(0f, 40f), Is.LessThan(-2f), "the floating props are in water");
            Assert.That(world.HeightAt(0f, 55f), Is.LessThan(-10f), "the sloop starts in deep water");
            Assert.That(world.HeightAt(-35f, 70f), Is.LessThan(-10f), "the brig lies in deep water");
        }
    }
}
