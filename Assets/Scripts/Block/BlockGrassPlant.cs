using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGrassPlant : Block
{
    public BlockGrassPlant(string name, Vector2 uvCoord) : base(name, uvCoord)
    {
    }

    public override void GenerateCustomMesh(BlockPos pos, Vector3 posInChunk, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        int t = vertices.Count;
        vertices.Add(posInChunk);
        vertices.Add(posInChunk+Vector3.up);
        vertices.Add(posInChunk+new Vector3(1,1,1));
        vertices.Add(posInChunk+new Vector3(1,0,1));

        uvs.Add(uvCoord);
        uvs.Add(uvCoord+new Vector2(0,1f/16));
        uvs.Add(uvCoord+new Vector2(1f/16,1f/16));
        uvs.Add(uvCoord+new Vector2(1f/16,0));

        triangles.Add(t);
        triangles.Add(t+1);
        triangles.Add(t+3);
        triangles.Add(t+3);
        triangles.Add(t+1);
        triangles.Add(t+2);

        triangles.Add(t+3);
        triangles.Add(t + 1);
        triangles.Add(t);
        triangles.Add(t + 2);
        triangles.Add(t + 1);
        triangles.Add(t + 3);

        t = vertices.Count;
        vertices.Add(posInChunk+Vector3.forward);
        vertices.Add(posInChunk + new Vector3(0,1,1));
        vertices.Add(posInChunk + new Vector3(1, 1, 0));
        vertices.Add(posInChunk + new Vector3(1, 0, 0));

        uvs.Add(uvCoord);
        uvs.Add(uvCoord + new Vector2(0, 1f / 16));
        uvs.Add(uvCoord + new Vector2(1f / 16, 1f / 16));
        uvs.Add(uvCoord + new Vector2(1f / 16, 0));

        triangles.Add(t);
        triangles.Add(t + 1);
        triangles.Add(t + 3);
        triangles.Add(t + 3);
        triangles.Add(t + 1);
        triangles.Add(t + 2);

        triangles.Add(t + 3);
        triangles.Add(t + 1);
        triangles.Add(t);
        triangles.Add(t + 2);
        triangles.Add(t + 1);
        triangles.Add(t + 3);
    }

    public override bool IsFullCube()
    {
        return false;
    }

    public override bool IsTransparent()
    {
        return true;
    }
}
