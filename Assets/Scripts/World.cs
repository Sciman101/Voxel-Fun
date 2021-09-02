﻿using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World instance;

    public GameObject chunkPrefab;

    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    // Used for non-immediate chunk loading
    private HashSet<Vector3Int> chunkPositionsToLoad = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> chunkPositionsToUnload = new HashSet<Vector3Int>();

    private Queue<Chunk> chunksToRegenerate = new Queue<Chunk>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Time measuring
        float start = Time.realtimeSinceStartup;

        LoadChunksAround(Vector3Int.zero,8);

        Debug.Log(string.Format("World generation took {0}s",Time.realtimeSinceStartup-start));
    }


    private void LateUpdate()
    {
        // Unload queued chunks
        if (chunkPositionsToUnload.Count > 0)
        {
            foreach (Vector3Int chunkPos in chunkPositionsToUnload)
                UnloadChunk(chunkPos);
            chunkPositionsToUnload.Clear();
        }

        // Load queued chunks
        if (chunkPositionsToLoad.Count > 0)
        {
            foreach (Vector3Int chunkPos in chunkPositionsToLoad)
                LoadChunk(chunkPos);
            chunkPositionsToLoad.Clear();
        }

        // Update any chunks that need it
        if (chunksToRegenerate.Count > 0)
        {
            foreach (Chunk c in chunksToRegenerate)
            {
                c.RegenerateChunk();
            }
            chunksToRegenerate.Clear();
        }
    }


    #region Chunk Handling

    // Loads all chunks around a point, optionally unloads chunks not within range
    void LoadChunksAround(BlockPos pos, int radius, bool unloadOthers=true)
    {
        // Get the chunk position
        Vector3Int chunkPos = pos / Chunk.CHUNK_SIZE;

        // Set up every chunk to be unloaded
        foreach (Vector3Int cPos in chunks.Keys)
        {
            chunkPositionsToUnload.Add(cPos);
        }

        // Figure out which chunks to load
        for (int x=-radius;x<=radius;x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3Int offset = new Vector3Int(x, y, z);
                    // Check if chunk is within radius
                    if (offset.magnitude <= radius)
                    {
                        chunkPositionsToLoad.Add(chunkPos+offset);
                        chunkPositionsToUnload.Remove(chunkPos+offset);
                    }
                }
            }
        }
    }

    // Load a chunk at the given chunk pos
    void LoadChunk(Vector3Int pos)
    {
        // Make sure the chunk is already loaded
        if (chunks.ContainsKey(pos))
        {
            Debug.LogWarning("Trying to load existing chunk at " + pos);
            return;
        }

        // Create object (TODO use pooling for this)
        GameObject chunkGo = Instantiate(chunkPrefab, pos * Chunk.CHUNK_SIZE, Quaternion.identity, transform);
        chunkGo.name = "Chunk " + pos.ToString();

        // Setup the chunk
        Chunk chunk = chunkGo.GetComponent<Chunk>();
        chunk.chunkPos = pos;

        // Add to dictionary
        chunks[pos] = chunk;
        chunksToRegenerate.Enqueue(chunk);
    }

    void UnloadChunk(Vector3Int pos)
    {
        // Make sure the chunk is already loaded
        if (chunks.ContainsKey(pos))
        {
            Chunk chunk = chunks[pos];
            Destroy(chunk.gameObject);
            chunks.Remove(pos);
        }
        else
        {
            Debug.LogWarning("Trying to unload nonexistent chunk at " + pos);
        }
    }
    #endregion

    #region Block Handling
    // Block handling
    public Block GetBlock(BlockPos pos)
    {
        // Figure out the chunk this belongs to
        Vector3Int chunkPos = pos / Chunk.CHUNK_SIZE;
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk chunk = chunks[chunkPos];
            return chunk.GetBlock(pos - (BlockPos)(chunk.chunkPos * Chunk.CHUNK_SIZE));
        }
        else
        {
            // Nope
            return null;
        }
    }

    public void SetBlock(BlockPos pos, Block block)
    {
        // Figure out the chunk this belongs to
        Vector3Int chunkPos = pos / Chunk.CHUNK_SIZE;
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk chunk = chunks[chunkPos];
            chunk.SetBlock(pos - (BlockPos)(chunk.chunkPos * Chunk.CHUNK_SIZE),block);

            chunksToRegenerate.Enqueue(chunk);
        }
        else
        {
            // Nope
            // TODO queue block placements
        }
    }
    #endregion
}
