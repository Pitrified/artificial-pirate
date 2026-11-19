namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Game.Gameplay.World;
    using Game.UI;

    /// <summary>
    /// <see cref="WorldMap"/> and the HUD's <see cref="Minimap"/>: explored cells drawn by height
    /// and the others dark, new island tiles painted within a budget while open sea costs nothing,
    /// and the minimap drawing the cells around the ship, centring its window on it, turning its
    /// arrow and telling when it is tapped.
    /// </summary>
    public sealed class WorldMapTests
    {
        private const int Seed = 1;

        private SO_WorldSettings _worldSettings;
        private SO_MapStyle _style;
        private GameObject _target;
        private GameObject _worldObject;
        private WorldStreamer _streamer;
        private WorldMap _map;

        [SetUp]
        public void SetUp()
        {
            _worldSettings = ScriptableObject.CreateInstance<SO_WorldSettings>();
            _style = ScriptableObject.CreateInstance<SO_MapStyle>();
            _target = new GameObject("Ship");
            _worldObject = new GameObject("World");
            _streamer = _worldObject.AddComponent<WorldStreamer>();
            _streamer.Configure(_worldSettings, _target.transform, null, null, 1);
            _streamer.Begin(Seed);
            _map = _worldObject.AddComponent<WorldMap>();
            _map.Configure(_streamer, _style);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_worldObject);
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_worldSettings);
            Object.DestroyImmediate(_style);
        }

        private int Size => _style.TileSize;

        [Test]
        public void Compose_DrawsExploredCellsByHeight_AndTheRestDark()
        {
            _streamer.Explored.Add(new CellCoord(0, 0));
            var pixels = new Color32[2 * Size * Size];

            bool complete = _map.Compose(new CellCoord(0, 0), 2, 1, pixels, 1);

            Assert.IsTrue(complete);
            int centre = (Size / 2) * 2 * Size + Size / 2;
            Assert.AreEqual(_style.ColourAt(_streamer.World.HeightAt(0f, 0f)), pixels[centre], "the start island");
            Assert.AreEqual(_style.Unexplored, pixels[centre + Size], "the cell to the east, not explored");
        }

        [Test]
        public void Compose_PaintsNewIslandTilesWithinItsBudget()
        {
            _streamer.Explored.Add(new CellCoord(0, 0));
            var pixels = new Color32[Size * Size];

            Assert.IsFalse(_map.Compose(new CellCoord(0, 0), 1, 1, pixels, 0), "an island tile waits for budget");
            Assert.IsTrue(_map.Compose(new CellCoord(0, 0), 1, 1, pixels, 1));
            Assert.IsTrue(_map.Compose(new CellCoord(0, 0), 1, 1, pixels, 0), "painted once, kept");
        }

        [Test]
        public void ComposeExplored_FitsTheExploredCells_OrCapsAroundTheShip()
        {
            _streamer.Explored.Reveal(new CellCoord(0, 0), 1);
            _streamer.Explored.Add(new CellCoord(3, 0));

            Texture2D picture = _map.ComposeExplored(new CellCoord(0, 0), out Rect area);

            float cell = _worldSettings.CellSize;
            Assert.AreEqual(5 * Size, picture.width, "x from -1 to 3: five cells, a square");
            Assert.AreEqual(-1.5f * cell, area.x, 1e-3f);
            Assert.AreEqual(5f * cell, area.width, 1e-3f);
            Assert.AreEqual(-2.5f * cell, area.y, 1e-3f, "the three rows centred in five");

            _streamer.Explored.Add(new CellCoord(40, 0));
            picture = _map.ComposeExplored(new CellCoord(40, 0), out area);
            Assert.AreEqual(21 * Size, picture.width, "capped");
            Assert.AreEqual(30f * cell - cell / 2f, area.x, 1e-3f, "centred on the ship's cell");
        }

        [Test]
        public void Minimap_DrawsAroundTheShip_CentresOnIt_AndTurnsItsArrow()
        {
            var hud = new GameObject("HUD");
            try
            {
                RawImage image = new GameObject("Minimap").AddComponent<RawImage>();
                image.transform.SetParent(hud.transform);
                RawImage arrow = new GameObject("Ship").AddComponent<RawImage>();
                arrow.transform.SetParent(image.transform);
                Minimap minimap = image.gameObject.AddComponent<Minimap>();
                minimap.Configure(_map, _target.transform, image, arrow);
                _target.transform.SetPositionAndRotation(new Vector3(0f, 0f, 100f), Quaternion.Euler(0f, 90f, 0f));

                _streamer.Tick(); // marks the cells around the ship explored
                for (int i = 0; i < 20 && !minimap.Complete; i++)
                {
                    minimap.Tick();
                }

                Assert.IsTrue(minimap.Complete);
                Assert.AreSame(minimap.Texture, image.texture);
                Rect window = image.uvRect;
                float shipU = 0.5f + 0f;
                float shipV = 0.5f + 100f / (3f * _worldSettings.CellSize);
                Assert.AreEqual(shipU, window.center.x, 1e-4f, "the window centred on the ship");
                Assert.AreEqual(shipV, window.center.y, 1e-4f);
                Assert.AreEqual(-90f, Mathf.DeltaAngle(0f, arrow.rectTransform.localEulerAngles.z), 1e-3f, "heading east");

                bool clicked = false;
                minimap.Clicked += () => clicked = true;
                minimap.OnPointerClick(new PointerEventData(null));
                Assert.IsTrue(clicked);
            }
            finally
            {
                Object.DestroyImmediate(hud);
            }
        }
    }
}
