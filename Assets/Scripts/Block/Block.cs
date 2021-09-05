using System.Collections.Generic;
using UnityEngine;

public class Block
{
    protected byte id;
    protected readonly string name;
    protected Vector2 uvCoord;
    protected bool isTransparent;

    public byte Id {
        get => id;
    }

    public Block(string name, Vector2 uvCoord, bool isTransparent)
    {
        this.name = name;
        this.uvCoord = uvCoord;
        this.isTransparent = isTransparent;
    }

    public Block(string name, Vector2 uvCoord)
    {
        this.name = name;
        this.uvCoord = uvCoord;
    }

    public Block(string name)
    {
        this.name = name;
    }

    // Set the id for this block
    public void SetId(byte id)
    {
        if (this.id == 0)
        {
            this.id = id;
        }
    }

    // Should adjacent blocks render faces for this?
    public virtual bool IsTransparent()
    {
        return isTransparent;
    }

    // Does this block occupy a full cube?
    public virtual bool IsFullCube()
    {
        return true;
    }

    // Does this block generate a mesh?
    public virtual bool HasMesh()
    {
        return this != Blocks.AIR;
    }

    // Get the coordinate for a face texture of the block
    public virtual Vector2 GetFaceTextureCoord(BlockFace face)
    {
        return uvCoord;
    }

    // Add a custom mesh to the world
    // Will only call if IsFullCube is false
    public virtual void GenerateCustomMesh(BlockPos pos, Vector3 posInChunk, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {

    }

    public override string ToString()
    {
        return string.Format("{0} - id {1}",name,id);
    }
}
