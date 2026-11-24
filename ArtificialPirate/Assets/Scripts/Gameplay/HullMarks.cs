namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Marks on the hull where it was hit: one quad for each of <see cref="ShipDamage"/>'s kept hits,
    /// a splintered patch or, for a big hit, a hole, sized by the hit. They sit on the hull's side
    /// above the water (the type's <see cref="SO_ShipType.HalfBeamAt"/>), between the waterline and
    /// the rail, facing out. All the marks are one mesh, rebuilt when the hull is damaged or
    /// restored, and not drawn while there are none. Lives on a child of the hull, so the marks ride with it.
    /// See docs/systems/ship-controller.md.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class HullMarks : MonoBehaviour
    {
        private const int Corners = 4;
        private const float Cell = 0.5f;
        private const float SlopeStep = 0.25f;
        private const float VariantsPerMetre = 7f;

        [Tooltip("The hull whose hits are drawn.")]
        [SerializeField] private ShipDamage _damage;

        [Tooltip("A mark's side for the smallest hit (m).")]
        [SerializeField] private float _smallSize = 1.2f;

        [Tooltip("A mark's side for a hit of Big Hit or more (m).")]
        [SerializeField] private float _bigSize = 3.5f;

        [Tooltip("The health a hit takes for the biggest mark.")]
        [SerializeField] private float _bigHit = 0.25f;

        [Tooltip("The health a hit takes to leave a hole rather than a patch.")]
        [SerializeField] private float _holeAbove = 0.1f;

        [Tooltip("How far off the hull's side a mark sits (m).")]
        [SerializeField] private float _standOff = 0.04f;

        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _uvs;

        /// <summary>The marks' mesh, in the hull's space, for tests.</summary>
        internal Mesh Mesh => _mesh;

        /// <summary>Wires the hull and draws its hits (marks built in code: tests).</summary>
        internal void Configure(ShipDamage damage)
        {
            Unsubscribe();
            _damage = damage;
            Build();
            Subscribe();
            Rebuild();
        }

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Rebuild();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        private void Build()
        {
            if (_mesh != null)
            {
                return;
            }
            int count = ShipDamage.KeptHits * Corners;
            _vertices = new Vector3[count];
            _normals = new Vector3[count];
            _uvs = new Vector2[count];
            var triangles = new int[ShipDamage.KeptHits * 6];
            for (int i = 0; i < ShipDamage.KeptHits; i++)
            {
                int v = i * Corners;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
            }
            _mesh = new Mesh { name = "HullMarks" };
            _mesh.MarkDynamic();
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(triangles, 0);
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnDamaged(Vector3 point, float amount) => Rebuild();

        /// <summary>Lays a quad on the hull for each kept hit; the rest are folded away.</summary>
        private void Rebuild()
        {
            SO_ShipType type = _damage != null ? _damage.Type : null;
            if (_mesh == null || type == null)
            {
                return;
            }
            int hits = _damage.HitCount;
            for (int i = 0; i < ShipDamage.KeptHits; i++)
            {
                if (i < hits)
                {
                    Place(i, _damage.GetHit(i), type);
                }
                else
                {
                    Fold(i);
                }
            }
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetUVs(0, _uvs);
            _mesh.RecalculateBounds();
            _renderer.enabled = hits > 0;
        }

        private void Place(int index, HullHit hit, SO_ShipType type)
        {
            float size = Mathf.Lerp(_smallSize, _bigSize, Mathf.Clamp01(hit.Amount / _bigHit));
            float half = size * 0.5f;
            float side = hit.Point.x >= 0f ? 1f : -1f;
            float reach = Mathf.Max(type.HalfLength - half, 0f);
            float z = Mathf.Clamp(hit.Point.z, -reach, reach);
            float top = type.Freeboard - half;
            float y = top > half ? Mathf.Clamp(hit.Point.y, half, top) : type.Freeboard * 0.5f;

            float slope = (type.HalfBeamAt(z + SlopeStep) - type.HalfBeamAt(z - SlopeStep)) / (2f * SlopeStep);
            Vector3 normal = new Vector3(side, 0f, -slope).normalized;
            Vector3 centre = new Vector3(side * type.SideAt(z, y), y, z) + normal * _standOff;
            Vector3 right = Vector3.Cross(Vector3.up, -normal) * half;
            Vector3 up = Vector3.up * half;

            int v = index * Corners;
            _vertices[v] = centre - right - up;
            _vertices[v + 1] = centre - right + up;
            _vertices[v + 2] = centre + right + up;
            _vertices[v + 3] = centre + right - up;

            float u0 = Mathf.FloorToInt(Mathf.Abs(hit.Point.z) * VariantsPerMetre) % 2 == 0 ? 0f : Cell;
            float v0 = hit.Amount >= _holeAbove ? Cell : 0f;
            _uvs[v] = new Vector2(u0, v0);
            _uvs[v + 1] = new Vector2(u0, v0 + Cell);
            _uvs[v + 2] = new Vector2(u0 + Cell, v0 + Cell);
            _uvs[v + 3] = new Vector2(u0 + Cell, v0);

            for (int k = 0; k < Corners; k++)
            {
                _normals[v + k] = normal;
            }
        }

        private void Fold(int index)
        {
            int v = index * Corners;
            for (int k = 0; k < Corners; k++)
            {
                _vertices[v + k] = Vector3.zero;
                _normals[v + k] = Vector3.up;
                _uvs[v + k] = Vector2.zero;
            }
        }

        private void Subscribe()
        {
            if (_damage != null)
            {
                Unsubscribe();
                _damage.Damaged += OnDamaged;
                _damage.Restored += Rebuild;
            }
        }

        private void Unsubscribe()
        {
            if (_damage != null)
            {
                _damage.Damaged -= OnDamaged;
                _damage.Restored -= Rebuild;
            }
        }
    }
}
