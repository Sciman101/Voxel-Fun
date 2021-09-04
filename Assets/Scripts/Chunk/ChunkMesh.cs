using System.Collections.Generic;
using UnityEngine;

public class ChunkMesh
{
    readonly Mesh mesh;

    public readonly List<Vector3> vertices;
    public readonly List<int> triangles;
    public readonly List<Vector2> uvs;

    public ChunkMesh(MeshFilter filter)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        filter.sharedMesh = mesh;

        vertices = new List<Vector3>(200000);
        triangles = new List<int>(200000*3);
        uvs = new List<Vector2>(200000);
    }

    public void Upload()
    {
        mesh.Clear();

        int submeshCount = (vertices.Count / 65535) + 1;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

}
