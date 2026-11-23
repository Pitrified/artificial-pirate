namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// The camera that follows the player's ship, from one of the type's rigs (<see cref="CameraView"/>):
    /// the chase behind and above, smoothed; the bird's eye high and astern, smoothed; the helm at the
    /// helmsman's eyes, held fast, steady or riding the hull's pitch and roll. A change of view eases
    /// from where the camera was to the new view over the tuning's switch time. It shakes when told
    /// (<see cref="Shake"/>): an offset that wanders and dies away, laid over the pose. A target set
    /// with a bare offset (<see cref="SetTarget"/>) is followed as a chase. Attach to the Game
    /// scene's MainCamera. See docs/systems/camera-controller.md.
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private SO_CameraTuning _tuning;

        private Transform _transform;
        private Transform _target;
        private Transform _hull;
        private SO_ShipType _type;
        private CameraView _view;
        private bool _helmRides;
        private Vector3 _offset;
        private Vector3 _lookAt;
        private bool _nullWarningLogged;
        private float _shake;
        private float _shakeTime;
        private Vector3 _shakeOffset;
        private Vector3 _followPos;
        private Quaternion _followRot = Quaternion.identity;
        private Vector3 _blendFromPos;
        private Quaternion _blendFromRot = Quaternion.identity;
        private float _blend = 1f;

        /// <summary>The camera's tuning.</summary>
        public SO_CameraTuning Tuning => _tuning;

        /// <summary>The view it shows.</summary>
        public CameraView View => _view;

        /// <summary>The shake's offset laid over the camera's position now, in metres.</summary>
        internal Vector3 ShakeOffset => _shakeOffset;

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
        /// <paramref name="lookAt"/>, both in its local space, as a chase.
        /// Pass <c>null</c> to detach.
        /// </summary>
        public void SetTarget(Transform target, Vector3 offset, Vector3 lookAt)
        {
            _target = target;
            _hull = null;
            _type = null;
            _view = CameraView.Chase;
            _offset = offset;
            _lookAt = lookAt;
            _nullWarningLogged = false;
            FollowFromHere();
        }

        /// <summary>
        /// Follows a ship: its <paramref name="root"/> (the heading), its <paramref name="hull"/>
        /// (the pitch and roll a riding helm camera takes) and its <paramref name="type"/>'s rigs,
        /// showing <paramref name="view"/>.
        /// </summary>
        public void SetShip(Transform root, Transform hull, SO_ShipType type, CameraView view, bool helmRides)
        {
            _target = root;
            _hull = hull;
            _type = type;
            _nullWarningLogged = false;
            UseRig(view, helmRides);
            FollowFromHere();
        }

        /// <summary>Changes the view, easing from where the camera is to the new one.</summary>
        public void SetView(CameraView view, bool helmRides)
        {
            if (_type == null || _target == null || (view == _view && helmRides == _helmRides))
            {
                return;
            }
            _blendFromPos = _transform.position - _shakeOffset;
            _blendFromRot = _transform.rotation;
            _blend = 0f;
            UseRig(view, helmRides);
            Wanted(out _followPos, out _followRot);
        }

        /// <summary>
        /// Shakes the camera by up to <paramref name="strength"/> metres, dying away; a stronger
        /// shake replaces a weaker one, and none exceeds the tuning's most.
        /// </summary>
        public void Shake(float strength)
        {
            float most = _tuning != null ? _tuning.ShakeMost : strength;
            _shake = Mathf.Min(most, Mathf.Max(_shake, strength));
        }

        /// <summary>
        /// Instantly places the camera where its view wants it.
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
            _shakeOffset = Vector3.zero;
            _blend = 1f;
            Wanted(out _followPos, out _followRot);
            _transform.position = _followPos;
            if (_tuning == null || _tuning.LookAtTarget || _view == CameraView.Helm)
                _transform.rotation = _followRot;
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

            Wanted(out Vector3 wantedPos, out Quaternion wantedRot);
            bool turns = _tuning.LookAtTarget || _view == CameraView.Helm;
            if (_view == CameraView.Helm)
            {
                _followPos = wantedPos;
                _followRot = wantedRot;
            }
            else
            {
                float tPos = 1f - Mathf.Exp(-_tuning.PositionLerp * dt);
                _followPos = Vector3.Lerp(_followPos, wantedPos, tPos);
                Vector3 lookDir = Frame().TransformPoint(_lookAt) - _followPos;
                if (turns && lookDir.sqrMagnitude > 0.0001f)
                {
                    float tRot = 1f - Mathf.Exp(-_tuning.RotationLerp * dt);
                    _followRot = Quaternion.Slerp(_followRot, Quaternion.LookRotation(lookDir), tRot);
                }
            }

            Vector3 pos = _followPos;
            Quaternion rot = _followRot;
            if (_blend < 1f)
            {
                _blend = Mathf.Min(1f, _blend + dt / Mathf.Max(_tuning.SwitchSeconds, Mathf.Epsilon));
                float t = Mathf.SmoothStep(0f, 1f, _blend);
                pos = Vector3.Lerp(_blendFromPos, pos, t);
                rot = Quaternion.Slerp(_blendFromRot, rot, t);
            }
            _shakeOffset = NextShake(dt);
            _transform.position = pos + _shakeOffset;
            if (turns)
                _transform.rotation = rot;
        }

        private void UseRig(CameraView view, bool helmRides)
        {
            _view = view;
            _helmRides = helmRides;
            CameraRig rig = _type.Rig(view);
            _offset = rig.Offset;
            _lookAt = rig.LookAt;
        }

        private void FollowFromHere()
        {
            if (_transform == null)
                _transform = transform;
            _followPos = _transform.position - _shakeOffset;
            _followRot = _transform.rotation;
            _blend = 1f;
        }

        /// <summary>What the view is measured from: the hull for a riding helm camera, else the root.</summary>
        private Transform Frame() => _view == CameraView.Helm && _helmRides && _hull != null ? _hull : _target;

        /// <summary>Where the view wants the camera, and its turn when looking at its point.</summary>
        private void Wanted(out Vector3 position, out Quaternion rotation)
        {
            Transform frame = Frame();
            position = frame.TransformPoint(_offset);
            Vector3 look = frame.TransformPoint(_lookAt) - position;
            Vector3 up = frame == _hull ? _hull.up : Vector3.up;
            rotation = look.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(look, up) : _transform.rotation;
        }

        private Vector3 NextShake(float dt)
        {
            _shake *= Mathf.Exp(-_tuning.ShakeDecay * dt);
            if (_shake < 1e-3f)
            {
                _shake = 0f;
                return Vector3.zero;
            }
            _shakeTime += dt * _tuning.ShakeFrequency;
            return new Vector3(Wander(0f), Wander(17f), Wander(41f)) * _shake;
        }

        /// <summary>Smooth noise from -1 to 1 along the shake's time, one lane per axis.</summary>
        private float Wander(float lane) => 2f * Mathf.PerlinNoise(_shakeTime, lane) - 1f;
    }
}
