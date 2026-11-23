namespace Game.UI
{
    using UnityEngine;

    /// <summary>A ship on the full map: where, its heading, its icon, tint and size.</summary>
    public struct MapMarker
    {
        /// <summary>Its place on the map's picture, 0 to 1 from the south-west corner.</summary>
        public Vector2 Uv;

        /// <summary>Its heading, degrees clockwise from north.</summary>
        public float YawDeg;

        /// <summary>Its type's icon, bow up.</summary>
        public Texture2D Icon;

        /// <summary>Its side's tint.</summary>
        public Color Tint;

        /// <summary>The icon's side, in pixels.</summary>
        public float Pixels;
    }
}
