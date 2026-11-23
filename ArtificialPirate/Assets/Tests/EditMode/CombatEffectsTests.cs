namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    /// <summary>
    /// <see cref="CombatEffects"/> and <see cref="DamageSmoke"/> with particle systems made in code:
    /// a launch flashes and smokes, a hull hit throws splinters and dust, a ball that strikes
    /// nothing emits nothing else; a ship smokes from above its heaviest hit only below the
    /// threshold, and not every step.
    /// </summary>
    public sealed class CombatEffectsTests
    {
        private static readonly Vector3 Far = new Vector3(-3000f, 0f, 3000f);

        private readonly List<Object> _made = new List<Object>();
        private Cannonballs _balls;
        private CombatEffects _effects;
        private ParticleSystem _flash;
        private ParticleSystem _smoke;
        private ParticleSystem _splinters;
        private ParticleSystem _splash;
        private ParticleSystem _dirt;

        [SetUp]
        public void SetUp()
        {
            var ballsObject = Make("Cannonballs");
            _balls = ballsObject.AddComponent<Cannonballs>();
            _balls.Configure(null, 1 << LayerMask.NameToLayer("Ship"), 4);
            _flash = System("Flash");
            _smoke = System("Smoke");
            _splinters = System("Splinters");
            _splash = System("Splash");
            _dirt = System("Dirt");
            _effects = Make("Effects").AddComponent<CombatEffects>();
            _effects.Configure(_balls, _flash, _smoke, _splinters, _splash, _dirt);
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

        private GameObject Make(string name)
        {
            var go = new GameObject(name);
            _made.Add(go);
            return go;
        }

        private ParticleSystem System(string name)
        {
            ParticleSystem system = Make(name).AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 1000;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;
            system.Simulate(0f, true, true, false); // a system that has never stepped drops what is emitted into it
            return system;
        }

        /// <summary>The particles a system holds; a stopped system counts what was emitted once it steps.</summary>
        private static int Count(ParticleSystem system)
        {
            system.Simulate(1e-4f, true, false, false);
            return system.particleCount;
        }

        [Test]
        public void Launch_FlashesAndSmokes_AtTheMuzzle()
        {
            _balls.Launch(Far, Vector3.right * 60f, null);

            Assert.That(Count(_flash), Is.GreaterThan(0));
            Assert.That(Count(_smoke), Is.GreaterThan(0));
            Assert.AreEqual(0, Count(_splinters) + Count(_splash) + Count(_dirt));
        }

        [Test]
        public void HullHit_ThrowsSplintersAndDust()
        {
            var target = Make("Target");
            target.transform.position = Far + Vector3.right * 20f;
            target.layer = LayerMask.NameToLayer("Ship");
            target.AddComponent<BoxCollider>().size = new Vector3(2f, 4f, 10f);
            Physics.SyncTransforms();
            _balls.Launch(Far + Vector3.up * 0.5f, Vector3.right * 60f, null);
            int smokeAtLaunch = Count(_smoke);

            for (int i = 0; i < 30; i++)
            {
                _balls.Tick(1f / 60f);
            }

            Assert.That(Count(_splinters), Is.GreaterThan(0));
            Assert.That(Count(_smoke), Is.GreaterThan(smokeAtLaunch), "dust from the hit");
            Assert.AreEqual(0, Count(_splash) + Count(_dirt));
        }

        private ShipDamage Ship()
        {
            var type = ScriptableObject.CreateInstance<SO_ShipType>();
            type.SetHull(10f, 3f, 2f, 2f);
            _made.Add(type);
            var root = Make("Ship");
            root.transform.position = Far;
            var hull = new GameObject("Hull");
            hull.transform.SetParent(root.transform, false);
            hull.layer = LayerMask.NameToLayer("Ship");
            BoxCollider box = hull.AddComponent<BoxCollider>();
            HullContact contact = root.AddComponent<HullContact>();
            contact.Configure(type, box, 1 << LayerMask.NameToLayer("Ship"), 0.5f);
            ShipDamage damage = root.AddComponent<ShipDamage>();
            damage.Configure(contact);
            return damage;
        }

        [Test]
        public void DamageSmoke_OnlyBelowTheThreshold_FromAboveTheHeaviestHit()
        {
            ShipDamage damage = Ship();
            DamageSmoke smoke = damage.gameObject.AddComponent<DamageSmoke>();
            smoke.Configure(damage, _effects);

            damage.Hit(Far + new Vector3(3f, 0.5f, 4f), 0.5f);
            Assert.IsFalse(smoke.Tick(1f), "half the health: no smoke");
            Assert.AreEqual(0, Count(_smoke));

            damage.Hit(Far + new Vector3(-3f, 0.5f, -2f), 0.3f);
            Assert.IsTrue(smoke.Tick(1f), "a fifth left: smoke");
            Assert.IsFalse(smoke.Tick(0.01f), "not again at once");

            var particles = new ParticleSystem.Particle[Count(_smoke)];
            _smoke.GetParticles(particles);
            Assert.That(particles.Length, Is.GreaterThan(0));
            Assert.AreEqual(Far.x + 3f, particles[0].position.x, 0.01f, "over the heavier hit, to starboard");
            Assert.AreEqual(Far.z + 4f, particles[0].position.z, 0.01f);
            Assert.AreEqual(2f, particles[0].position.y, 0.01f, "at the rail");
        }
    }
}
