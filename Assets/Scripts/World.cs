using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class World : MonoBehaviour
{
    // How far out should chunks be loaded?
    private static readonly int CHUNK_LOAD_RADIUS = 7;
    private static readonly int MAX_CHUNKS = 600;

    public static World instance;

    public ChunkPositionUpdateReporter playerChunkPositionUpdateReporter;
    public GameObject chunkPrefab;


    // Pool containing all chunks we can pull from
    private Queue<Chunk> chunkPool = new Queue<Chunk>(MAX_CHUNKS);

    // Dictionary of currently loaded chunks
    public Dictionary<Vector3Int, Chunk> loadedChunks = new Dictionary<Vector3Int, Chunk>(MAX_CHUNKS);

    // Queues used for late-updating chunks
    private Queue<Chunk> chunksToRegenerate = new Queue<Chunk>(MAX_CHUNKS);
    private HashSet<Chunk> chunksToUnload = new HashSet<Chunk>();
    private HashSet<Vector3Int> chunkPositionsToLoad = new HashSet<Vector3Int>();

    public int NumLoadedChunks
    {
        get => loadedChunks.Count;
    }
    public int ChunkPoolCount
    {
        get => chunkPool.Count;
    }

    public float lastChunkUpdateTime = 0;

    Vector3Int lastReloadChunkPos;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Create pool of chunks to load from
        InitializePool();

        LoadChunksAround(Vector3Int.zero, CHUNK_LOAD_RADIUS);

        playerChunkPositionUpdateReporter.onChunkPositionChanged.AddListener((pos) =>
        {
            // Check for adequate distance
            if ((lastReloadChunkPos-pos).sqrMagnitude >= 9){
                lastReloadChunkPos = pos;
                LoadChunksAround(pos, CHUNK_LOAD_RADIUS);
            }
        });
    }

    // Regenerate and load queued chunks
    private void LateUpdate()
    {
        float startTime = -1;
        // unload uneeded chunks
        if (chunksToUnload.Count > 0)
        {
            startTime = Time.realtimeSinceStartup;
            foreach (Chunk chunk in chunksToUnload)
            {
                UnloadChunkImmediate(chunk);
            }
            chunksToUnload.Clear();
        }

        // Load needed chunks
        if (chunkPositionsToLoad.Count > 0)
        {
            foreach (Vector3Int pos in chunkPositionsToLoad)
            {
                LoadChunkImmediate(pos);
            }
            chunkPositionsToLoad.Clear();
        }
        
        // Regenerate all chunks that need to be regenerated
        while (chunksToRegenerate.Count > 0)
        {
            chunksToRegenerate.Dequeue().RegenerateChunk();
        }
        // Track how long the update took
        if (startTime != -1)
        {
            lastChunkUpdateTime = Time.realtimeSinceStartup - startTime;
        }

        ChunkMeshGenerator.Update();
        TerrainGenerator.Update();
    }

    #region Chunk Handling
    public Vector3Int GetChunkPos(Vector3 pos)
    {
        pos /= Chunk.CHUNK_SIZE;
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    // If a chunk exists, add it to the load queue
    private void TryReloadChunk(Vector3Int pos)
    {
        if (loadedChunks.ContainsKey(pos))
        {
            chunksToRegenerate.Enqueue(loadedChunks[pos]);
        }
    }

    // Load all the chunks in a radius around a center, unloading chunks that need to be unloaded
    public void LoadChunksAround(Vector3Int center, int radius)
    {
        // Register every chunk to be unloaded
        foreach (Chunk chunk in loadedChunks.Values)
        {
            chunksToUnload.Add(chunk);
        }

        // Loop over region
        for (int x=-radius;x<=radius;x++)
        {
            for (int y = -1; y <= 3; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    // Check if chunk is in loading range
                    if (Math.Abs(x) + Math.Abs(y) + Math.Abs(z) <= radius)
                    {
                        Vector3Int pos = center + new Vector3Int(x,y,z);
                        
                        if (loadedChunks.ContainsKey(pos))
                        {
                            // If this chunk was already loaded, remove it from the unload list
                            chunksToUnload.Remove(loadedChunks[pos]);
                        }
                        else
                        {
                            // Tell it to load
                            chunkPositionsToLoad.Add(pos);
                        }

                    }
                }

            }

        }
    }

    public void LoadChunk(Vector3Int pos)
    {
        chunkPositionsToLoad.Add(pos);
    }

    public void UnloadChunk(Vector3Int pos)
    {
        if (loadedChunks.ContainsKey(pos))
        {
            chunksToUnload.Add(loadedChunks[pos]);
        }
    }

    // Load a chunk at the given position
    public void LoadChunkImmediate(Vector3Int pos)
    {
        // Make sure the chunk is already loaded
        if (loadedChunks.ContainsKey(pos))
        {
            Debug.LogWarning("Trying to load existing chunk at " + pos);
            return;
        }

        // Get a chunk from the queue
        if (chunkPool.Count <= 0)
        {
            // This chunk is already enabled, we ran out of chunks
            Debug.LogError("Error loading chunk at position " + pos + ",Out of chunks to load!");
            return;
        }

        Chunk chunk = chunkPool.Dequeue();

        // Enable the chunk
        chunk.gameObject.SetActive(true);
        chunk.SetVisible(false);
        chunk.SetPosition(pos);

        // Tell the chunk what to do
        TerrainGenerator.RequestChunkTerrainGeneration(chunk, QueueChunkForRegeneration);

        // Add to dictionary
        loadedChunks[pos] = chunk;
    }

    public static void QueueChunkForRegeneration(Chunk chunk)
    {
        World.instance.chunksToRegenerate.Enqueue(chunk);
    }

    // Immediately unload a chunk
    public void UnloadChunkImmediate(Chunk chunk)
    {
        // Make sure the chunk is already loaded
        if (loadedChunks.ContainsKey(chunk.chunkPos))
        {
            loadedChunks.Remove(chunk.chunkPos);
            chunk.gameObject.SetActive(false);
            // Add back to pool
            chunkPool.Enqueue(chunk);
        }
        else
        {
            Debug.LogWarning("Trying to unload nonexistent chunk at " + chunk.chunkPos);
        }
    }

    // Create the pool of chunk gameobjects to pull from
    private void InitializePool()
    {
        for (int i = 0; i < MAX_CHUNKS; i++)
        {
            GameObject chunkGo = Instantiate(chunkPrefab, transform);
            chunkGo.SetActive(false);

            Chunk chunk = chunkGo.GetComponent<Chunk>();
            chunkPool.Enqueue(chunk);
        }
    }
    #endregion

    #region Block Handling
    // Block handling
    public Block GetBlock(BlockPos pos)
    {
        // Figure out the chunk this belongs to
        Vector3Int chunkPos = pos.GetChunkPos();
        if (loadedChunks.ContainsKey(chunkPos))
        {
            Chunk chunk = loadedChunks[chunkPos];
            return chunk.GetBlock(chunk.World2Chunk(pos));
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
        Vector3Int chunkPos = pos.GetChunkPos();
        if (loadedChunks.ContainsKey(chunkPos))
        {
            Chunk chunk = loadedChunks[chunkPos];
            BlockPos posInChunk = chunk.World2Chunk(pos);
            chunk.SetBlock(posInChunk, block);

            chunksToRegenerate.Enqueue(chunk);

            int cs = Chunk.CHUNK_SIZE-1;

            // Check for additional chunks to update, if this block was on a chunk edge
            if (posInChunk.x == 0) TryReloadChunk(chunkPos+Vector3Int.left);
            else if (posInChunk.x == cs) TryReloadChunk(chunkPos+Vector3Int.right);

            if (posInChunk.y == 0) TryReloadChunk(chunkPos + Vector3Int.down);
            else if (posInChunk.y == cs) TryReloadChunk(chunkPos + Vector3Int.up);

            if (posInChunk.z == 0) TryReloadChunk(chunkPos + new Vector3Int(0,0,-1));
            else if (posInChunk.z == cs) TryReloadChunk(chunkPos + new Vector3Int(0, 0, 1));
        }
        else
        {
            // Nope
            // TODO queue block placements
        }
    }
    #endregion
}
