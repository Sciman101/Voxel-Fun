using UnityEngine;

public static class TerrainGenerator
{
    // Generate the terrain for a chunk
    public static void GenerateChunkTerrain(Chunk chunk)
    {
        Vector3Int chunkPos = chunk.chunkPos;
        if (chunkPos.y > 2) return; // Skip air stuff

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

                    float AB = Mathf.PerlinNoise(sampler.x,sampler.y);
                    float BC = Mathf.PerlinNoise(sampler.y,sampler.z);
                    float AC = Mathf.PerlinNoise(sampler.x,sampler.z);

                    float BA = Mathf.PerlinNoise(sampler.y, sampler.x);
                    float CB = Mathf.PerlinNoise(sampler.z, sampler.y);
                    float CA = Mathf.PerlinNoise(sampler.z, sampler.x);

                    float avg = (AB + BC + AC + BA + CB + CA) / 6;

                    bool fill = avg > 0f;

                    if (fill)
                    {
                        chunk.SetBlock(blockPosInChunk, Blocks.STONE);
                    }

                }
            }
        }
    }
}
