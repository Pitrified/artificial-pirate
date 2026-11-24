namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="ShipSails"/> on a two-masted ship built in code: the band shown for the wind off
    /// the bow on either tack, held within the hysteresis as the ship turns and changed beyond it;
    /// furled at anchor; a broken mast left alone; the flag streaming downwind whatever the heading.
    /// </summary>
    public sealed class ShipSailsTests
    {
        private const int Masts = 2;
        private const float Hysteresis = 8f;
        private const float Tolerance = 1e-3f;
        private static readonly float[] Bands = { 60f, 90f, 135f, 180f, -135f, -90f, -60f };

        private readonly List<Object> _made = new List<Object>();
        private SO_SailTrim _trim;

        [SetUp]
        public void SetUp()
        {
            _trim = ScriptableObject.CreateInstance<SO_SailTrim>();
            _trim.Configure(Bands, Hysteresis);
            _made.Add(_trim);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object made in _made)
            {
                if (made != null)
                {
                    Object.DestroyImmediate(made);
                }
            }
            _made.Clear();
        }

        private sealed class Rig
        {
            public ShipSails Sails;
            public ShipMasts Masts;
            public GameObject[] Anchored;
            public GameObject[][] Set;
            public Transform Flag;
        }

        private Rig Ship(bool set)
        {
            var root = new GameObject("Ship");
            _made.Add(root);
            var hull = new GameObject("Hull");
            hull.transform.SetParent(root.transform, false);
            var rig = new Rig { Anchored = new GameObject[Masts], Set = new GameObject[Masts][] };
            var masts = new ShipSails.MastSails[Masts];
            var parts = new MastParts[Masts];
            for (int i = 0; i < Masts; i++)
            {
                rig.Anchored[i] = Child(hull, $"Mast{i}Anchored");
                rig.Set[i] = new GameObject[Bands.Length];
                var all = new List<GameObject> { rig.Anchored[i] };
                for (int b = 0; b < Bands.Length; b++)
                {
                    rig.Set[i][b] = Child(hull, $"Mast{i}Sailing{b}");
                    all.Add(rig.Set[i][b]);
                }
                masts[i] = new ShipSails.MastSails(rig.Anchored[i], rig.Set[i]);
                parts[i] = new MastParts(0f, 0f, all.ToArray(), Child(hull, $"Mast{i}Stump"));
            }
            GameObject flag = Child(hull, "Flag");
            rig.Flag = flag.transform;
            rig.Masts = root.AddComponent<ShipMasts>();
            rig.Masts.Configure(null, parts, 0);
            rig.Sails = root.AddComponent<ShipSails>();
            rig.Sails.Configure(_trim, masts, rig.Masts, rig.Flag, 1, set);
            return rig;
        }

        private static GameObject Child(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void Head(Rig rig, float headingDeg, float windFromDeg)
        {
            rig.Sails.transform.rotation = Quaternion.Euler(0f, headingDeg, 0f);
            rig.Sails.UseWind(windFromDeg);
        }

        private static void AssertShows(Rig rig, int band)
        {
            Assert.AreEqual(band, rig.Sails.Band);
            for (int i = 0; i < Masts; i++)
            {
                Assert.IsFalse(rig.Anchored[i].activeSelf, $"mast {i} not furled");
                for (int b = 0; b < Bands.Length; b++)
                {
                    Assert.AreEqual(b == band, rig.Set[i][b].activeSelf, $"mast {i} band {b}");
                }
            }
        }

        [TestCase(0f, 90f, 1)]
        [TestCase(0f, 270f, 5)]
        [TestCase(90f, 270f, 3)]
        [TestCase(30f, 0f, 6)]
        [TestCase(330f, 0f, 0)]
        [TestCase(200f, 340f, 2)]
        public void Shows_TheBand_TheHeadingPutsTheWindIn(float heading, float windFrom, int band)
        {
            Rig rig = Ship(true);
            Head(rig, heading, windFrom);
            AssertShows(rig, band);
        }

        [Test]
        public void HoldsItsBand_WithinTheHysteresis_ThenChanges()
        {
            Rig rig = Ship(true);
            Head(rig, 0f, 90f);
            AssertShows(rig, 1);

            Head(rig, 0f, 115f);
            AssertShows(rig, 1);

            Head(rig, 0f, 120f);
            AssertShows(rig, 2);

            Head(rig, 0f, 110f);
            AssertShows(rig, 2);
        }

        [Test]
        public void AtAnchor_ShowsTheSailsFurled_ThenSetsThem()
        {
            Rig rig = Ship(false);
            Head(rig, 0f, 90f);
            Assert.AreEqual(-1, rig.Sails.Band);
            for (int i = 0; i < Masts; i++)
            {
                Assert.IsTrue(rig.Anchored[i].activeSelf);
                foreach (GameObject band in rig.Set[i])
                {
                    Assert.IsFalse(band.activeSelf);
                }
            }

            rig.Sails.SetSails(true);
            AssertShows(rig, 1);
        }

        [Test]
        public void ABrokenMast_IsLeftAlone()
        {
            Rig rig = Ship(true);
            Head(rig, 0f, 90f);
            rig.Masts.Restore(new[] { 0 });

            Head(rig, 0f, 180f);

            Assert.AreEqual(3, rig.Sails.Band);
            Assert.IsTrue(rig.Set[1][3].activeSelf, "the standing mast follows the wind");
            foreach (GameObject band in rig.Set[0])
            {
                Assert.IsFalse(band.activeSelf, "the broken mast's sails stay gone");
            }
        }

        [TestCase(0f, 90f)]
        [TestCase(45f, 90f)]
        [TestCase(200f, 30f)]
        public void Flag_StreamsDownwind_WhateverTheHeading(float heading, float windFrom)
        {
            Rig rig = Ship(true);
            Head(rig, heading, windFrom);

            Vector3 fly = rig.Flag.TransformDirection(Vector3.back);
            Vector3 downwind = Quaternion.Euler(0f, windFrom + 180f, 0f) * Vector3.forward;
            Assert.That(Vector3.Angle(fly, downwind), Is.LessThan(Tolerance + 0.01f));
        }
    }
}
