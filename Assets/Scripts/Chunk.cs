using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{

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
        BlockPos a = BlockPos.zero;
        BlockPos b = BlockPos.one * CHUNK_SIZE;

        foreach (BlockPos pos in BlockPos.BlocksInVolume(a,b))
        {
            Vector3 p = (chunkPos * CHUNK_SIZE) + (Vector3Int)pos;
            SetBlock(pos, (byte)((
                p.y <= 0 ||
                Mathf.PerlinNoise(p.x / (float)CHUNK_SIZE, p.z / (float)CHUNK_SIZE) * CHUNK_SIZE >= p.y) ? 1 : 0));
        }
        // Create a mesh from the result
        RegenerateChunk();
    }

    // Tells the chunk to rebuild it's mesh
    public void RegenerateChunk()
    {
        ChunkMeshGenerator.GenerateMesh(this);
    }

    // Take new mesh data and populate our mesh with it
    public void SetMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        collider.sharedMesh = mesh;
    }

    #region Block Handling
    // Make sure a position exists in the chunk
    bool ValidatePos(BlockPos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
            pos.x < CHUNK_SIZE && pos.y < CHUNK_SIZE && pos.z < CHUNK_SIZE;
    }

    // Get a block at a position
    public byte GetBlock(BlockPos pos)
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
        return GetBlock(new BlockPos(pos));
    }

    // Set a block at a position
    public void SetBlock(BlockPos pos, byte block)
    {
        if (ValidatePos(pos))
        {
            blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE] = block;
        }
    }
    #endregion
}
