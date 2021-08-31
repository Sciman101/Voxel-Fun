using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World instance;

    public GameObject chunkPrefab;

    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Queue<Chunk> chunksToRegenerate = new Queue<Chunk>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Time measuring
        float start = Time.realtimeSinceStartup;

        for (int i=0;i<10;i++)
        {
            LoadChunk(Vector3Int.right * i);
        }

        Debug.Log(string.Format("World generation took {0}s",Time.realtimeSinceStartup-start));
    }


    private void LateUpdate()
    {
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
            byte id = chunk.GetByte(pos - (BlockPos)(chunk.chunkPos * Chunk.CHUNK_SIZE));
            return Blocks.FromId(id);
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
            chunk.SetByte(pos - (BlockPos)(chunk.chunkPos * Chunk.CHUNK_SIZE),block.Id);

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
