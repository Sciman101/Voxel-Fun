using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshGenerator
{
    static BlockFace[] faces = (BlockFace[])Enum.GetValues(typeof(BlockFace));
    // Uv offsets
    private static readonly Vector2 UV_RIGHT = new Vector2(1f/16,0);
    private static readonly Vector2 UV_UP = new Vector2(0,1f / 16);
    private static readonly Vector2 UV_CORNER = UV_RIGHT + UV_UP;

    static Queue<Chunk.ChunkCallback<Chunk>> chunkThreadCallbackQueue = new Queue<Chunk.ChunkCallback<Chunk>>();

    public static void Update()
    {
        if (chunkThreadCallbackQueue.Count > 0)
        {
            chunkThreadCallbackQueue.Dequeue().Invoke();
        }
    }

    public static void RequestChunkMesh(Chunk chunk, Action<Chunk> callback)
    {
        ThreadStart threadStart = delegate
        {
            GenerateMeshThread(chunk, callback);
        };
        new Thread(threadStart).Start();
    }

    public static void GenerateMeshThread(Chunk chunk, Action<Chunk> callback)
    {
        chunk.isEmpty = true;
        ChunkMesh mesh = chunk.mesh;

        // Reset arrays
        mesh.vertices.Clear();
        mesh.triangles.Clear();
        mesh.uvs.Clear();

        int cs = Chunk.CHUNK_SIZE;

        // Loop through chunk
        for (int x = 0; x < cs; x++)
        {
            for (int y = 0; y < cs; y++)
            {
                for (int z = 0; z < cs; z++)
                {
                    BlockPos blockInChunkPos = new BlockPos(x,y,z);
                    // Only generate faces for blocks that exist
                    Block block = chunk.GetBlock(blockInChunkPos);
                    if (block != null && block.HasMesh())
                    {
                        chunk.isEmpty = false;
                        // Handle full cube rendering
                        if (block.IsFullCube())
                        {
                            // Check adjacent faces
                            for (int f = 0; f < 6; f++)
                            {
                                BlockFace face = faces[f];
                                Block adjacentBlock = World.instance.GetBlock(chunk.Chunk2World(blockInChunkPos).offset(face));
                                if (adjacentBlock == null || adjacentBlock.IsTransparent())
                                {
                                    GenerateFace((Vector3)blockInChunkPos, face, block, mesh);
                                }
                            }
                        }
                        else
                        {
                            // Add a custom mesh
                            block.GenerateCustomMesh(chunk.Chunk2World(blockInChunkPos), (Vector3)blockInChunkPos, mesh.vertices,mesh.triangles,mesh.uvs);
                        }
                    }
                }
            }
        }

        lock(chunkThreadCallbackQueue)
        {
            chunkThreadCallbackQueue.Enqueue(new Chunk.ChunkCallback<Chunk>(callback,chunk));
        }
    }

    // Add a face to the mesh
    // TODO split this into like 6 different face additions
    private static void GenerateFace(Vector3 chunkPos, BlockFace face, Block block, ChunkMesh mesh)
    {

        int t = mesh.vertices.Count;
        Vector2 uvCorner = block.GetFaceTextureCoord(face);
        switch (face)
        {
            case BlockFace.TOP:
                mesh.vertices.Add(chunkPos+new Vector3(0,1,0));
                mesh.vertices.Add(chunkPos+new Vector3(0,1,1));
                mesh.vertices.Add(chunkPos+new Vector3(1,1,1));
                mesh.vertices.Add(chunkPos+new Vector3(1,1,0));
                break;

            case BlockFace.BOTTOM:
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 0));
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 0));
                break;

            case BlockFace.NORTH:
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 1));
                mesh.vertices.Add(chunkPos + new Vector3(1, 1, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 1, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 1));
                break;

            case BlockFace.SOUTH:
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 0));
                mesh.vertices.Add(chunkPos + new Vector3(0, 1, 0));
                mesh.vertices.Add(chunkPos + new Vector3(1, 1, 0));
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 0));
                break;

            case BlockFace.EAST:
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 0));
                mesh.vertices.Add(chunkPos + new Vector3(1, 1, 0));
                mesh.vertices.Add(chunkPos + new Vector3(1, 1, 1));
                mesh.vertices.Add(chunkPos + new Vector3(1, 0, 1));
                break;

            case BlockFace.WEST:
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 1, 1));
                mesh.vertices.Add(chunkPos + new Vector3(0, 1, 0));
                mesh.vertices.Add(chunkPos + new Vector3(0, 0, 0));
                break;
        }
        // Add triangles
        mesh.triangles.Add(t+1);
        mesh.triangles.Add(t+3);
        mesh.triangles.Add(t);

        mesh.triangles.Add(t+3);
        mesh.triangles.Add(t+1);
        mesh.triangles.Add(t+2);

        // Add UVS
        mesh.uvs.Add(uvCorner);
        mesh.uvs.Add(uvCorner+UV_UP);
        mesh.uvs.Add(uvCorner+UV_CORNER);
        mesh.uvs.Add(uvCorner+UV_RIGHT);
    }
}
