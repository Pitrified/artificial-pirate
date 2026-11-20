namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="ShipMasts"/> on a two-masted hull built in code: a hit brings down the standing
    /// mast nearest it (with the chance set to certain), its objects going over the side away from
    /// the blow and its stump showing; the chance rises as the health drops and is smaller for a
    /// small hit; each mast lost takes its share off the top speed, never below the floor; the
    /// broken masts come back from a save. <see cref="FallingMast"/> tips about its foot, then sinks.
    /// </summary>
    public sealed class ShipMastsTests
    {
        private const float HalfLength = 10f;
        private const float HalfBeam = 3f;
        private const float Draft = 2.4f;
        private const float Freeboard = 2.5f;
        private const float ForeZ = 5f;
        private const float MainZ = -4f;
        private const float FootY = 1.6f;
        private const float BreakingHit = 0.1f;
        private const float Tolerance = 1e-4f;

        private readonly List<Object> _made = new List<Object>();
        private SO_ShipType _type;

        [SetUp]
        public void SetUp()
        {
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            _type.SetHull(HalfLength, HalfBeam, Draft, Freeboard);
            _type.SetMastBreaking(1f, 1f, BreakingHit);
            _made.Add(_type);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (FallingMast fallen in Object.FindObjectsByType<FallingMast>(FindObjectsInactive.Include))
            {
                Object.DestroyImmediate(fallen.gameObject);
            }
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
            public ShipDamage Damage;
            public ShipMasts Masts;
            public Transform Hull;
            public GameObject[] Standing;
            public GameObject[] Stumps;
        }

        private Rig Ship()
        {
            var root = new GameObject("Ship");
            _made.Add(root);
            var hull = new GameObject("Hull");
            hull.transform.SetParent(root.transform, false);
            BoxCollider box = hull.AddComponent<BoxCollider>();
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(_type, box, 0, 0f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            ShipMasts masts = root.AddComponent<ShipMasts>();
            damage.Configure(contact, masts);

            var standing = new GameObject[2];
            var stumps = new GameObject[2];
            var parts = new MastParts[2];
            float[] along = { ForeZ, MainZ };
            for (int i = 0; i < 2; i++)
            {
                standing[i] = new GameObject($"Mast{i}");
                standing[i].transform.SetParent(hull.transform, false);
                stumps[i] = new GameObject($"Mast{i}Stump");
                stumps[i].transform.SetParent(hull.transform, false);
                stumps[i].SetActive(false);
                parts[i] = new MastParts(along[i], FootY, new[] { standing[i] }, stumps[i]);
            }
            masts.Configure(damage, parts, seed: 1);
            return new Rig { Damage = damage, Masts = masts, Hull = hull.transform, Standing = standing, Stumps = stumps };
        }

        private static FallingMast FallenOf(GameObject part) => part.GetComponentInParent<FallingMast>();

        [Test]
        public void Hit_BringsDownTheMastNearestIt()
        {
            Rig ship = Ship();

            ship.Damage.Hit(new Vector3(HalfBeam, 0f, MainZ + 1f), BreakingHit);

            Assert.IsTrue(ship.Masts.IsBroken(1), "the mainmast, nearest the hit");
            Assert.IsFalse(ship.Masts.IsBroken(0));
            Assert.AreEqual(1, ship.Masts.Lost);
            Assert.IsTrue(ship.Stumps[1].activeSelf, "its stump shows");
            Assert.AreNotEqual(ship.Hull, ship.Standing[1].transform.parent, "gone over the side");
            Assert.IsNotNull(FallenOf(ship.Standing[1]), "falling");
            Assert.AreEqual(ship.Hull, ship.Standing[0].transform.parent, "the other stands");
        }

        [Test]
        public void NextHit_TakesTheNextNearestStandingMast()
        {
            Rig ship = Ship();
            ship.Damage.Hit(new Vector3(HalfBeam, 0f, MainZ), BreakingHit);

            ship.Damage.Hit(new Vector3(HalfBeam, 0f, MainZ), BreakingHit);

            Assert.IsTrue(ship.Masts.IsBroken(0));
            Assert.AreEqual(2, ship.Masts.Lost);
        }

        [Test]
        public void BreakChance_RisesAsTheHealthDrops_LessForASmallHit()
        {
            _type.SetMastBreaking(0.05f, 0.5f, BreakingHit);
            Rig ship = Ship();

            float whole = ship.Masts.BreakChance(1f, BreakingHit);
            float half = ship.Masts.BreakChance(0.5f, BreakingHit);
            float ruined = ship.Masts.BreakChance(0f, BreakingHit);

            Assert.That(whole, Is.EqualTo(0.05f).Within(Tolerance));
            Assert.That(half, Is.GreaterThan(whole));
            Assert.That(ruined, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(ship.Masts.BreakChance(0.5f, BreakingHit * 0.25f), Is.EqualTo(half * 0.25f).Within(Tolerance));
            Assert.That(ship.Masts.BreakChance(0.5f, BreakingHit * 4f), Is.EqualTo(half).Within(Tolerance), "no more than the full chance");
        }

        [Test]
        public void Seeded_RollsMoreBreaksAsTheHealthDrops()
        {
            _type.SetMastBreaking(0.05f, 0.5f, BreakingHit);
            const int Trials = 200;

            int Breaks(float health)
            {
                int breaks = 0;
                for (int seed = 0; seed < Trials; seed++)
                {
                    Rig ship = Ship();
                    ship.Masts.Configure(ship.Damage, new[] { new MastParts(0f, FootY, new GameObject[0], null) }, seed);
                    float before = 1f - health - BreakingHit;
                    ship.Damage.Restore(new DamageSaveData { lost = before, starboard = before });
                    ship.Damage.Hit(Vector3.zero, BreakingHit);
                    breaks += ship.Masts.Lost;
                }
                return breaks;
            }

            Assert.That(Breaks(0.2f), Is.GreaterThan(Breaks(0.8f)));
        }

        [Test]
        public void MastsLost_TakeTheirShareOfTheSpeed_NeverBelowTheFloor()
        {
            Rig ship = Ship();
            float floor = _type.DamagedSpeedFloor;

            ship.Masts.Break(0, 1f);
            Assert.That(ship.Damage.SpeedFactor, Is.EqualTo(1f - (1f - floor) * 0.5f).Within(Tolerance), "one of two");

            ship.Masts.Break(1, 1f);
            Assert.That(ship.Damage.SpeedFactor, Is.EqualTo(floor).Within(Tolerance), "both: the floor");

            ship.Damage.Restore(new DamageSaveData { lost = 0.5f, starboard = 0.5f });
            Assert.That(ship.Damage.SpeedFactor, Is.EqualTo(floor).Within(Tolerance), "never below it");
        }

        [Test]
        public void Restore_BringsBackTheBrokenMasts()
        {
            Rig ship = Ship();
            ship.Masts.Break(0, -1f);
            DamageSaveData saved = ship.Damage.ToSaveData();
            CollectionAssert.AreEqual(new[] { 0 }, saved.brokenMasts);

            Rig restored = Ship();
            restored.Damage.Restore(saved);

            Assert.IsTrue(restored.Masts.IsBroken(0));
            Assert.IsFalse(restored.Masts.IsBroken(1));
            Assert.IsFalse(restored.Standing[0].activeSelf, "gone");
            Assert.IsTrue(restored.Stumps[0].activeSelf, "its stump");
            Assert.IsTrue(restored.Standing[1].activeSelf);
        }

        [Test]
        public void FallingMast_TipsAwayFromTheBlow_ThenSinks()
        {
            Rig ship = Ship();
            ship.Masts.Break(0, 1f);
            FallingMast fallen = FallenOf(ship.Standing[0]);
            Vector3 foot = fallen.transform.position;
            Assert.AreEqual(ship.Hull.TransformPoint(new Vector3(0f, FootY, ForeZ)), foot, "about its foot");

            fallen.Tick(_type.MastFallSeconds);

            Assert.AreEqual(_type.MastFallDeg, Vector3.Angle(Vector3.up, fallen.transform.up), 0.01f);
            Assert.That(fallen.transform.up.x, Is.LessThan(0f), "struck on starboard, it falls to port");
            Assert.AreEqual(foot.y, fallen.transform.position.y, Tolerance, "not sinking yet");

            fallen.Tick(1f);

            Assert.AreEqual(foot.y - _type.MastSinkSpeed, fallen.transform.position.y, Tolerance, "sinking");
            Assert.IsFalse(fallen.Gone);
            fallen.Tick(_type.MastSinkSeconds);
            Assert.IsTrue(fallen.Gone);
        }
    }
}
