using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public static class ChunkMeshGenerator
{

    // List of triangles/vertices
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Vector2> uvs = new List<Vector2>();

    private static Vector2Int[] CORNER_OFFSETS = new Vector2Int[] {
        new Vector2Int(-1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(1,1),
        new Vector2Int(1,-1),
    };

    static Array faces = Enum.GetValues(typeof(BlockFace));

    public static void GenerateMesh(Chunk chunk)
    {
        // Reset arrays
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        BlockPos chunkOffset = chunk.chunkPos * Chunk.CHUNK_SIZE;

        // Loop through chunk
        foreach (BlockPos pos in BlockPos.BlocksInVolume(BlockPos.zero, BlockPos.one * Chunk.CHUNK_SIZE))
        {
            // Only generate faces for blocks that exist
            Block b = chunk.GetBlock(pos);
            if (b != null && b != Blocks.AIR)
            {
                // Check adjacent faces
                foreach (BlockFace face in faces)
                {
                    Block b1 = World.instance.GetBlock(pos.offset(face)+chunkOffset);
                    if (b1 == null || b1.IsTransparent()) {
                        GenerateFace(pos, face, vertices, triangles, uvs);
                    }
                }
            }
        }
        // Set data
        chunk.SetMeshData(vertices, triangles, uvs);
    }

    // Add a face to the mesh
    // TODO split this into like 6 different face additions
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
