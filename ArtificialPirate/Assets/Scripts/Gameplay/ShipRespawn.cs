namespace Game.Gameplay
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Game.Gameplay.World;
    using Game.Systems;

    /// <summary>
    /// Keeps a ship in the scene: its template, a ship kept inactive in the scene, is copied at the
    /// template's place when the scene starts, and when the copy sinks a whole new one is made there
    /// once the delay has passed and the player's ship is far enough from that place that it does
    /// not appear in sight. A copy keeps the template's references to the scene (the water, the
    /// balls, the effects); each is given the world's ground. Fallen masts are destroyed as they
    /// sink, so a new copy is the only way back to a whole ship. See docs/systems/combat.md.
    /// </summary>
    public sealed class ShipRespawn : MonoBehaviour
    {
        [Tooltip("The ship to copy, kept inactive in the scene.")]
        [SerializeField] private GameObject _template;

        [Tooltip("The player's ship, which must be beyond the clearance for a sunk ship to come back.")]
        [SerializeField] private Transform _player;

        [Tooltip("From the sinking until a new ship can come (s).")]
        [SerializeField] private float _delay = 60f;

        [Tooltip("How far the player's ship must be from the ship's place for it to come back (m).")]
        [SerializeField] private float _clearance = 300f;

        private GameObject _current;
        private ShipDamage _currentDamage;
        private WorldGenerator _ground;
        private bool _waiting;
        private float _sinceSunk;

        /// <summary>The ship now in the scene; null before the first.</summary>
        public GameObject Current => _current;

        /// <summary>How many ships it has made.</summary>
        public int Made { get; private set; }

        /// <summary>Wires it in code (tests).</summary>
        internal void Configure(GameObject template, Transform player, float delay, float clearance)
        {
            _template = template;
            _player = player;
            _delay = delay;
            _clearance = clearance;
        }

        /// <summary>The player's ship, which must be clear of the place for a sunk ship to come back.</summary>
        public void Follow(Transform player) => _player = player;

        /// <summary>Gives every ship it makes the world's ground, so a moving one stops at the shore.</summary>
        public void UseGround(WorldGenerator ground)
        {
            _ground = ground;
            WireGround();
        }

        private void Start()
        {
            if (_current == null)
            {
                Spawn();
            }
        }

        private void OnDestroy()
        {
            if (_currentDamage != null)
            {
                _currentDamage.Sunk -= OnSunk;
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>One step of <paramref name="dt"/> seconds. Exposed internally for tests.</summary>
        internal void Tick(float dt)
        {
            if (!_waiting)
            {
                return;
            }
            _sinceSunk += dt;
            bool clear = _player == null || (_player.position - _template.transform.position).magnitude > _clearance;
            if (_sinceSunk >= _delay && clear)
            {
                Destroy(_current);
                Spawn();
            }
        }

        /// <summary>Makes a new copy of the template at its place. Exposed internally for tests.</summary>
        internal void Spawn()
        {
            if (_template == null)
            {
                Log.Warn(LogCat.Gameplay, "ShipRespawn: {0} has no template", name);
                return;
            }
            if (_currentDamage != null)
            {
                _currentDamage.Sunk -= OnSunk;
            }
            _template.SetActive(false);
            Transform place = _template.transform;
            _current = Instantiate(_template, place.position, place.rotation);
            SceneManager.MoveGameObjectToScene(_current, gameObject.scene);
            _current.name = _template.name;
            _currentDamage = _current.GetComponent<ShipDamage>();
            if (_currentDamage != null)
            {
                _currentDamage.Sunk += OnSunk;
            }
            WireGround();
            _current.SetActive(true);
            _waiting = false;
            Made++;
            Log.Info(LogCat.Gameplay, "ShipRespawn: {0} made at {1}", _current.name, place.position);
        }

        private void OnSunk()
        {
            _waiting = true;
            _sinceSunk = 0f;
        }

        private void WireGround()
        {
            if (_current != null && _ground != null && _current.TryGetComponent(out HullContact contact))
            {
                contact.UseGround(_ground);
            }
        }
    }
}
