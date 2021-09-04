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
        filter.sharedMesh = mesh;

        vertices = new List<Vector3>(30000);
        triangles = new List<int>(150000);
        uvs = new List<Vector2>(30000);
    }

    public void Upload()
    {
        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

}
