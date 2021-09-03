using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGrass : Block
{
    public BlockGrass(string name) : base(name)
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
                return Vector2.zero;

            case BlockFace.TOP:
                return new Vector2(3f / 16,0);

            default:
                return new Vector2(2f/16,0);
        }
    }
}
