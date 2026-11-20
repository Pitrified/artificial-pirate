namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// One mast of a ship's model: where it stands in the hull's space, the objects that go when it
    /// breaks (the mast, its sail states, the flag on the tallest), and the stump left behind.
    /// </summary>
    [System.Serializable]
    public struct MastParts
    {
        [Tooltip("Where the mast stands along the hull, toward the bow (m, the hull's space).")]
        [SerializeField] private float _alongZ;

        [Tooltip("The mast's foot on deck, above the waterline (m, the hull's space).")]
        [SerializeField] private float _footY;

        [Tooltip("The objects that go over the side when it breaks.")]
        [SerializeField] private GameObject[] _parts;

        [Tooltip("What is left standing; off until the mast breaks.")]
        [SerializeField] private GameObject _stump;

        /// <summary>Initializes a new <see cref="MastParts"/>.</summary>
        public MastParts(float alongZ, float footY, GameObject[] parts, GameObject stump)
        {
            _alongZ = alongZ;
            _footY = footY;
            _parts = parts;
            _stump = stump;
        }

        /// <summary>Where the mast stands along the hull, toward the bow, in metres.</summary>
        public float AlongZ => _alongZ;

        /// <summary>The mast's foot on deck above the waterline, in metres.</summary>
        public float FootY => _footY;

        /// <summary>The objects that go over the side when it breaks.</summary>
        public GameObject[] Parts => _parts;

        /// <summary>What is left standing when it breaks.</summary>
        public GameObject Stump => _stump;
    }
}
