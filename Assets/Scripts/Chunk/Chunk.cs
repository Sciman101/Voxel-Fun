﻿using System;
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
        BlockPos chunkPos = new BlockPos();
        // Loop through chunk
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            chunkPos.x = x;
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                chunkPos.y = y;
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    chunkPos.z = z;
                    Vector3 samplePos = (Vector3)(chunkPos + (BlockPos)(chunkPos * CHUNK_SIZE)) * 0.05f;
                    if (Mathf.PerlinNoise(samplePos.x, samplePos.z) > ((float)chunkPos.y) / 16f)
                    {
                        SetBlock(chunkPos, Blocks.DIRT);
                    }
                }
            }
        }
    }

    // Tells the chunk to rebuild it's mesh
    public void RegenerateChunk()
    {
        //Debug.Log("Regenerating chunk " + chunkPos);
        ChunkMeshGenerator.GenerateMesh(this);
    }

    // Take new mesh data and populate our mesh with it
    public void SetMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
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
    public Block GetBlock(BlockPos pos)
    {
        if (ValidatePos(pos))
        {
            return Blocks.FromId(blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE]);
        }
        else
        {
            return null; // Empty block
        }
    }

    public Block GetBlock(Vector3 pos)
    {
        return GetBlock(new BlockPos(pos));
    }

    // Set a block at a position
    public void SetBlock(BlockPos pos, Block block)
    {
        if (ValidatePos(pos))
        {
            blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE] = block.Id;
        }
    }
    #endregion
}