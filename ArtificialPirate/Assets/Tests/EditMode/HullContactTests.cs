namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="HullContact"/> with hulls built in code on the Ship layer: a ram is shared by mass
    /// and both hulls end apart, a hull that cannot move stops it, a glancing touch and backing away
    /// keep their speed, a crate on another layer is ignored, and a drift fades to rest.
    /// </summary>
    public sealed class HullContactTests
    {
        private const float HalfLength = 10f;
        private const float HalfBeam = 3f;
        private const float Draft = 2.4f;
        private const float Freeboard = 1.5f;
        private const float DriftDamping = 0.5f;
        private const float Overlap = 0.5f;
        private const float Speed = 6f;
        private const float Tolerance = 1e-3f;

        private readonly List<Object> _made = new List<Object>();
        private LayerMask _ships;

        [SetUp]
        public void SetUp()
        {
            int layer = LayerMask.NameToLayer("Ship");
            Assert.That(layer, Is.GreaterThanOrEqualTo(0), "the project has a Ship layer");
            _ships = 1 << layer;
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

        private SO_ShipType Type(float scale)
        {
            var type = ScriptableObject.CreateInstance<SO_ShipType>();
            type.SetHull(HalfLength * scale, HalfBeam * scale, Draft * scale, Freeboard * scale);
            _made.Add(type);
            return type;
        }

        private BoxCollider Hull(string name, SO_ShipType type, Vector3 position, int layer)
        {
            var root = new GameObject(name);
            _made.Add(root);
            root.transform.position = position;
            var hull = new GameObject("Hull");
            hull.layer = layer;
            hull.transform.SetParent(root.transform, false);
            hull.transform.localScale = new Vector3(2f * type.HalfBeam, type.Draft + type.Freeboard, 2f * type.HalfLength);
            return hull.AddComponent<BoxCollider>();
        }

        private HullContact Ship(string name, SO_ShipType type, Vector3 position)
        {
            BoxCollider hull = Hull(name, type, position, LayerMask.NameToLayer("Ship"));
            HullContact contact = hull.transform.parent.gameObject.AddComponent<HullContact>();
            contact.Configure(type, hull, _ships, DriftDamping);
            return contact;
        }

        /// <summary>The rammer's bow overlaps the other's stern by <see cref="Overlap"/>, both heading +z.</summary>
        private (HullContact rammer, HullContact rammed) Astern(float rammedScale)
        {
            SO_ShipType rammerType = Type(1f);
            SO_ShipType rammedType = Type(rammedScale);
            HullContact rammer = Ship("Rammer", rammerType, Vector3.zero);
            float gap = rammerType.HalfLength + rammedType.HalfLength - Overlap;
            HullContact rammed = Ship("Rammed", rammedType, new Vector3(0f, 0f, gap));
            Physics.SyncTransforms();
            return (rammer, rammed);
        }

        private static float Gap(HullContact a, HullContact b)
        {
            return Mathf.Abs(b.transform.position.z - a.transform.position.z);
        }

        [Test]
        public void Ram_EqualShips_ShareTheSpeedAndEndApart()
        {
            (HullContact rammer, HullContact rammed) = Astern(1f);
            float touching = Gap(rammer, rammed) + Overlap;

            Vector3 velocity = rammer.Collide(Vector3.forward * Speed);

            Assert.That(velocity.z, Is.EqualTo(Speed / 2f).Within(Tolerance));
            Assert.That(rammed.Drift.z, Is.EqualTo(Speed / 2f).Within(Tolerance));
            Assert.That(Gap(rammer, rammed), Is.EqualTo(touching).Within(Tolerance));
        }

        [Test]
        public void Ram_HeavierShip_IsPushedLessAndSlowsTheRammerMore()
        {
            (HullContact rammer, HullContact rammed) = Astern(2f);
            Assert.That(rammed.Mass, Is.GreaterThan(rammer.Mass));

            Vector3 velocity = rammer.Collide(Vector3.forward * Speed);

            float shared = Speed * rammer.Mass / (rammer.Mass + rammed.Mass);
            Assert.That(velocity.z, Is.EqualTo(shared).Within(Tolerance));
            Assert.That(rammed.Drift.z, Is.EqualTo(shared).Within(Tolerance));
            Assert.That(shared, Is.LessThan(Speed / 2f));
        }

        [Test]
        public void Ram_HullThatCannotMove_StopsTheRammer()
        {
            SO_ShipType type = Type(1f);
            HullContact rammer = Ship("Rammer", type, Vector3.zero);
            Hull("Rock", type, new Vector3(0f, 0f, 2f * HalfLength - Overlap), LayerMask.NameToLayer("Ship"));
            Physics.SyncTransforms();

            Vector3 velocity = rammer.Collide(Vector3.forward * Speed);

            Assert.That(velocity.z, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Collide_BackingAway_KeepsItsSpeedAndPushesNothing()
        {
            (HullContact rammer, HullContact rammed) = Astern(1f);

            Vector3 velocity = rammer.Collide(Vector3.back * Speed);

            Assert.That(velocity.z, Is.EqualTo(-Speed).Within(Tolerance));
            Assert.That(rammed.Drift, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Collide_AlongTheOtherSide_SlidesAtFullSpeed()
        {
            SO_ShipType type = Type(1f);
            HullContact ship = Ship("Ship", type, Vector3.zero);
            HullContact beside = Ship("Beside", type, new Vector3(2f * HalfBeam - Overlap, 0f, 0f));
            Physics.SyncTransforms();

            Vector3 velocity = ship.Collide(Vector3.forward * Speed);

            Assert.That(velocity.z, Is.EqualTo(Speed).Within(Tolerance));
            Assert.That(beside.Drift, Is.EqualTo(Vector3.zero));
            Assert.That(ship.transform.position.x, Is.EqualTo(-Overlap).Within(Tolerance));
        }

        [Test]
        public void Collide_CrateOnAnotherLayer_ChangesNothing()
        {
            SO_ShipType type = Type(1f);
            HullContact ship = Ship("Ship", type, Vector3.zero);
            Hull("Crate", Type(0.1f), new Vector3(0f, 0f, HalfLength), 0);
            Physics.SyncTransforms();

            Vector3 velocity = ship.Collide(Vector3.forward * Speed);

            Assert.That(velocity.z, Is.EqualTo(Speed).Within(Tolerance));
            Assert.That(ship.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Drift_Fades_AfterGlidingAboutSpeedOverDamping()
        {
            HullContact ship = Ship("Ship", Type(1f), Vector3.zero);
            ship.Push(Vector3.right * 2f);

            const float Step = 0.02f;
            for (int i = 0; i < 2000 && ship.Drift != Vector3.zero; i++)
            {
                ship.Tick(Step);
            }

            Assert.That(ship.Drift, Is.EqualTo(Vector3.zero));
            Assert.That(ship.transform.position.x, Is.EqualTo(2f / DriftDamping).Within(0.1f));
        }

        [Test]
        public void ShipController_UnderThrottle_ShovesTheOtherShipWithoutPassingThrough()
        {
            (HullContact rammer, HullContact rammed) = Astern(2f);
            float touching = Gap(rammer, rammed) + Overlap;
            float start = rammed.transform.position.z;
            ShipController controller = rammer.gameObject.AddComponent<ShipController>();
            SetField(controller, "_type", Type(1f));
            SetField(controller, "_contact", rammer);
            controller.SetInput(1f, 0f);

            const float Step = 0.02f;
            for (int i = 0; i < 500; i++)
            {
                controller.Tick(Step);
                rammed.Tick(Step);
                Physics.SyncTransforms();
                Assert.That(Gap(rammer, rammed), Is.GreaterThan(touching - 0.2f), "at step " + i);
            }

            Assert.That(rammed.transform.position.z - start, Is.GreaterThan(1f), "the rammed ship was shoved");
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }
    }
}
