﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshGenerator
{
    // List of triangles/vertices
    static List<Vector3> vertices = new List<Vector3>(30000);
    static List<int> triangles = new List<int>(150000);
    static List<Vector2> uvs = new List<Vector2>(30000);

    static BlockFace[] faces = (BlockFace[])Enum.GetValues(typeof(BlockFace));

    // Uv offsets
    private static readonly Vector2 UV_RIGHT = new Vector2(1f/16,0);
    private static readonly Vector2 UV_DOWN = new Vector2(0,1f / 16);
    private static readonly Vector2 UV_CORNER = UV_RIGHT + UV_DOWN;

    public static void GenerateMesh(Chunk chunk)
    {
        // Reset arrays
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        int cs = Chunk.CHUNK_SIZE;
        BlockPos chunkOffset = chunk.chunkPos * cs;

        BlockPos chunkPos = new BlockPos();
        // Loop through chunk
        for (int x = 0; x < cs; x++)
        {
            chunkPos.x = x;
            for (int y = 0; y < cs; y++)
            {
                chunkPos.y = y;
                for (int z = 0; z < cs; z++)
                {
                    chunkPos.z = z;
                    // Only generate faces for blocks that exist
                    Block block = chunk.GetBlock(chunkPos);
                    if (block != null && block.HasMesh())
                    {
                        // Check adjacent faces
                        for (int f=0;f<6;f++) 
                        {
                            BlockFace face = faces[f];
                            Block adjacentBlock = World.instance.GetBlock(chunkPos.offset(face) + chunkOffset);
                            if (adjacentBlock == null || adjacentBlock.IsTransparent())
                            {
                                GenerateFace((Vector3)chunkPos, face, block);
                            }
                        }
                    }
                }
            }
        }
        // Set data
        chunk.SetMeshData(vertices, triangles, uvs);
    }

    // Add a face to the mesh
    // TODO split this into like 6 different face additions
    private static void GenerateFace(Vector3 chunkPos, BlockFace face, Block block)
    {

        int t = vertices.Count;
        Vector2 uvCorner = block.GetFaceTextureCoord(face);
        switch (face)
        {

            case BlockFace.TOP:
                vertices.Add(chunkPos+new Vector3(0,1,0));
                vertices.Add(chunkPos+new Vector3(0,1,1));
                vertices.Add(chunkPos+new Vector3(1,1,1));
                vertices.Add(chunkPos+new Vector3(1,1,0));
                break;

            case BlockFace.BOTTOM:  
                vertices.Add(chunkPos + new Vector3(1, 0, 0));
                vertices.Add(chunkPos + new Vector3(1, 0, 1));
                vertices.Add(chunkPos + new Vector3(0, 0, 1));
                vertices.Add(chunkPos + new Vector3(0, 0, 0));
                break;

            case BlockFace.NORTH:
                vertices.Add(chunkPos + new Vector3(1, 0, 1));
                vertices.Add(chunkPos + new Vector3(1, 1, 1));
                vertices.Add(chunkPos + new Vector3(0, 1, 1));
                vertices.Add(chunkPos + new Vector3(0, 0, 1));
                break;

            case BlockFace.SOUTH:
                vertices.Add(chunkPos + new Vector3(0, 0, 0));
                vertices.Add(chunkPos + new Vector3(0, 1, 0));
                vertices.Add(chunkPos + new Vector3(1, 1, 0));
                vertices.Add(chunkPos + new Vector3(1, 0, 0));
                break;

            case BlockFace.EAST:
                vertices.Add(chunkPos + new Vector3(1, 0, 0));
                vertices.Add(chunkPos + new Vector3(1, 1, 0));
                vertices.Add(chunkPos + new Vector3(1, 1, 1));
                vertices.Add(chunkPos + new Vector3(1, 0, 1));
                break;

            case BlockFace.WEST:
                vertices.Add(chunkPos + new Vector3(0, 0, 1));
                vertices.Add(chunkPos + new Vector3(0, 1, 1));
                vertices.Add(chunkPos + new Vector3(0, 1, 0));
                vertices.Add(chunkPos + new Vector3(0, 0, 0));
                break;
        }
        // Add triangles
        triangles.Add(t+1);
        triangles.Add(t+3);
        triangles.Add(t);

        triangles.Add(t+3);
        triangles.Add(t+1);
        triangles.Add(t+2);

        // Add UVS
        uvs.Add(uvCorner+UV_DOWN);
        uvs.Add(uvCorner);
        uvs.Add(uvCorner+UV_RIGHT);
        uvs.Add(uvCorner+UV_CORNER);
    }
}