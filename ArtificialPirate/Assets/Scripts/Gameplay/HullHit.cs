namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>A hit on the hull: where it landed in the hull's local space, and the health it took.</summary>
    public readonly struct HullHit
    {
        /// <summary>Where it landed, in the hull's local space.</summary>
        public readonly Vector3 Point;

        /// <summary>The health it took, 0 to 1.</summary>
        public readonly float Amount;

        /// <summary>Initializes a new <see cref="HullHit"/>.</summary>
        public HullHit(Vector3 point, float amount)
        {
            Point = point;
            Amount = amount;
        }
    }
}
