namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;
    using Game.Gameplay.World;

    /// <summary>
    /// <see cref="ShipDamage"/> on hulls built in code on the Ship layer: a ram hurts both ships by
    /// their share of the masses, a nudge does no harm, a grounding hurts once however long the
    /// throttle is held against the shore, health stops at 0, and a damaged ship is slower, sits
    /// lower and lists toward the side that was hit.
    /// </summary>
    public sealed class ShipDamageTests
    {
        private const float HalfLength = 10f;
        private const float HalfBeam = 3f;
        private const float Draft = 2.4f;
        private const float Freeboard = 2.5f;
        private const float DriftDamping = 0.5f;
        private const float Overlap = 0.5f;
        private const float Speed = 6f;
        private const float Tolerance = 1e-4f;

        private readonly List<Object> _made = new List<Object>();
        private LayerMask _ships;

        [SetUp]
        public void SetUp()
        {
            _ships = 1 << LayerMask.NameToLayer("Ship");
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

        private ShipDamage Ship(string name, SO_ShipType type, Vector3 position)
        {
            var root = new GameObject(name);
            _made.Add(root);
            root.transform.position = position;
            var hull = new GameObject("Hull");
            hull.layer = LayerMask.NameToLayer("Ship");
            hull.transform.SetParent(root.transform, false);
            hull.transform.localScale = new Vector3(2f * type.HalfBeam, type.Draft + type.Freeboard, 2f * type.HalfLength);
            BoxCollider box = hull.AddComponent<BoxCollider>();
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(type, box, _ships, DriftDamping);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            return damage;
        }

        private static float Blow(SO_ShipType type, float speed, float share)
        {
            float harmless = type.HarmlessSpeed;
            return share * (speed * speed - harmless * harmless) / (type.SinkingSpeed * type.SinkingSpeed);
        }

        /// <summary>The rammer's bow overlaps the other's stern, both heading +z.</summary>
        private (ShipDamage rammer, ShipDamage rammed) Astern(float rammedScale)
        {
            SO_ShipType rammerType = Type(1f);
            SO_ShipType rammedType = Type(rammedScale);
            ShipDamage rammer = Ship("Rammer", rammerType, Vector3.zero);
            float gap = rammerType.HalfLength + rammedType.HalfLength - Overlap;
            ShipDamage rammed = Ship("Rammed", rammedType, new Vector3(0f, 0f, gap));
            Physics.SyncTransforms();
            return (rammer, rammed);
        }

        [Test]
        public void Ram_HurtsBothShips_ByTheirShareOfTheMasses()
        {
            (ShipDamage rammer, ShipDamage rammed) = Astern(2f);
            HullContact rammerHull = rammer.GetComponent<HullContact>();
            HullContact rammedHull = rammed.GetComponent<HullContact>();
            float total = rammerHull.Mass + rammedHull.Mass;

            rammerHull.Collide(Vector3.forward * Speed);

            float rammerTook = Blow(rammerHull.Type, Speed, rammedHull.Mass / total);
            float rammedTook = Blow(rammedHull.Type, Speed, rammerHull.Mass / total);
            Assert.That(rammer.Health, Is.EqualTo(1f - rammerTook).Within(Tolerance));
            Assert.That(rammed.Health, Is.EqualTo(1f - rammedTook).Within(Tolerance));
            Assert.That(rammerTook, Is.GreaterThan(rammedTook), "the lighter ship takes more");
        }

        [Test]
        public void Ram_BelowTheHarmlessSpeed_DoesNoHarm()
        {
            (ShipDamage rammer, ShipDamage rammed) = Astern(1f);
            float slow = rammer.GetComponent<HullContact>().Type.HarmlessSpeed * 0.9f;

            rammer.GetComponent<HullContact>().Collide(Vector3.forward * slow);

            Assert.AreEqual(1f, rammer.Health);
            Assert.AreEqual(1f, rammed.Health);
        }

        [Test]
        public void Grounding_HurtsOnce_WithTheThrottleHeldAgainstTheShore()
        {
            SO_ShipType type = Type(1f);
            ShipDamage damage = Ship("Ship", type, new Vector3(0f, 0f, 120f));
            HullContact contact = damage.GetComponent<HullContact>();
            var settings = ScriptableObject.CreateInstance<SO_WorldSettings>();
            _made.Add(settings);
            contact.UseGround(new WorldGenerator(1, settings));
            ShipController controller = damage.gameObject.AddComponent<ShipController>();
            SetField(controller, "_type", type);
            SetField(controller, "_contact", contact);
            SetField(controller, "_damage", damage);
            controller.Teleport(new Vector3(0f, 0f, 120f), 180f);
            controller.SetInput(1f, 0f);

            const float Step = 0.02f;
            for (int i = 0; i < 1000; i++)
            {
                controller.Tick(Step);
            }

            Assert.That(damage.Health, Is.EqualTo(1f - Blow(type, type.MaxSpeed, 1f)).Within(Tolerance), "one blow at full speed");
        }

        [Test]
        public void Hit_StopsAtZero_AndReportsWhatItTook()
        {
            ShipDamage damage = Ship("Ship", Type(1f), Vector3.zero);
            var took = new List<float>();
            damage.Damaged += (_, amount) => took.Add(amount);

            damage.Hit(Vector3.right, 0.7f);
            damage.Hit(Vector3.right, 0.7f);
            damage.Hit(Vector3.right, 0.7f);

            Assert.AreEqual(0f, damage.Health);
            Assert.AreEqual(2, took.Count, "a ruined hull takes no more");
            Assert.That(took[1], Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void Damage_Slows_Settles_AndListsTowardTheSideHit()
        {
            SO_ShipType type = Type(1f);
            ShipDamage damage = Ship("Ship", type, Vector3.zero);

            damage.Hit(new Vector3(HalfBeam, 0f, 2f), 0.5f);

            Assert.That(damage.SpeedFactor, Is.EqualTo(Mathf.Lerp(type.DamagedSpeedFloor, 1f, 0.5f)).Within(Tolerance));
            Assert.That(damage.Sink, Is.EqualTo(0.5f * type.DamagedSinkShare * type.Freeboard).Within(Tolerance));
            Assert.That(damage.ListDeg, Is.EqualTo(0.5f * type.DamagedListDeg).Within(Tolerance), "toward starboard");

            damage.Hit(new Vector3(-HalfBeam, 0f, -2f), 0.5f);

            Assert.That(damage.SpeedFactor, Is.EqualTo(type.DamagedSpeedFloor).Within(Tolerance));
            Assert.That(damage.ListDeg, Is.EqualTo(0f).Within(Tolerance), "as much to port: level");
        }

        [Test]
        public void ShipController_RuinedHull_SailsAtTheFloorSpeed()
        {
            SO_ShipType type = Type(1f);
            ShipDamage damage = Ship("Ship", type, Vector3.zero);
            damage.Hit(Vector3.zero, 1f);
            ShipController controller = damage.gameObject.AddComponent<ShipController>();
            SetField(controller, "_type", type);
            SetField(controller, "_damage", damage);
            controller.SetInput(1f, 0f);
            for (int i = 0; i < 100; i++)
            {
                controller.Tick(0.1f);
            }

            float before = controller.GetState().Position.z;
            controller.Tick(1f);

            float speed = controller.GetState().Position.z - before;
            Assert.That(speed, Is.EqualTo(type.MaxSpeed * type.DamagedSpeedFloor).Within(1e-3f));
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }
    }
}
