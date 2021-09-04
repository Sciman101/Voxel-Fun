using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{

    // How big is a chunk on a side?
    // Keeping this at 16 due to mesh data limits
    // Maybe make blocks share common vertices?
    public const int CHUNK_SIZE = 24;

    public Vector3Int chunkPos;

    // Mesh display
    new MeshRenderer renderer;
    MeshFilter filter;
    MeshCollider collider;

    public ChunkMesh mesh;

    // Block data
    byte[] blocks = new byte[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

    // Chunk properties
    public bool isEmpty = true; // Is this chunk only air blocks?

    private void Awake()
    {
        // Get components
        renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();

        mesh = new ChunkMesh(filter);
    }

    // Tells the chunk to rebuild it's mesh
    public void RegenerateChunk()
    {
        //Debug.Log("Regenerating chunk " + chunkPos);
        //ChunkMeshGenerator.GenerateMesh(this);
        ChunkMeshGenerator.RequestChunkMesh(this,UploadMeshData);
    }

    public static void UploadMeshData(Chunk chunk)
    {
        chunk.SetVisible(true);
        chunk.mesh.Upload();
    }

    // Set the position of this chunk
    public void SetPosition(Vector3Int pos)
    {
        chunkPos = pos;
        transform.position = pos * CHUNK_SIZE;
        gameObject.name = "Chunk " + pos;
    }

    public void SetVisible(bool vis)
    {
        renderer.enabled = vis;
    }

    #region Coordinate Conversion
    public BlockPos Chunk2World(BlockPos pos)
    {
        return pos + (BlockPos)(chunkPos * CHUNK_SIZE);
    }

    public BlockPos World2Chunk(BlockPos pos)
    {
        return pos - (BlockPos)pos.GetChunkPos() * CHUNK_SIZE;
    }
    #endregion

    #region Block Handling
    // Make sure a position exists in the chunk
    bool ValidatePos(BlockPos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
            pos.x < CHUNK_SIZE && pos.y < CHUNK_SIZE && pos.z < CHUNK_SIZE;
    }

    // Get a block at a position
    public Block GetBlock(BlockPos pos)
    {
        if (ValidatePos(pos))
        {
            return Blocks.FromId(blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE]);
        }
        else
        {
            return null; // Empty block
        }
    }

    public Block GetBlock(Vector3 pos)
    {
        return GetBlock(new BlockPos(pos));
    }

    // Set a block at a position
    public void SetBlock(BlockPos pos, Block block)
    {
        if (ValidatePos(pos))
        {
            blocks[pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE] = block.Id;
        }
    }
    #endregion

    public readonly struct ChunkCallback<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;
        public ChunkCallback(Action<T> callback, T parameter)
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
