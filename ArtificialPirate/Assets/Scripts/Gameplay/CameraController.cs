namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// Third-person chase camera. Follows a target Transform from an offset in the target's local
    /// space and looks at a point in it, using framerate-independent smoothing. Attach to the Game
    /// scene's MainCamera.
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private SO_CameraTuning _tuning;

        private Transform _transform;
        private Transform _target;
        private Vector3 _offset;
        private Vector3 _lookAt;
        private bool _nullWarningLogged;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _transform = transform;
        }

        private void LateUpdate()
        {
            Tick(Time.deltaTime);
        }

        // ------------------------------------------------------------------ public API

        /// <summary>
        /// Sets the Transform the camera will follow, from <paramref name="offset"/> and looking at
        /// <paramref name="lookAt"/>, both in its local space.
        /// Call this after the Game scene loads and the ship is ready.
        /// Pass <c>null</c> to detach.
        /// </summary>
        public void SetTarget(Transform target, Vector3 offset, Vector3 lookAt)
        {
            _target = target;
            _offset = offset;
            _lookAt = lookAt;
            _nullWarningLogged = false;
        }

        /// <summary>
        /// Instantly places the camera at the desired offset from the current target.
        /// Call this immediately after a scene load or Teleport to avoid a visible snap.
        /// </summary>
        public void SnapToTarget()
        {
            if (_transform == null)
                _transform = transform;

            if (_target == null)
            {
                if (!_nullWarningLogged)
                {
                    Log.Warn(LogCat.Gameplay, "CameraController.SnapToTarget: target is null");
                    _nullWarningLogged = true;
                }
                return;
            }

            _nullWarningLogged = false;

            _transform.position = _target.TransformPoint(_offset);

            if (_tuning == null || _tuning.LookAtTarget)
                _transform.LookAt(_target.TransformPoint(_lookAt));
        }

        // ------------------------------------------------------------------ internal (testable)

        /// <summary>
        /// Applies one LateUpdate step of <paramref name="dt"/> seconds.
        /// Called by <see cref="LateUpdate"/> with <see cref="Time.deltaTime"/>;
        /// exposed internally for EditMode tests.
        /// </summary>
        internal void Tick(float dt)
        {
            if (_transform == null)
                _transform = transform;

            if (_tuning == null)
                return;

            if (_target == null)
            {
                if (!_nullWarningLogged)
                {
                    Log.Warn(LogCat.Gameplay, "CameraController: target is null, holding last position");
                    _nullWarningLogged = true;
                }
                return;
            }

            _nullWarningLogged = false;

            Vector3 desiredPos = _target.TransformPoint(_offset);
            float tPos = 1f - Mathf.Exp(-_tuning.PositionLerp * dt);
            _transform.position = Vector3.Lerp(_transform.position, desiredPos, tPos);

            if (_tuning.LookAtTarget)
            {
                Vector3 lookDir = _target.TransformPoint(_lookAt) - _transform.position;
                if (lookDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion desiredRot = Quaternion.LookRotation(lookDir);
                    float tRot = 1f - Mathf.Exp(-_tuning.RotationLerp * dt);
                    _transform.rotation = Quaternion.Slerp(_transform.rotation, desiredRot, tRot);
                }
            }
        }
    }
}
