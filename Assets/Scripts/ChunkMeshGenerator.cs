﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshGenerator
{
    private static Vector2Int[] CORNER_OFFSETS = new Vector2Int[] {
        new Vector2Int(-1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(1,1),
        new Vector2Int(1,-1),
    };

    public static void GenerateMesh(Chunk chunk)
    {
        // List of triangles/vertices
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Loop through chunk
        foreach (BlockPos pos in BlockPos.BlocksInVolume(BlockPos.zero, BlockPos.one * Chunk.CHUNK_SIZE))
        {
            // Only generate faces for blocks that exist
            if (chunk.GetBlock(pos) != 0)
            {
                // Check adjacent faces
                foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
                {
                    if (chunk.GetBlock(pos.offset(face)) == 0)
                    {
                        GenerateFace(pos, face, vertices, triangles, uvs);
                    }
                }
            }
        }
        // Set data
        chunk.SetMeshData(vertices, triangles, uvs);
    }

    // Add a face to the mesh
    private static void GenerateFace(Vector3Int pos, BlockFace face, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        // Create vertices from direction
        for (int i = 0; i < 4; i++)
        {
            // Figure out the vector to use for the face offset
            Vector3 vOffset = (Vector3)BlockPos.zero.offset(face);
            Vector2 corners = CORNER_OFFSETS[i];
            int c = 0;
            for (int j = 0; j < 3; j++)
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
        int t = vertices.Count - 4;
        if (face == BlockFace.TOP || face == BlockFace.SOUTH || face == BlockFace.WEST)
        {
            triangles.Add(t);
            triangles.Add(t + 1);
            triangles.Add(t + 3);

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
}
