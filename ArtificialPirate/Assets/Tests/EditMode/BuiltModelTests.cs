namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// Checks each model built by Tools/asset_builders as Unity imports it: no rotation or scale on
    /// any transform (the export bakes the axis change and applies the units) and its material slots
    /// mapped to the project materials (docs/building_assets.md, "The Unity side"). A prop has the
    /// size its builder's parameters give and its pivot at the centre of its base. A ship has its
    /// pivot on the waterline, matches its <see cref="SO_ShipType"/> there (its size and its side
    /// along the length) and at the keel, faces +Z, has each mast's objects: the mast, its two
    /// sail states and its stump; and a barrel object for each of its type's guns, a barrel's mouth
    /// at each muzzle. A new model adds a case.
    /// </summary>
    public sealed class BuiltModelTests
    {
        private const float SizeTolerance = 0.02f;
        private const float PivotTolerance = 0.005f;
        private const float HullTolerance = 0.05f;
        private const float WaterlineBand = 0.005f;
        private const float MuzzleTolerance = 0.3f; // the mouth's rim is the barrel's radius from its centre

        private static readonly string[] ShipMaterials =
        {
            "Assets/Art/M_HullBottom.mat", "Assets/Art/M_Topsides.mat", "Assets/Art/M_Trim.mat",
            "Assets/Art/M_ShipWood.mat", "Assets/Art/M_SailCloth.mat", "Assets/Art/M_Tar.mat",
            "Assets/Art/M_Deck.mat", "Assets/Art/M_Gilt.mat", "Assets/Art/M_Palette.mat", "Assets/Art/M_Flag.mat",
            "Assets/Art/M_FurledCloth.mat", "Assets/Art/M_Spar.mat", "Assets/Art/M_LanternGlass.mat",
        };

        /// <summary>A built model and what its builder's PARAMS say it should be.</summary>
        public sealed class Expected
        {
            /// <summary>The FBX's asset path.</summary>
            public string Path;

            /// <summary>Bounds size in Unity axes: X width, Y height, Z depth, in metres.</summary>
            public Vector3 Size;

            /// <summary>The project materials its slots map to.</summary>
            public string[] Materials;

            /// <summary>A ship's type asset; null for a prop.</summary>
            public string ShipType;

            /// <summary>A ship's masts, as its builder's PARAMS list them.</summary>
            public int Masts;

            /// <summary>The model's name, for the test runner.</summary>
            public override string ToString() => System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        private static IEnumerable<Expected> Models() => Props().Concat(Ships());

        private static IEnumerable<Expected> Ships()
        {
            yield return new Expected
            {
                Path = "Assets/Art/Models/Sloop.fbx",
                Materials = ShipMaterials,
                ShipType = "Assets/Settings/SO_Ship_Sloop.asset",
                Masts = 1,
            };
            yield return new Expected
            {
                Path = "Assets/Art/Models/Brig.fbx",
                Materials = ShipMaterials,
                ShipType = "Assets/Settings/SO_Ship_Brig.asset",
                Masts = 2,
            };
        }

        private static IEnumerable<Expected> Props()
        {
            // barrel.py: 0.72 m at the bilge (hoops and bung stand a little off it), 0.95 m tall.
            yield return new Expected
            {
                Path = "Assets/Art/Models/Barrel.fbx",
                Size = new Vector3(0.72f, 0.95f, 0.72f),
                Materials = new[] { "Assets/Art/M_Wood.mat", "Assets/Art/M_Iron.mat", "Assets/Art/M_Cork.mat" },
            };
            // crate.py: 0.27 x 0.265 m with the lid's overhangs (0.005 m a side, 0.01 m front and back), 0.135 m tall.
            yield return new Expected
            {
                Path = "Assets/Art/Models/Crate.fbx",
                Size = new Vector3(0.28f, 0.135f, 0.285f),
                Materials = new[] { "Assets/Art/M_CrateWood.mat" },
            };
        }

        [TestCaseSource(nameof(Props))]
        public void Size_MatchesBuilderDimensions(Expected model)
        {
            Bounds bounds = ModelBounds(Load(model), out _);
            Assert.AreEqual(model.Size.x, bounds.size.x, SizeTolerance, "Width along X");
            Assert.AreEqual(model.Size.y, bounds.size.y, SizeTolerance, "Height along Unity's Y axis");
            Assert.AreEqual(model.Size.z, bounds.size.z, SizeTolerance, "Depth along Z");
        }

        [TestCaseSource(nameof(Props))]
        public void Pivot_IsAtBaseCentre(Expected model)
        {
            Bounds bounds = ModelBounds(Load(model), out List<Vector3> points);
            Assert.AreEqual(0f, bounds.min.y, PivotTolerance, "The base sits at y 0");

            // The centre of the base, not of the bounds: a feature on one side (a bung) moves the bounds.
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (Vector3 p in points)
            {
                if (p.y < PivotTolerance)
                {
                    sum += p;
                    count++;
                }
            }
            Assert.That(count, Is.GreaterThan(0), "The model has vertices at its base");
            Vector3 centre = sum / count;
            Assert.AreEqual(0f, centre.x, PivotTolerance, "The base is centred on X");
            Assert.AreEqual(0f, centre.z, PivotTolerance, "The base is centred on Z");
        }

        [TestCaseSource(nameof(Models))]
        public void Transforms_AreUnrotatedAtScaleOne(Expected model)
        {
            foreach (Transform t in Load(model).GetComponentsInChildren<Transform>())
            {
                Assert.That(Quaternion.Angle(Quaternion.identity, t.localRotation), Is.LessThan(0.01f),
                    $"'{t.name}' must not carry a rotation");
                Assert.That((t.localScale - Vector3.one).magnitude, Is.LessThan(0.0001f),
                    $"'{t.name}' must be at scale 1");
            }
        }

        [TestCaseSource(nameof(Models))]
        public void Materials_AreMappedToProjectMaterials(Expected model)
        {
            var paths = new HashSet<string>();
            foreach (MeshRenderer renderer in Load(model).GetComponentsInChildren<MeshRenderer>(true))
            {
                paths.UnionWith(System.Array.ConvertAll(renderer.sharedMaterials, AssetDatabase.GetAssetPath));
            }
            CollectionAssert.AreEquivalent(model.Materials, paths);
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_MatchesItsTypeAtTheWaterline(Expected model)
        {
            SO_ShipType type = LoadType(model);
            ModelBounds(Load(model), out List<Vector3> points);
            float halfLength = 0f;
            float halfBeam = 0f;
            foreach (Vector3 p in points)
            {
                if (Mathf.Abs(p.y) < WaterlineBand)
                {
                    halfLength = Mathf.Max(halfLength, Mathf.Abs(p.z));
                    halfBeam = Mathf.Max(halfBeam, Mathf.Abs(p.x));
                }
            }
            Assert.AreEqual(type.HalfLength, halfLength, HullTolerance, "Half length at the waterline");
            Assert.AreEqual(type.HalfBeam, halfBeam, HullTolerance, "Half beam at the waterline");
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_SideAlongTheLength_MatchesItsType(Expected model)
        {
            SO_ShipType type = LoadType(model);
            ModelBounds(Load(model), out List<Vector3> points);
            var widest = new Dictionary<float, float>();
            foreach (Vector3 p in points)
            {
                if (Mathf.Abs(p.y) < WaterlineBand && Mathf.Abs(p.z) < type.HalfLength)
                {
                    float station = Mathf.Round(p.z * 100f) / 100f;
                    widest[station] = Mathf.Max(widest.TryGetValue(station, out float x) ? x : 0f, Mathf.Abs(p.x));
                }
            }
            Assert.That(widest.Count, Is.GreaterThan(10), "The hull's stations at the waterline");
            foreach (KeyValuePair<float, float> station in widest)
            {
                Assert.AreEqual(type.HalfBeamAt(station.Key), station.Value, HullTolerance, $"Half beam at z {station.Key}");
            }
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_HasEachMastsObjects(Expected model)
        {
            GameObject root = Load(model);
            string name = root.name;
            for (int i = 0; i < model.Masts; i++)
            {
                foreach (string part in new[] { "", "Sailing", "Anchored", "Stump" })
                {
                    Assert.IsNotNull(root.transform.Find($"{name}Mast{i}{part}"), $"{name}Mast{i}{part}");
                }
            }
            Assert.IsNull(root.transform.Find($"{name}Mast{model.Masts}"), "No more masts");
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_GunsAreAtTheBarrelsMouths(Expected model)
        {
            SO_ShipType type = LoadType(model);
            ModelBounds(Load(model), out List<Vector3> points);
            Assert.That(type.GunsPerSide, Is.GreaterThan(0), "the type lists its guns");
            for (int i = 0; i < type.GunsPerSide; i++)
            {
                foreach (bool starboard in new[] { true, false })
                {
                    Vector3 muzzle = type.Gun(i, starboard);
                    float nearest = points.Min(p => Vector3.Distance(p, muzzle));
                    Assert.That(nearest, Is.LessThan(MuzzleTolerance), $"a barrel's mouth at gun {i} {(starboard ? "starboard" : "port")} {muzzle}");
                }
            }
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_HasABarrelForEachGun_OnItsSide(Expected model)
        {
            GameObject root = Load(model);
            int guns = LoadType(model).GunsPerSide;
            foreach (string side in new[] { "Starboard", "Port" })
            {
                for (int i = 0; i < guns; i++)
                {
                    Transform barrel = root.transform.Find($"{root.name}Gun{side}{i}");
                    Assert.IsNotNull(barrel, $"{root.name}Gun{side}{i}");
                    Assert.AreEqual(side == "Starboard", barrel.localPosition.x > 0f, "turning about a point on its own side");
                }
                Assert.IsNull(root.transform.Find($"{root.name}Gun{side}{guns}"), "No more barrels");
            }
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_HasAMapIcon_BowUp(Expected model)
        {
            Texture2D icon = LoadType(model).MapIcon;
            Assert.IsNotNull(icon, "the type's map icon (textures.py map_icons)");
            int Width(int row)
            {
                int filled = 0;
                for (int x = 0; x < icon.width; x++)
                {
                    filled += icon.GetPixel(x, row).a > 0.5f ? 1 : 0;
                }
                return filled;
            }
            int middle = Width(icon.height / 2);
            Assert.That(middle, Is.GreaterThan(icon.width / 6), "the hull's beam amidships");
            Assert.That(Width(icon.height * 9 / 10), Is.LessThan(middle / 2), "narrow at the top: the bow up");
            Assert.That(Width(icon.height / 20), Is.GreaterThan(middle / 3), "the transom at the bottom");
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_KeelIsAtItsDraft(Expected model)
        {
            Bounds bounds = ModelBounds(Load(model), out _);
            Assert.AreEqual(-LoadType(model).Draft, bounds.min.y, HullTolerance);
        }

        [TestCaseSource(nameof(Ships))]
        public void Ship_BowFacesPlusZ(Expected model)
        {
            // The bowsprit reaches farther ahead than the boom does astern.
            Bounds bounds = ModelBounds(Load(model), out _);
            Assert.That(bounds.max.z, Is.GreaterThan(-bounds.min.z));
        }

        private static GameObject Load(Expected model) => LoadModel(model.Path);

        private static SO_ShipType LoadType(Expected model)
        {
            var type = AssetDatabase.LoadAssetAtPath<SO_ShipType>(model.ShipType);
            Assert.IsNotNull(type, $"{model.ShipType} must exist");
            return type;
        }

        /// <summary>The imported model at <paramref name="path"/>, failing the test when it is missing.</summary>
        internal static GameObject LoadModel(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(go, $"{path} must exist; build it with Tools/asset_builders");
            return go;
        }

        /// <summary>The bounds of every vertex of the model, in its root's space; the vertices too.</summary>
        internal static Bounds ModelBounds(GameObject root, out List<Vector3> points)
        {
            points = new List<Vector3>();
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Matrix4x4 toModel = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                foreach (Vector3 v in filter.sharedMesh.vertices)
                {
                    points.Add(toModel.MultiplyPoint3x4(v));
                }
            }
            Assert.That(points.Count, Is.GreaterThan(0), $"'{root.name}' has a mesh");
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (Vector3 p in points)
            {
                bounds.Encapsulate(p);
            }
            return bounds;
        }
    }
}
