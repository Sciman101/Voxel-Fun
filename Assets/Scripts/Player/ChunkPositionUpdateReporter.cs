using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ChunkPositionEvent : UnityEvent<Vector3Int> { }

public class ChunkPositionUpdateReporter : MonoBehaviour
{
    // This method calls whenever the associated transform of this script enters a new chunk
    public ChunkPositionEvent onChunkPositionChanged = new ChunkPositionEvent();

    private Vector3Int chunkPos;
    public Vector3Int ChunkPos
    {
        get => chunkPos;
    }

    private void Start()
    {
        chunkPos = World.instance.GetChunkPos(transform.position);
    }

    private void Update()
    {
        // Update chunk pos
        Vector3Int newChunkPos = World.instance.GetChunkPos(transform.position);
        if (newChunkPos != chunkPos)
        {
            chunkPos = newChunkPos;
            onChunkPositionChanged.Invoke(chunkPos);
        }
    }

}
