using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Mesh assets for grey-box geometry. Boxes get UVs in world units, so a texture tiles at the
    /// same density on a 0.5 m crate and a 30 m hangar instead of stretching like a scaled cube.
    /// </summary>
    public static class GreyboxMeshes
    {
        const string Folder = GreyboxKit.Root + "/Generated/Meshes";
        /// <summary>Metres covered by one repeat of a texture.</summary>
        const float TileMetres = 2f;

        static readonly Dictionary<Vector3Int, Mesh> Boxes = new Dictionary<Vector3Int, Mesh>();

        public static void ClearCache() => Boxes.Clear();

        public static Mesh Box(Vector3 size)
        {
            var key = Vector3Int.RoundToInt(size * 1000f);
            if (Boxes.TryGetValue(key, out var cached) && cached != null) return cached;

            string path = $"{Folder}/Box_{key.x}x{key.y}x{key.z}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                GreyboxKit.EnsureFolder(GreyboxKit.Root + "/Generated", "Meshes");
                mesh = BuildBox(size);
                mesh.name = $"Box_{key.x}x{key.y}x{key.z}";
                AssetDatabase.CreateAsset(mesh, path);
            }
            Boxes[key] = mesh;
            return mesh;
        }

        static Mesh BuildBox(Vector3 size)
        {
            Vector3 h = size * 0.5f;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // Each face: normal, and the two in-plane axes (u, v) with their lengths.
            void Face(Vector3 normal, Vector3 u, Vector3 v, float uLength, float vLength)
            {
                int start = vertices.Count;
                Vector3 centre = Vector3.Scale(normal, h);
                for (int i = 0; i < 4; i++)
                {
                    float su = (i == 1 || i == 2) ? 0.5f : -0.5f;
                    float sv = (i >= 2) ? 0.5f : -0.5f;
                    vertices.Add(centre + u * (su * uLength) + v * (sv * vLength));
                    normals.Add(normal);
                    uvs.Add(new Vector2((su + 0.5f) * uLength, (sv + 0.5f) * vLength) / TileMetres);
                }
                triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }

            Face(Vector3.forward, Vector3.left, Vector3.up, size.x, size.y);
            Face(Vector3.back, Vector3.right, Vector3.up, size.x, size.y);
            Face(Vector3.right, Vector3.forward, Vector3.up, size.z, size.y);
            Face(Vector3.left, Vector3.back, Vector3.up, size.z, size.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z);
            Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z);

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>A low-poly pine: trunk on submesh 0, two stacked foliage cones on submesh 1.</summary>
        public static Mesh Pine()
        {
            string path = $"{Folder}/Pine.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            GreyboxKit.EnsureFolder(GreyboxKit.Root + "/Generated", "Meshes");

            var vertices = new List<Vector3>();
            var trunk = new List<int>();
            var foliage = new List<int>();
            Cone(vertices, trunk, 0f, 2.2f, 0.25f, 0.18f, 6);
            Cone(vertices, foliage, 1.2f, 6.5f, 2.2f, 0f, 8);
            Cone(vertices, foliage, 4f, 9f, 1.5f, 0f, 8);

            mesh = new Mesh { name = "Pine", subMeshCount = 2 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(trunk, 0);
            mesh.SetTriangles(foliage, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        /// <summary>A flat-shaded frustum (cone when topRadius is 0) with its own vertices per face.</summary>
        static void Cone(List<Vector3> vertices, List<int> triangles, float bottom, float top, float bottomRadius,
            float topRadius, int sides)
        {
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                var b0 = new Vector3(Mathf.Cos(a0) * bottomRadius, bottom, Mathf.Sin(a0) * bottomRadius);
                var b1 = new Vector3(Mathf.Cos(a1) * bottomRadius, bottom, Mathf.Sin(a1) * bottomRadius);
                var t0 = new Vector3(Mathf.Cos(a0) * topRadius, top, Mathf.Sin(a0) * topRadius);
                var t1 = new Vector3(Mathf.Cos(a1) * topRadius, top, Mathf.Sin(a1) * topRadius);
                int s = vertices.Count;
                vertices.AddRange(new[] { b0, b1, t1, t0 });
                triangles.AddRange(new[] { s, s + 2, s + 1, s, s + 3, s + 2 });
            }
        }
    }
}
