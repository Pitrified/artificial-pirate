namespace Game.Gameplay.World
{
    /// <summary>
    /// Integer hashing and the noise built on it: the same inputs give the same outputs on every
    /// platform and run, which is what lets a cell be dropped and rebuilt the same.
    /// </summary>
    public static class WorldHash
    {
        private const uint Prime1 = 0x9E3779B1u;
        private const uint Prime2 = 0x85EBCA77u;
        private const uint Prime3 = 0xC2B2AE3Du;
        private const float ToUnit = 1f / 16777216f; // 2^-24: the top 24 bits as a float in [0, 1)

        /// <summary>A well-mixed hash of a seed and two coordinates.</summary>
        public static uint Hash(int seed, int x, int z)
        {
            uint h = (uint)seed * Prime1;
            h ^= (uint)x * Prime2;
            h = (h << 13) | (h >> 19);
            h ^= (uint)z * Prime3;
            return Mix(h);
        }

        /// <summary>A hash in [0, 1).</summary>
        public static float Unit(uint hash)
        {
            return (hash >> 8) * ToUnit;
        }

        /// <summary>Murmur3's finaliser: every input bit reaches every output bit.</summary>
        public static uint Mix(uint h)
        {
            h ^= h >> 16;
            h *= Prime2;
            h ^= h >> 13;
            h *= Prime3;
            h ^= h >> 16;
            return h;
        }

        /// <summary>Smooth value noise in [0, 1) with features about one unit apart.</summary>
        public static float ValueNoise(int seed, float x, float z)
        {
            int x0 = FloorToInt(x);
            int z0 = FloorToInt(z);
            float tx = Fade(x - x0);
            float tz = Fade(z - z0);
            float a = Unit(Hash(seed, x0, z0));
            float b = Unit(Hash(seed, x0 + 1, z0));
            float c = Unit(Hash(seed, x0, z0 + 1));
            float d = Unit(Hash(seed, x0 + 1, z0 + 1));
            float bottom = a + (b - a) * tx;
            float top = c + (d - c) * tx;
            return bottom + (top - bottom) * tz;
        }

        /// <summary>
        /// Value noise summed over <paramref name="octaves"/>, each twice the frequency and half the
        /// weight of the one before, scaled back to [0, 1).
        /// </summary>
        public static float Fractal(int seed, float x, float z, int octaves)
        {
            float sum = 0f;
            float weight = 1f;
            float total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += ValueNoise(seed + i, x, z) * weight;
                total += weight;
                x *= 2f;
                z *= 2f;
                weight *= 0.5f;
            }
            return sum / total;
        }

        private static float Fade(float t)
        {
            return t * t * (3f - 2f * t);
        }

        private static int FloorToInt(float v)
        {
            int i = (int)v;
            return v < i ? i - 1 : i;
        }
    }
}
