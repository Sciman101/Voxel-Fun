using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // How far out should chunks be loaded?
    private static readonly int CHUNK_LOAD_RADIUS = 5;

    public static World instance;

    public GameObject chunkPrefab;
    public Player player;

    Vector3Int lastChunkLoadPoint;

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

        LoadChunksAround(Vector3Int.zero, CHUNK_LOAD_RADIUS);

        Debug.Log(string.Format("World generation took {0}s",Time.realtimeSinceStartup-start));

        player.onPlayerChunkChanged.AddListener((pos) =>
        {
            if ((lastChunkLoadPoint - pos).sqrMagnitude >= 16)
            {
                Debug.Log("loading new chunks...");
                LoadChunksAround(pos, CHUNK_LOAD_RADIUS);
                lastChunkLoadPoint = pos;
            }
        });
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

    // Get the chunk a world position is in
    public Vector3Int GetChunkPos(Vector3 pos)
    {
        pos /= Chunk.CHUNK_SIZE;
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    // Loads all chunks around a point, optionally unloads chunks not within range
    void LoadChunksAround(Vector3Int chunkPos, int radius, bool unloadOthers=true)
    {

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
                    if (Mathf.Abs(x)+Mathf.Abs(y)+Mathf.Abs(z) <= radius)
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

        // Tell the chunk what to do
        TerrainGenerator.GenerateChunkTerrain(chunk);

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

    // Try and reload a chunk by position
    void TryReloadChunk(Vector3Int pos)
    {
        if (chunks.ContainsKey(pos))
        {
            chunksToRegenerate.Enqueue(chunks[pos]);
        }
    }
    #endregion

    #region Block Handling
    // Block handling
    public Block GetBlock(BlockPos pos)
    {
        // Figure out the chunk this belongs to
        Vector3Int chunkPos = pos.GetChunkPos();
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk chunk = chunks[chunkPos];
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
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk chunk = chunks[chunkPos];
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
