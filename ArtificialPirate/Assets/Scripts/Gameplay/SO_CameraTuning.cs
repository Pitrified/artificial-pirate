namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Designer-tunable data for the third-person chase camera. Where it sits comes from the
    /// followed ship's type (<see cref="SO_ShipType.CameraOffset"/>).
    /// Create an instance via Assets > Create > artificial-pirate > Camera Tuning
    /// and assign it to the CameraController on MainCamera.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pirate/Camera Tuning", fileName = "SO_CameraTuning")]
    public sealed class SO_CameraTuning : ScriptableObject
    {
        [SerializeField] private float _positionLerp = 5f;
        [SerializeField] private float _rotationLerp = 5f;
        [SerializeField] private bool _lookAtTarget = true;

        /// <summary>Per-second position smoothing coefficient for framerate-independent Lerp.</summary>
        public float PositionLerp => _positionLerp;

        /// <summary>Per-second rotation smoothing coefficient for framerate-independent Slerp.</summary>
        public float RotationLerp => _rotationLerp;

        /// <summary>When true the camera continuously rotates to look at the target.</summary>
        public bool LookAtTarget => _lookAtTarget;
    }
}
