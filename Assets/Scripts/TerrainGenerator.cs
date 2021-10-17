using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public static class TerrainGenerator
{
    static Queue<Chunk.ChunkCallback<Chunk>> chunkThreadCallbackQueue = new Queue<Chunk.ChunkCallback<Chunk>>();

    static System.Random random = new System.Random();

    static int WATERLEVEL = 8;

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
        Task.Factory.StartNew(() => GenerateChunkTerrainThread(chunk, callback));
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

                    chunk.SetBlock(blockPosInChunk, Blocks.AIR);

                    int h = (int)(Mathf.PerlinNoise(sampler.x, sampler.z) * 20);
                    int h2 = (int)(Mathf.PerlinNoise(sampler.x+0.434384f, sampler.z+0.2389349f) * 20);

                    if (blockPos.y < WATERLEVEL)
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.WATER);
                    }
                    if (h == blockPos.y)
                    {
                        chunk.SetBlock(blockPosInChunk, blockPos.y < WATERLEVEL-1 ? Blocks.STONE : Blocks.GRASS);
                    }
                    else if (h == blockPos.y - 1)
                    {
                        lock (random)
                        {
                            if (random.NextDouble() < .01)
                            {
                               chunk.SetBlock(blockPosInChunk, Blocks.FLOWER);
                            }
                            else if (random.NextDouble() < .05)
                            {
                                chunk.SetBlock(blockPosInChunk, Blocks.GRASS_PLANT);
                            }
                        }
                    }
                    else if (blockPos.y < h)
                    {
                        if (blockPos.y <= h2)
                        {
                            chunk.SetBlock(blockPosInChunk, Blocks.STONE);
                        }
                        else
                        {
                            chunk.SetBlock(blockPosInChunk, Blocks.DIRT);
                        }
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
