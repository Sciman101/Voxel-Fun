using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLog : Block
{
    public BlockLog(string name) : base(name)
    {
    }

    public override bool IsTransparent()
    {
        return false;
    }

    public override Vector2 GetFaceTextureCoord(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.BOTTOM:
            case BlockFace.TOP:
                return new Vector2(5f / 16, 0);

            default:
                return new Vector2(4f / 16, 0);
        }
    }
}
