using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    private static Vector3Int[] DIRECTIONS = new Vector3Int[]
    {
        Vector3Int.right,
        new Vector3Int(0,0,1),
        Vector3Int.left,
        new Vector3Int(0,0,-1),
        Vector3Int.up,
        Vector3Int.down
    };
    private static Vector2Int[] CORNER_OFFSETS = new Vector2Int[] {
        new Vector2Int(-1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(1,1),
        new Vector2Int(1,-1),
    };

    // How big is a chunk on a side?
    // Keeping this at 16 due to mesh data limits
    // Maybe make blocks share common vertices?
    public const int CHUNK_SIZE = 16;

    public Vector3Int chunkPos;

    // Mesh display
    new MeshRenderer renderer;
    MeshFilter filter;
    MeshCollider collider;

    private Mesh mesh;

    // Block data
    byte[] blocks = new byte[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

    private void Start()
    {
        // Get components
        renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        // Setup mesh
        filter.mesh = mesh = new Mesh();
        mesh.name = "Chunk";

        GenerateBlocks();
    }


    // Generate an intresting structure
    void GenerateBlocks()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Vector3 p = chunkPos * CHUNK_SIZE + new Vector3(x, y, z);
                    SetBlock(new Vector3Int(x, y, z), (byte)((
                        p.y <= 0 ||
                        Mathf.PerlinNoise(p.x/(float)CHUNK_SIZE,p.z/(float)CHUNK_SIZE)*CHUNK_SIZE >= p.y) ? 1 : 0));
                }
            }
        }
        // Create a mesh from the result
        GenerateMesh();
    }

    #region Block Handling
    // Make sure a position exists in the chunk
    bool ValidatePos(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
            pos.x < CHUNK_SIZE && pos.y < CHUNK_SIZE && pos.z < CHUNK_SIZE;
    }

    // Get a block at a position
    public byte GetBlock(Vector3Int pos)
    {
        if (ValidatePos(pos))
        {
            return blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE];
        }
        else
        {
            return 0; // Empty block
        }
    }

    public byte GetBlock(Vector3 pos)
    {
        return GetBlock(new Vector3Int((int)pos.x,(int)pos.y,(int)pos.z));
    }

    // Set a block at a position
    public void SetBlock(Vector3Int pos, byte block)
    {
        if (ValidatePos(pos))
        {
            blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE] = block;
        }
    }
    #endregion

    #region Mesh Generation

    public void GenerateMesh()
    {
        // List of triangles/vertices
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Loop through chunk
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    // Only generate faces for blocks that exist
                    if (GetBlock(pos) != 0)
                    {
                        // Check adjacent faces
                        foreach (Vector3Int dir in DIRECTIONS)
                        {
                            if (GetBlock(pos + dir) == 0)
                            {
                                GenerateFace(pos,dir,vertices,triangles,uvs);
                            }
                        }
                    }
                }
            }
        }
        // Set data
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles,0);
        mesh.SetUVs(0,uvs);
        mesh.RecalculateNormals();

        collider.sharedMesh = mesh;
    }

    // Add a face to the mesh
    private void GenerateFace(Vector3Int pos, Vector3Int dir, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        // Create vertices from direction
        for (int i=0;i<4;i++)
        {
            Vector3 vOffset = dir;
            Vector2 corners = CORNER_OFFSETS[i];
            int c = 0;
            for (int j=0;j<3;j++)
            {
                if (vOffset[j] == 0)
                {
                    vOffset[j] = corners[c++];
                }
            }
            // Actually put mesh values in place
            vertices.Add(pos + (vOffset + Vector3.one) * 0.5f);
        }

        uvs.Add(Vector2.right);
        uvs.Add(Vector2.one);
        uvs.Add(Vector2.up);
        uvs.Add(Vector2.zero);


        // Generate triangles
        int t = vertices.Count-4;
        if (dir.y == 1 || dir.x == -1 || dir.z == -1)
        {
            triangles.Add(t);
            triangles.Add(t + 1);
            triangles.Add(t+3);

            triangles.Add(t + 1);
            triangles.Add(t + 2);
            triangles.Add(t + 3);
        }
        else
        {
            triangles.Add(t + 3);
            triangles.Add(t + 1);
            triangles.Add(t);

            triangles.Add(t + 3);
            triangles.Add(t + 2);
            triangles.Add(t + 1);
        }
    }


    #endregion
}
