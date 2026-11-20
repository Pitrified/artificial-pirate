namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// A broken mast going over the side: it tips about its foot, faster as it goes, then sinks
    /// and is removed. Made by <see cref="ShipMasts"/> around the mast's objects when it breaks.
    /// </summary>
    public sealed class FallingMast : MonoBehaviour
    {
        private Quaternion _upright;
        private Vector3 _axis;
        private float _angleDeg;
        private float _fallSeconds;
        private float _sinkSpeed;
        private float _sinkSeconds;
        private float _age;

        /// <summary>Whether it has sunk and is to be removed.</summary>
        public bool Gone => _age >= _fallSeconds + _sinkSeconds;

        /// <summary>
        /// Starts the fall from where it stands, tipping by <paramref name="angleDeg"/> about the
        /// world <paramref name="axis"/> through its foot (this object's position).
        /// </summary>
        public void Begin(Vector3 axis, float angleDeg, float fallSeconds, float sinkSpeed, float sinkSeconds)
        {
            _upright = transform.rotation;
            _axis = axis;
            _angleDeg = angleDeg;
            _fallSeconds = Mathf.Max(fallSeconds, Mathf.Epsilon);
            _sinkSpeed = sinkSpeed;
            _sinkSeconds = sinkSeconds;
            _age = 0f;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
            if (Gone)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Moves the fall on by <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            _age += dt;
            float fallen = Mathf.Clamp01(_age / _fallSeconds);
            transform.rotation = Quaternion.AngleAxis(_angleDeg * fallen * fallen, _axis) * _upright;
            if (_age > _fallSeconds)
            {
                transform.position += Vector3.down * (_sinkSpeed * dt);
            }
        }
    }
}
