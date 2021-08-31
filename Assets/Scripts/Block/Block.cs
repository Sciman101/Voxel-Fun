using UnityEngine;

public class Block
{
    private byte id;
    private readonly string name;
    private Vector2 uvCoord;
    private bool isTransparent;

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
    public bool IsTransparent()
    {
        return isTransparent;
    }

    // Get the coordinate for a face texture of the block
    public Vector2 GetFaceTextureCoord(BlockPos pos, BlockFace face)
    {
        return uvCoord;
    }
}
