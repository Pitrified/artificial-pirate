namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.TestTools;
    using Game.Gameplay.Water;

    /// <summary>
    /// The ripple simulation on the GPU, read back: an impulse lifts the water where it lands,
    /// a ring has travelled outward a few steps later, and the field dies down; a square that
    /// follows a target keeps its ripples where they are in the world; foam fades and spreads;
    /// a moving <see cref="WakeSource"/> leaves foam on its path and ripples behind it. And the
    /// sample cannonball launcher's ball lands in the water and sinks.
    /// </summary>
    public sealed class WaterRippleTests
    {
        private const int Resolution = 64;
        private const float Size = 32f;
        private const int Height = 0;
        private const int Foam = 2;

        // Far from the Game scene's island, which an earlier test may have left loaded.
        private static readonly Vector3 Origin = new Vector3(1000f, 0f, 1000f);

        private GameObject _host;
        private SO_WaterSettings _settings;
        private Material _simulation;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WaterSettings>();
            _settings.SetWaves(0f, new GerstnerWave[0]);
            _host = new GameObject("TestWater");
            _host.AddComponent<WaterSurface>().UseSettings(_settings);
            _simulation = new Material(Shader.Find("artificial-pirate/WaterRippleSim"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_host);
            Object.Destroy(_settings);
            Object.Destroy(_simulation);
        }

        [UnityTest]
        public IEnumerator Impulse_SpreadsAsARing_AndDiesDown()
        {
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), Size, Resolution, 0f); // stepped by hand below

            ripples.AddImpulse(Origin, 1.5f, 0.5f);
            ripples.Step(1);
            yield return null;
            yield return null;
            float[] first = Read(ripples.Field, Height);
            int centre = Resolution / 2 * Resolution + Resolution / 2;
            Assert.That(first[centre], Is.GreaterThan(0.1f), "the impulse lifts the water where it lands");

            ripples.Step(10);
            yield return null;
            yield return null;
            float[] later = Read(ripples.Field, Height);
            float ring = RingEnergy(later, 6, 14);
            Assert.That(ring, Is.GreaterThan(RingEnergy(first, 6, 14) + 0.01f), "a ring has travelled outward");

            float before = Energy(later);
            ripples.Step(150);
            yield return null;
            yield return null;
            Assert.That(Energy(Read(ripples.Field, Height)), Is.LessThan(before * 0.2f), "the ripples die down");
        }

        [UnityTest]
        public IEnumerator FollowingSquare_KeepsRipplesInTheirWorldPlace()
        {
            var target = new GameObject("Target").transform;
            target.position = Origin;
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), Size, Resolution, 0f, target);

            Vector3 splash = Origin + new Vector3(4f, 0f, -3f);
            ripples.AddImpulse(splash, 1.5f, 0.5f);
            ripples.Step(1);
            yield return null;
            Vector2 before = PeakWorld(Read(ripples.Field, Height), ripples.Rect);

            target.position = Origin + new Vector3(3.1f, 0f, 2.4f); // not whole texels: rounded to them
            ripples.Step(1);
            yield return null;
            Vector4 rect = ripples.Rect;
            Vector2 after = PeakWorld(Read(ripples.Field, Height), rect);

            float texel = Size / Resolution;
            Assert.AreEqual(Origin.x + 3f - Size * 0.5f, rect.x, 1e-3f, "the square moved onto the target, in whole texels");
            Assert.AreEqual(Origin.z + 2.5f - Size * 0.5f, rect.y, 1e-3f, "the square moved onto the target, in whole texels");
            Assert.AreEqual(splash.x, before.x, texel, "the splash lands where it was made");
            // Within a texel: the stamp straddles two texels, either can read highest.
            Assert.AreEqual(before.x, after.x, texel + 1e-3f, "and stays there in the world when the square moves");
            Assert.AreEqual(before.y, after.y, texel + 1e-3f, "and stays there in the world when the square moves");
            Object.Destroy(target.gameObject);
        }

        [UnityTest]
        public IEnumerator Foam_FadesAndSpreads()
        {
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), Size, Resolution, 0f);
            int centre = Resolution / 2 * Resolution + Resolution / 2;
            int outside = Resolution / 2 * Resolution + Resolution / 2 + 5; // 2.5 m off, beyond the 2 m stamp

            ripples.AddFoam(Origin, 2f, 1f);
            ripples.Step(1);
            yield return null;
            float[] first = Read(ripples.Field, Foam);
            Assert.That(first[centre], Is.GreaterThan(0.9f), "full foam where it was stamped");
            Assert.AreEqual(0f, first[outside], 1e-4f, "none beyond the stamp");
            Assert.AreEqual(0f, Energy(Read(ripples.Field, Height)), 1e-6f, "foam does not make ripples");

            ripples.Step(45); // 1.5 s at 30 steps/s
            yield return null;
            float[] later = Read(ripples.Field, Foam);
            Assert.That(later[centre], Is.InRange(0.2f, 0.7f), "about half gone after 1.5 s");
            Assert.That(later[outside], Is.GreaterThan(0.01f), "it spread beyond the stamp");
        }

        [UnityTest]
        public IEnumerator MovingWake_LeavesFoamOnItsPath_AndRipplesBehind()
        {
            const float speed = 8f;
            const float dt = 1f / 30f;
            const float gameSize = 12f; // 0.1875 m texels, as in the game, so ripples travel at its 4 m/s
            var ship = new GameObject("Ship").transform;
            ship.SetPositionAndRotation(Origin + new Vector3(-10f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f)); // heading +x
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), gameSize, Resolution, 0f, ship);
            var wake = ship.gameObject.AddComponent<WakeSource>();
            wake.Configure(ripples, 1f, 0.5f);

            for (int i = 0; i < 45; i++) // 1.5 s, 12 m
            {
                ship.position += Vector3.right * (speed * dt);
                wake.Tick(dt);
                ripples.Step(1);
            }
            yield return null;
            Vector4 rect = ripples.Rect;
            float[] foam = Read(ripples.Field, Foam);
            float[] height = Read(ripples.Field, Height);
            Vector3 at = ship.position;

            Assert.That(Sample(foam, rect, at + new Vector3(-3f, 0f, 0f)), Is.GreaterThan(0.2f), "foam on the path behind");
            Assert.AreEqual(0f, Sample(foam, rect, at + new Vector3(3f, 0f, 0f)), 1e-3f, "no foam ahead");
            Assert.AreEqual(0f, Sample(foam, rect, at + new Vector3(-3f, 0f, 2.5f)), 1e-2f, "none far to the side");
            float behind = BoxEnergy(height, rect, at + new Vector3(-3.5f, 0f, 0f), 5f);
            float ahead = BoxEnergy(height, rect, at + new Vector3(4f, 0f, 0f), 3f);
            Assert.That(behind, Is.GreaterThan(0f), "ripples behind");
            Assert.That(ahead, Is.LessThan(behind * 0.1f), "faster than the ripples travel: almost none ahead");
            Object.Destroy(ship.gameObject);
        }

        [UnityTest]
        public IEnumerator Cannonball_LandsInTheWater_AndSinks()
        {
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), Size, Resolution, 30f);
            var launcherObject = new GameObject("Launcher");
            launcherObject.transform.position = Origin + new Vector3(0f, 2f, -8f);
            var launcher = launcherObject.AddComponent<CannonballLauncher>();
            launcher.Configure(_host.GetComponent<WaterSurface>(), ripples, Origin, 999f);

            launcher.FireNow();
            Rigidbody ball = launcher.Balls[0];
            for (int i = 0; i < Mathf.RoundToInt(4f / Time.fixedDeltaTime); i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(ball.position.y, Is.LessThan(-1f), "an iron ball sinks");
            Assert.That(new Vector2(ball.position.x - Origin.x, ball.position.z - Origin.z).magnitude, Is.LessThan(8f),
                "it landed near the target (spread 5 m, and it drifts as it sinks)");
            Object.Destroy(launcherObject);
            foreach (Rigidbody b in launcher.Balls)
            {
                Object.Destroy(b.gameObject);
            }
        }

        // One channel of the field: Height or Foam.
        private static float[] Read(RenderTexture field, int channel)
        {
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(field, 0, TextureFormat.RGBAFloat);
            request.WaitForCompletion();
            NativeArray<Vector4> data = request.GetData<Vector4>();
            var values = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                values[i] = data[i][channel];
            }
            return values;
        }

        // The texel under a world position, in a field covering rect (x min, z min, size).
        private static Vector2Int Texel(Vector4 rect, Vector3 world) => new Vector2Int(
            Mathf.FloorToInt((world.x - rect.x) / rect.z * Resolution),
            Mathf.FloorToInt((world.z - rect.y) / rect.z * Resolution));

        private static float Sample(float[] h, Vector4 rect, Vector3 world)
        {
            Vector2Int t = Texel(rect, world);
            return h[t.y * Resolution + t.x];
        }

        // The world x and z of the highest texel's centre.
        private static Vector2 PeakWorld(float[] h, Vector4 rect)
        {
            int best = 0;
            for (int i = 1; i < h.Length; i++)
            {
                if (h[i] > h[best])
                {
                    best = i;
                }
            }
            float texel = rect.z / Resolution;
            return new Vector2(rect.x + (best % Resolution + 0.5f) * texel, rect.y + (best / Resolution + 0.5f) * texel);
        }

        // Squared height over a square of side metres centred on a world position.
        private static float BoxEnergy(float[] h, Vector4 rect, Vector3 centre, float side)
        {
            Vector2Int min = Texel(rect, centre - new Vector3(side, 0f, side) * 0.5f);
            Vector2Int max = Texel(rect, centre + new Vector3(side, 0f, side) * 0.5f);
            float sum = 0f;
            for (int y = Mathf.Max(0, min.y); y <= Mathf.Min(Resolution - 1, max.y); y++)
            {
                for (int x = Mathf.Max(0, min.x); x <= Mathf.Min(Resolution - 1, max.x); x++)
                {
                    sum += h[y * Resolution + x] * h[y * Resolution + x];
                }
            }
            return sum;
        }

        private static float Energy(float[] h)
        {
            float sum = 0f;
            foreach (float v in h)
            {
                sum += v * v;
            }
            return sum;
        }

        private static float RingEnergy(float[] h, int inner, int outer)
        {
            float sum = 0f;
            int c = Resolution / 2;
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    if (r >= inner && r < outer)
                    {
                        sum += h[y * Resolution + x] * h[y * Resolution + x];
                    }
                }
            }
            return sum;
        }
    }
}
