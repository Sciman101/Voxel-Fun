using UnityEngine;

public static class TerrainGenerator
{
    // Generate the terrain for a chunk
    public static void GenerateChunkTerrain(Chunk chunk)
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

                    int h = (int)(Mathf.PerlinNoise(sampler.x,sampler.z)*20);

                    if (h == blockPos.y)
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.GRASS);
                    }else if (blockPos.y < h)
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
    }
}
