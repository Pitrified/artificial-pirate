namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="HullMarks"/> on a hull built in code: a hit leaves a quad on the side it landed on,
    /// on the hull's surface and facing out; a bigger hit a bigger mark; the marks follow the kept
    /// hits, the oldest going first; restored hits are drawn; on a tapering hull a mark near the bow
    /// sits on the narrower side and turns with it; on a hull with tumblehome a mark high up sits on
    /// the side fallen in.
    /// </summary>
    public sealed class HullMarksTests
    {
        private const float HalfLength = 10f;
        private const float HalfBeam = 3f;
        private const float Draft = 2.4f;
        private const float Freeboard = 2.5f;
        private const float StandOff = 0.04f;
        private const float Tolerance = 1e-3f;

        private readonly List<Object> _made = new List<Object>();
        private SO_ShipType _type;

        [SetUp]
        public void SetUp()
        {
            _type = ScriptableObject.CreateInstance<SO_ShipType>();
            _type.SetHull(HalfLength, HalfBeam, Draft, Freeboard);
            _made.Add(_type);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object made in _made)
            {
                Object.DestroyImmediate(made);
            }
            _made.Clear();
        }

        private (ShipDamage damage, HullMarks marks) Ship()
        {
            var root = new GameObject("Ship");
            _made.Add(root);
            var hull = new GameObject("Hull");
            hull.transform.SetParent(root.transform, false);
            BoxCollider box = hull.AddComponent<BoxCollider>();
            box.size = new Vector3(2f * HalfBeam, Draft + Freeboard, 2f * HalfLength);
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(_type, box, 0, 0f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            var marksObject = new GameObject("Marks", typeof(MeshFilter));
            marksObject.transform.SetParent(hull.transform, false);
            HullMarks marks = marksObject.AddComponent<HullMarks>();
            marks.Configure(damage);
            _made.Add(marks.Mesh);
            return (damage, marks);
        }

        private static Vector3 Centre(HullMarks marks, int quad)
        {
            var vertices = new List<Vector3>();
            marks.Mesh.GetVertices(vertices);
            Vector3 sum = Vector3.zero;
            for (int k = 0; k < 4; k++)
            {
                sum += vertices[quad * 4 + k];
            }
            return sum / 4f;
        }

        private static float Width(HullMarks marks, int quad)
        {
            var vertices = new List<Vector3>();
            marks.Mesh.GetVertices(vertices);
            return Vector3.Distance(vertices[quad * 4], vertices[quad * 4 + 3]);
        }

        private static Vector3 Facing(HullMarks marks, int quad)
        {
            var vertices = new List<Vector3>();
            marks.Mesh.GetVertices(vertices);
            Vector3 a = vertices[quad * 4];
            return Vector3.Cross(vertices[quad * 4 + 1] - a, vertices[quad * 4 + 2] - a).normalized;
        }

        [Test]
        public void Hit_OnStarboard_LeavesAMarkOnThatSide_FacingOut()
        {
            (ShipDamage damage, HullMarks marks) = Ship();

            damage.Hit(new Vector3(HalfBeam, 1f, 3f), 0.05f);

            Vector3 centre = Centre(marks, 0);
            Assert.AreEqual(HalfBeam + StandOff, centre.x, Tolerance, "on the side, just off it");
            Assert.AreEqual(3f, centre.z, Tolerance);
            Assert.That(centre.y, Is.InRange(0f, Freeboard), "between the waterline and the rail");
            Assert.That(Vector3.Dot(Facing(marks, 0), Vector3.right), Is.GreaterThan(0.99f), "facing out");
            Assert.AreEqual(Vector3.zero, Centre(marks, 1), "the rest folded away");
        }

        [Test]
        public void Hit_OnPort_FacesPort()
        {
            (ShipDamage damage, HullMarks marks) = Ship();

            damage.Hit(new Vector3(-HalfBeam, 0f, -2f), 0.05f);

            Assert.AreEqual(-HalfBeam - StandOff, Centre(marks, 0).x, Tolerance);
            Assert.That(Vector3.Dot(Facing(marks, 0), Vector3.left), Is.GreaterThan(0.99f));
        }

        [Test]
        public void BiggerHit_BiggerMark()
        {
            (ShipDamage damage, HullMarks marks) = Ship();

            damage.Hit(new Vector3(HalfBeam, 0f, -4f), 0.02f);
            damage.Hit(new Vector3(HalfBeam, 0f, 4f), 0.2f);

            Assert.That(Width(marks, 1), Is.GreaterThan(Width(marks, 0)));
        }

        [Test]
        public void Marks_FollowTheKeptHits_OldestGoingFirst()
        {
            (ShipDamage damage, HullMarks marks) = Ship();

            for (int i = 0; i <= ShipDamage.KeptHits; i++)
            {
                damage.Hit(new Vector3(HalfBeam, 0f, -8f + i * 0.5f), 0.01f);
            }

            Assert.AreEqual(-7.5f, Centre(marks, 0).z, Tolerance, "the first hit's mark is gone");
            Assert.AreEqual(-8f + ShipDamage.KeptHits * 0.5f, Centre(marks, ShipDamage.KeptHits - 1).z, Tolerance);
        }

        [Test]
        public void Restore_DrawsTheSavedHits()
        {
            (ShipDamage damage, HullMarks marks) = Ship();
            var saved = new DamageSaveData
            {
                lost = 0.1f,
                port = 0.1f,
                hits = new[] { new HitSaveData { x = -HalfBeam, y = 1f, z = 5f, amount = 0.1f } },
            };

            damage.Restore(saved);

            Assert.AreEqual(-HalfBeam - StandOff, Centre(marks, 0).x, Tolerance);
            Assert.AreEqual(5f, Centre(marks, 0).z, Tolerance);
        }

        [Test]
        public void TumblehomeHull_MarkHighOnTheSide_SitsOnTheFallenInSide()
        {
            const float Tumble = 0.2f;
            _type.SetTumblehome(Tumble);
            (ShipDamage damage, HullMarks marks) = Ship();

            damage.Hit(new Vector3(HalfBeam, Freeboard, 0f), 0.02f);

            float x = Centre(marks, 0).x - StandOff;
            Assert.That(x, Is.LessThan(HalfBeam), "the side falls in toward the rail");
            Assert.AreEqual(_type.SideAt(0f, Centre(marks, 0).y), x, Tolerance);
        }

        [Test]
        public void TaperingHull_MarkNearTheBow_SitsOnTheNarrowerSide_TurnedForward()
        {
            _type.SetBeamAlongLength(AnimationCurve.Linear(-1f, 1f, 1f, 0.2f));
            (ShipDamage damage, HullMarks marks) = Ship();
            const float NearBow = 7f;

            damage.Hit(new Vector3(HalfBeam, 1f, NearBow), 0.02f);

            Vector3 facing = Facing(marks, 0);
            Assert.That(Centre(marks, 0).x, Is.LessThan(HalfBeam * 0.6f), "the side is narrower near the bow");
            Assert.That(Centre(marks, 0).x, Is.EqualTo(_type.HalfBeamAt(NearBow)).Within(0.1f));
            Assert.That(facing.x, Is.GreaterThan(0f));
            Assert.That(facing.z, Is.GreaterThan(0f), "the side turns toward the bow");
        }
    }
}
