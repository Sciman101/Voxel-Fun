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

    static Queue<ThreadCallbackInfo<Chunk>> chunkThreadCallbackQueue = new Queue<ThreadCallbackInfo<Chunk>>();

    public static void Update()
    {
        while (chunkThreadCallbackQueue.Count > 0)
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
                        // Check adjacent faces
                        for (int f=0;f<6;f++) 
                        {
                            BlockFace face = faces[f];
                            BlockPos sjfksd = chunk.Chunk2World(blockInChunkPos).offset(face);
                            Block adjacentBlock = World.instance.GetBlock(sjfksd);
                            if (adjacentBlock == null || adjacentBlock.IsTransparent())
                            {
                                GenerateFace((Vector3)blockInChunkPos, face, block, mesh);
                            }
                        }
                    }
                }
            }
        }

        lock(chunkThreadCallbackQueue)
        {
            chunkThreadCallbackQueue.Enqueue(new ThreadCallbackInfo<Chunk>(callback,chunk));
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


    private readonly struct ThreadCallbackInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;
        public ThreadCallbackInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

        public void Invoke()
        {
            callback(parameter);
        }
    }
}
