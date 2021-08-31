using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Vector3Int worldSize;
    public GameObject chunkPrefab;

    private Chunk[] chunks;

    private void Start()
    {
        chunks = new Chunk[worldSize.x * worldSize.y * worldSize.z];
        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int z = 0; z < worldSize.z; z++)
                {
                    GameObject go = Instantiate(chunkPrefab, transform);
                    go.transform.position = new Vector3(x, y, z) * Chunk.CHUNK_SIZE;
                    Chunk chunk = go.GetComponent<Chunk>();
                    chunk.chunkPos = new Vector3Int(x, y, z);

                    chunks[x + y * worldSize.x + z * worldSize.x * worldSize.y] = chunk;
                }
            }
        }
    }
}
