using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

public static class TerrainGenerator
{
    static Queue<Chunk.ChunkCallback<Chunk>> chunkThreadCallbackQueue = new Queue<Chunk.ChunkCallback<Chunk>>();

    static System.Random random = new System.Random();

    public static void Update()
    {
        if (chunkThreadCallbackQueue.Count > 0)
        {
            chunkThreadCallbackQueue.Dequeue().Invoke();
        }
    }

    // Generate the terrain for a chunk
    public static void RequestChunkTerrainGeneration(Chunk chunk,Action<Chunk> callback)
    {
        ThreadStart threadStart = delegate
        {
            GenerateChunkTerrainThread(chunk, callback);
        };
        new Thread(threadStart).Start();
    }


    static void GenerateChunkTerrainThread(Chunk chunk, Action<Chunk> callback)
    {
        Vector3Int chunkPos = chunk.chunkPos;

        int cs = Chunk.CHUNK_SIZE;
        // Loop through chunk
        for (int x = 0; x < cs; x++)
        {
            for (int y = 0; y < cs; y++)
            {
                for (int z = 0; z < cs; z++)
                {
                    BlockPos blockPosInChunk = new BlockPos(x, y, z);
                    BlockPos blockPos = chunk.Chunk2World(blockPosInChunk);

                    Vector3 sampler = (Vector3)blockPos * .05f;

                    int h = (int)(Mathf.PerlinNoise(sampler.x, sampler.z) * 20);

                    if (h == blockPos.y)
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.GRASS);
                    }
                    else if (blockPos.y < h)
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.DIRT);
                    }
                    else
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.AIR);
                    }

                }
            }
        }
        lock (chunkThreadCallbackQueue)
        {
            chunkThreadCallbackQueue.Enqueue(new Chunk.ChunkCallback<Chunk>(callback, chunk));
        }
    }
}
