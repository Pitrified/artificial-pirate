namespace Game.Gameplay.World
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Builds an island's mesh from <see cref="WorldGenerator.IslandHeight"/>: a square grid over
    /// the island's reach, in the island's local space (its centre at the origin, the waterline at
    /// 0). Quads wholly in deep water are left out. Two submeshes: sand below
    /// <see cref="SandBelow"/> and grass above, by each quad's mean height. UVs are world metres.
    /// The buffers are kept, so a build allocates nothing.
    /// </summary>
    public sealed class IslandMeshBuilder
    {
        /// <summary>Quads along a side, the same for every island.</summary>
        public const int Quads = 48;

        /// <summary>Submesh order, matching the renderer's materials.</summary>
        public const int SandSubmesh = 0;

        /// <summary>The grass submesh.</summary>
        public const int GrassSubmesh = 1;

        private const int Side = Quads + 1;
        private const float SandBelow = 0.8f; // m above the waterline: a beach
        private const float DeepShare = 0.5f; // a quad deeper than this share of the seabed all round is not drawn

        private readonly Vector3[] _vertices = new Vector3[Side * Side];
        private readonly Vector2[] _uvs = new Vector2[Side * Side];
        private readonly List<int> _sand = new List<int>(Quads * Quads * 6);
        private readonly List<int> _grass = new List<int>(Quads * Quads * 6);

        /// <summary>Fills <paramref name="mesh"/> with the island at <paramref name="site"/>.</summary>
        public void Build(WorldGenerator world, in IslandSite site, Mesh mesh)
        {
            float reach = world.Settings.Reach(site.Radius);
            float step = 2f * reach / Quads;
            float deep = world.Settings.Seabed * DeepShare;

            for (int row = 0; row < Side; row++)
            {
                for (int column = 0; column < Side; column++)
                {
                    float localX = column * step - reach;
                    float localZ = row * step - reach;
                    float x = site.Centre.x + localX;
                    float z = site.Centre.y + localZ;
                    int i = row * Side + column;
                    _vertices[i] = new Vector3(localX, world.IslandHeight(site, x, z), localZ);
                    _uvs[i] = new Vector2(x, z);
                }
            }

            _sand.Clear();
            _grass.Clear();
            for (int row = 0; row < Quads; row++)
            {
                for (int column = 0; column < Quads; column++)
                {
                    int a = row * Side + column;
                    int b = a + Side;
                    int c = a + 1;
                    int d = b + 1;
                    float ha = _vertices[a].y;
                    float hb = _vertices[b].y;
                    float hc = _vertices[c].y;
                    float hd = _vertices[d].y;
                    if (ha < deep && hb < deep && hc < deep && hd < deep)
                    {
                        continue;
                    }
                    List<int> into = (ha + hb + hc + hd) * 0.25f < SandBelow ? _sand : _grass;
                    into.Add(a);
                    into.Add(b);
                    into.Add(c);
                    into.Add(c);
                    into.Add(b);
                    into.Add(d);
                }
            }

            mesh.Clear();
            mesh.SetVertices(_vertices);
            mesh.SetUVs(0, _uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(_sand, SandSubmesh);
            mesh.SetTriangles(_grass, GrassSubmesh);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
