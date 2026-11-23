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

        [Tooltip("How long a change of camera takes to ease from one view to the next (s).")]
        [SerializeField] private float _switchSeconds = 0.5f;

        [Header("Shake")]
        [Tooltip("The most the camera shakes, each way (m).")]
        [SerializeField] private float _shakeMost = 0.8f;

        [Tooltip("How fast a shake dies away (1/s): it falls to about a third in 1 / this seconds.")]
        [SerializeField] private float _shakeDecay = 6f;

        [Tooltip("How quickly the shake moves (Hz).")]
        [SerializeField] private float _shakeFrequency = 12f;

        [Tooltip("The shake of the followed ship's own volley (m).")]
        [SerializeField] private float _volleyShake = 0.12f;

        [Tooltip("The shake of a hit on the followed ship, per share of health it takes (m).")]
        [SerializeField] private float _hitShakePerHealth = 10f;

        /// <summary>Per-second position smoothing coefficient for framerate-independent Lerp.</summary>
        public float PositionLerp => _positionLerp;

        /// <summary>Per-second rotation smoothing coefficient for framerate-independent Slerp.</summary>
        public float RotationLerp => _rotationLerp;

        /// <summary>When true the camera continuously rotates to look at the target.</summary>
        public bool LookAtTarget => _lookAtTarget;

        /// <summary>How long a change of camera eases over, in seconds.</summary>
        public float SwitchSeconds => _switchSeconds;

        /// <summary>The most the camera shakes, each way, in metres.</summary>
        public float ShakeMost => _shakeMost;

        /// <summary>How fast a shake dies away, per second.</summary>
        public float ShakeDecay => _shakeDecay;

        /// <summary>How quickly the shake moves, in Hz.</summary>
        public float ShakeFrequency => _shakeFrequency;

        /// <summary>The shake of the followed ship's own volley, in metres.</summary>
        public float VolleyShake => _volleyShake;

        /// <summary>The shake of a hit on the followed ship, per share of health it takes, in metres.</summary>
        public float HitShakePerHealth => _hitShakePerHealth;
    }
}
