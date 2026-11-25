namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="Broadside"/> and <see cref="Cannonballs"/> with ships built in code on the Ship
    /// layer, far from anything a scene left loaded: a side bears on a ship within range and within
    /// the arc of its beam, never on its own side, and fires at the nearest; a type's bow chasers bear
    /// on a ship ahead and its stern guns on one astern, and fire on it; a volley fires its guns
    /// one after another and then reloads; the solved arc lands on a still target and, led, on a
    /// moving one; a ball takes the firer's damage over the target's strength at the point struck,
    /// passing through its own hull; a ball that hits nothing is dropped. Barrels turn toward the
    /// target within the arc and rise to the shot, run in when they fire and out again over the reload.
    /// </summary>
    public sealed class BroadsideTests
    {
        private const float HalfLength = 10f;
        private const float HalfBeam = 3f;
        private const float Draft = 2f;
        private const float Freeboard = 2f;
        private const float Range = 90f;
        private const float Arc = 20f;
        private const float Speed = 60f;
        private const float Gap = 0.1f;
        private const float Reload = 5f;
        private const float Look = 0.25f;
        private const float BallDamage = 0.05f;
        private const float Step = 1f / 60f;
        private const float Trunnions = 1f;
        private const float Recoil = 0.5f;
        private static readonly Vector3 Far = new Vector3(3000f, 0f, 3000f);

        private readonly List<Object> _made = new List<Object>();
        private LayerMask _ships;
        private SO_Gunnery _gunnery;
        private Cannonballs _balls;

        [SetUp]
        public void SetUp()
        {
            _ships = 1 << LayerMask.NameToLayer("Ship");
            _gunnery = ScriptableObject.CreateInstance<SO_Gunnery>();
            _gunnery.Set(Range, Arc, Speed, 0f, Gap, Reload, Look);
            _made.Add(_gunnery);
            var go = new GameObject("Cannonballs");
            _made.Add(go);
            _balls = go.AddComponent<Cannonballs>();
            _balls.Configure(null, _ships, 16);
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

        private SO_ShipType Type(float strength, bool chasers, Vector3[] guns)
        {
            var type = ScriptableObject.CreateInstance<SO_ShipType>();
            type.SetHull(HalfLength, HalfBeam, Draft, Freeboard);
            // Muzzles just inside the hull's box, as a gun amidships can be: its own hull must not stop the ball.
            type.SetGuns(guns ?? new[] { new Vector3(HalfBeam - 0.2f, 1f, -4f), new Vector3(HalfBeam - 0.2f, 1f, 0f), new Vector3(HalfBeam - 0.2f, 1f, 4f) },
                BallDamage, strength);
            if (chasers)
            {
                type.SetChasers(new[] { new Vector3(-0.5f, 1f, HalfLength - 0.2f), new Vector3(0.5f, 1f, HalfLength - 0.2f) },
                    new[] { new Vector3(0f, 1f, -(HalfLength - 0.2f)) });
            }
            _made.Add(type);
            return type;
        }

        private Broadside Ship(string name, ShipSide side, Vector3 offset, float yawDeg = 0f, float strength = 1f, bool barrels = false,
            bool chasers = false, Vector3[] guns = null)
        {
            SO_ShipType type = Type(strength, chasers, guns);
            var root = new GameObject(name);
            _made.Add(root);
            root.transform.SetPositionAndRotation(Far + offset, Quaternion.Euler(0f, yawDeg, 0f));
            var hull = new GameObject("Hull");
            hull.layer = LayerMask.NameToLayer("Ship");
            hull.transform.SetParent(root.transform, false);
            BoxCollider box = hull.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, (Freeboard - Draft) / 2f, 0f);
            box.size = new Vector3(2f * HalfBeam, Draft + Freeboard, 2f * HalfLength);
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(type, box, _ships, 0.5f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            Broadside broadside = root.AddComponent<Broadside>();
            broadside.Configure(side, _gunnery, contact, damage, _balls, 7, barrels ? Barrels(hull.transform, type) : null);
            Physics.SyncTransforms();
            return broadside;
        }

        /// <summary>A barrel for each gun of each battery in turn, turning about a point <see cref="Trunnions"/> inboard of its mouth.</summary>
        private static Transform[] Barrels(Transform hull, SO_ShipType type)
        {
            var barrels = new List<Transform>();
            var outward = new Dictionary<Battery, Vector3>
            {
                { Battery.Starboard, Vector3.right }, { Battery.Port, Vector3.left }, { Battery.Bow, Vector3.forward }, { Battery.Stern, Vector3.back },
            };
            foreach (Battery battery in new[] { Battery.Starboard, Battery.Port, Battery.Bow, Battery.Stern })
            {
                for (int k = 0; k < type.GunCount(battery); k++)
                {
                    var barrel = new GameObject($"Barrel{battery}{k}").transform;
                    barrel.SetParent(hull, false);
                    barrel.localPosition = type.Gun(battery, k) - outward[battery] * Trunnions;
                    barrels.Add(barrel);
                }
            }
            return barrels.ToArray();
        }

        private static Vector3 Bearing(float distance, float degreesFromStarboardBeam)
        {
            return Quaternion.Euler(0f, -degreesFromStarboardBeam, 0f) * Vector3.right * distance;
        }

        private static float Health(Broadside ship) => ship.GetComponent<ShipDamage>().Health;

        private void Run(float seconds, params Broadside[] ships)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                foreach (Broadside ship in ships)
                {
                    ship.Tick(Step);
                }
                _balls.Tick(Step);
            }
        }

        [Test]
        public void InArc_OnTheBeamAndUpToTheArcBothWays_WithinRange()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            Broadside abeam = Ship("Abeam", ShipSide.Pirates, Bearing(60f, 0f));
            Broadside forward = Ship("Forward", ShipSide.Pirates, Bearing(60f, Arc - 1f));
            Broadside aft = Ship("Aft", ShipSide.Pirates, Bearing(60f, -(Arc - 1f)));
            Broadside beyondArc = Ship("BeyondArc", ShipSide.Pirates, Bearing(60f, Arc + 2f));
            Broadside ahead = Ship("Ahead", ShipSide.Pirates, Vector3.forward * 60f);
            Broadside beyondRange = Ship("BeyondRange", ShipSide.Pirates, Bearing(Range + 5f, 0f));
            Broadside toPort = Ship("ToPort", ShipSide.Pirates, -Bearing(60f, 0f));

            Assert.IsTrue(us.InArc(abeam, Battery.Starboard));
            Assert.IsTrue(us.InArc(forward, Battery.Starboard));
            Assert.IsTrue(us.InArc(aft, Battery.Starboard));
            Assert.IsFalse(us.InArc(beyondArc, Battery.Starboard));
            Assert.IsFalse(us.InArc(ahead, Battery.Starboard));
            Assert.IsFalse(us.InArc(ahead, Battery.Port));
            Assert.IsFalse(us.InArc(beyondRange, Battery.Starboard));
            Assert.IsFalse(us.InArc(toPort, Battery.Starboard));
            Assert.IsTrue(us.InArc(toPort, Battery.Port));
        }

        [Test]
        public void Look_TargetsTheNearestOfAnotherSide_NeverItsOwn()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            Ship("Ally", ShipSide.Player, Bearing(30f, 0f));
            Broadside near = Ship("Near", ShipSide.Pirates, Bearing(55f, 5f));
            Ship("FarOff", ShipSide.Pirates, Bearing(80f, -5f));

            us.Tick(Step);

            Assert.AreSame(near, us.TargetOf(Battery.Starboard));
            Assert.IsNull(us.TargetOf(Battery.Port));
        }

        [Test]
        public void Volley_FiresEachGunAGapApart_ThenReloads()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            Ship("Them", ShipSide.Pirates, Bearing(60f, 0f));
            var launches = new List<float>();
            float now = 0f;
            _balls.Launched += (muzzle, direction) => launches.Add(now);

            for (; now < Reload; now += Step)
            {
                us.Tick(Step);
            }

            Assert.AreEqual(3, launches.Count, "one volley of the side's three guns");
            Assert.AreEqual(Gap, launches[1] - launches[0], Step + 1e-4f);
            Assert.AreEqual(Gap, launches[2] - launches[1], Step + 1e-4f);

            for (; now < 2f * Gap + Reload + 2f * Step; now += Step)
            {
                us.Tick(Step);
            }
            Assert.AreEqual(4, launches.Count, "the next volley once reloaded");
        }

        [Test]
        public void Volley_TwoRows_FireTogether_StepByStep()
        {
            Vector3[] rows =
            {
                new Vector3(HalfBeam - 0.2f, 1f, -4f), new Vector3(HalfBeam - 0.2f, 1f, 0f), new Vector3(HalfBeam - 0.2f, 1f, 4f),
                new Vector3(HalfBeam - 0.2f, 2f, -3f), new Vector3(HalfBeam - 0.2f, 2f, 1f),
            };
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero, guns: rows);
            Ship("Them", ShipSide.Pirates, Bearing(60f, 0f));
            var launches = new List<float>();
            float now = 0f;
            _balls.Launched += (muzzle, direction) => launches.Add(now);

            for (; now < Reload; now += Step)
            {
                us.Tick(Step);
            }

            Assert.AreEqual(5, launches.Count, "one volley of both rows");
            Assert.AreEqual(launches[0], launches[1], "the rows' sternmost guns at once");
            Assert.AreEqual(launches[2], launches[3], "their second guns at once");
            Assert.AreEqual(Gap, launches[2] - launches[0], Step + 1e-4f);
            Assert.AreEqual(Gap, launches[4] - launches[2], Step + 1e-4f, "the longer row's last gun a step on");
        }

        [Test]
        public void Solve_StillTarget_TheArcPassesThroughIt()
        {
            Vector3 from = new Vector3(0f, 2f, 0f);
            Vector3 to = new Vector3(70f, 1f, 20f);

            Assert.IsTrue(Broadside.Solve(from, to, Vector3.zero, Speed, out Vector3 velocity));

            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            float time = new Vector3(to.x - from.x, 0f, to.z - from.z).magnitude / flat.magnitude;
            Vector3 landed = from + velocity * time + 0.5f * time * time * Physics.gravity;
            Assert.That(Vector3.Distance(landed, to), Is.LessThan(0.01f));
            Assert.That(velocity.magnitude, Is.EqualTo(Speed).Within(1e-3f));
        }

        [Test]
        public void Solve_MovingTarget_LeadsItSoTheyMeet()
        {
            Vector3 from = Vector3.zero;
            Vector3 to = new Vector3(70f, 0f, 0f);
            Vector3 moving = new Vector3(0f, 0f, 6f);

            Assert.IsTrue(Broadside.Solve(from, to, moving, Speed, out Vector3 velocity));

            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            float time = 70f / flat.x;
            Vector3 ball = from + velocity * time + 0.5f * time * time * Physics.gravity;
            Assert.That(Vector3.Distance(ball, to + moving * time), Is.LessThan(0.2f));
        }

        [Test]
        public void Ball_StrikesTheTarget_ForTheFirersDamageOverItsStrength()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            Broadside them = Ship("Them", ShipSide.Pirates, Bearing(50f, 0f), 0f, 2f);
            var struck = new List<Vector3>();
            _balls.HitHull += (point, normal) => struck.Add(point);

            Run(3f * Gap + 2f, us);

            Assert.AreEqual(3, struck.Count, "the volley's three balls struck");
            Assert.AreEqual(1f, Health(us), 1e-5f, "our own hull, which the balls start inside, is untouched");
            Assert.AreEqual(1f - 3f * BallDamage / 2f, Health(them), 1e-4f);
            Vector3 local = them.Hull.InverseTransformPoint(struck[0]);
            Assert.That(local.x, Is.EqualTo(-HalfBeam).Within(0.05f), "struck on the side facing us");
        }

        [Test]
        public void Barrels_TurnTowardTheTarget_RunInOnFiringAndOutAgain()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero, barrels: true);
            Ship("Them", ShipSide.Pirates, Bearing(60f, 15f));
            Transform first = us.Hull.Find("BarrelStarboard0");
            Transform port = us.Hull.Find("BarrelPort0");
            Vector3 rest = first.localPosition;

            us.Tick(Step);
            Assert.AreEqual(Recoil, Vector3.Distance(rest, first.localPosition), 0.01f, "the first gun fired and ran in");

            for (float t = 0f; t < 2f; t += Step)
            {
                us.Tick(Step);
            }
            Vector3 pointing = first.rotation * Vector3.right;
            float toBow = Vector3.SignedAngle(us.transform.right, new Vector3(pointing.x, 0f, pointing.z), Vector3.up);
            Assert.That(toBow, Is.LessThan(-5f).And.GreaterThan(-Arc - 0.5f), "turned toward the bow, within the arc");
            Assert.That(pointing.y, Is.GreaterThan(0f), "raised to the shot");
            Assert.AreEqual(Quaternion.identity, port.localRotation, "the other side, with no target, at rest");

            for (float t = 0f; t < Reload - 2f; t += Step)
            {
                us.Tick(Step);
            }
            Assert.That(Vector3.Distance(rest, first.localPosition), Is.LessThan(0.01f), "run out again before the next volley");
        }

        [Test]
        public void Ball_HittingNothing_IsDroppedAfterItsTime()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            _balls.Launch(us.Hull.TransformPoint(new Vector3(HalfBeam + 0.3f, 1f, 0f)), Vector3.right * Speed, us);
            Assert.AreEqual(1, _balls.Flying);

            for (float t = 0f; t < 7f; t += Step)
            {
                _balls.Tick(Step);
            }

            Assert.AreEqual(0, _balls.Flying);
        }

        [Test]
        public void Chasers_BearAheadAndStern_Astern_TheBroadsidesOnTheBeam()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero, chasers: true);
            Broadside ahead = Ship("Ahead", ShipSide.Pirates, Quaternion.Euler(0f, 25f, 0f) * Vector3.forward * 60f);
            Broadside astern = Ship("Astern", ShipSide.Pirates, Vector3.back * 60f);
            Broadside abeam = Ship("Abeam", ShipSide.Pirates, Bearing(40f, 0f));

            Assert.IsTrue(us.InArc(ahead, Battery.Bow));
            Assert.IsFalse(us.InArc(ahead, Battery.Starboard));
            Assert.IsTrue(us.InArc(astern, Battery.Stern));
            Assert.IsFalse(us.InArc(abeam, Battery.Bow));
            Assert.IsFalse(us.InArc(abeam, Battery.Stern));

            us.Tick(Step);

            Assert.AreSame(ahead, us.TargetOf(Battery.Bow));
            Assert.AreSame(astern, us.TargetOf(Battery.Stern));
            Assert.AreSame(abeam, us.TargetOf(Battery.Starboard));
        }

        [Test]
        public void NoChasers_NothingBearsAheadOrAstern()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero);
            Ship("Ahead", ShipSide.Pirates, Vector3.forward * 60f);
            Ship("Astern", ShipSide.Pirates, Vector3.back * 60f);

            us.Tick(Step);

            foreach (Battery battery in new[] { Battery.Starboard, Battery.Port, Battery.Bow, Battery.Stern })
            {
                Assert.IsNull(us.TargetOf(battery), battery.ToString());
            }
        }

        [Test]
        public void Chasers_FireTheirVolley_AndHitTheShipAhead()
        {
            Broadside us = Ship("Us", ShipSide.Player, Vector3.zero, barrels: true, chasers: true);
            Broadside ahead = Ship("Ahead", ShipSide.Pirates, Vector3.forward * 50f);
            var volleys = new List<Battery>();
            us.VolleyFired += volleys.Add;

            Run(2f, us);

            Assert.AreEqual(new[] { Battery.Bow }, volleys.ToArray());
            Assert.Less(Health(ahead), 1f, "the chasers hit the ship ahead");
        }
    }
}
