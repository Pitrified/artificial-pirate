namespace Game.UI
{
    using UnityEngine;

    /// <summary>The ship's arrow on the maps: a white triangle pointing up, on a clear ground, made in code.</summary>
    public static class MapArrow
    {
        private const int Size = 32;
        private const float Spread = 0.375f; // the half width at the base, a share of the height

        /// <summary>A new arrow texture; the caller destroys it.</summary>
        public static Texture2D Create()
        {
            var arrow = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { name = "MapArrow" };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                float halfWidth = (Size - 1 - y) * Spread; // widest at the bottom, a point at the top
                for (int x = 0; x < Size; x++)
                {
                    bool inside = Mathf.Abs(x - (Size - 1) * 0.5f) <= halfWidth;
                    pixels[y * Size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }
            arrow.SetPixels32(pixels);
            arrow.Apply(false);
            return arrow;
        }
    }
}
