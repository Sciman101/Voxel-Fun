using System;
using UnityEngine;

public struct BlockPos
{
    // Constants
    public static readonly BlockPos zero = new BlockPos(0, 0, 0);
    public static readonly BlockPos one = new BlockPos(1, 1, 1);
    public static readonly BlockPos up = new BlockPos(0, 1, 0);
    public static readonly BlockPos down = new BlockPos(0, -1, 0);
    public static readonly BlockPos fwd = new BlockPos(0, 0, 1);
    public static readonly BlockPos back = new BlockPos(0, 0, -1);
    public static readonly BlockPos right = new BlockPos(1, 0, 0);
    public static readonly BlockPos left = new BlockPos(-1, 0, 0);

    // Coordinate
    public int x;
    public int y;
    public int z;

    // Default constructor
    public BlockPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public BlockPos(Vector3 pos)
    {
        this.x = (int)pos.x;
        this.y = (int)pos.y;
        this.z = (int)pos.z;
    }
    public BlockPos(Vector3Int pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    // Offset this position by a 
    public BlockPos offset(int x, int y, int z) => new BlockPos(this.x+x,this.y+y,this.z+z);
    // Offset this position by a face
    public BlockPos offset(BlockFace face, int offset=1)
    {
        switch (face)
        {
            case BlockFace.BOTTOM:
                return this.offset(0,-offset,0);
            case BlockFace.TOP:
                return this.offset(0, offset, 0);
            case BlockFace.NORTH:
                return this.offset(0, 0, offset);
            case BlockFace.EAST:
                return this.offset(offset, 0, 0);
            case BlockFace.WEST:
                return this.offset(-offset, 0, 0);
            case BlockFace.SOUTH:
                return this.offset(0, 0, -offset);
        }
        return this;
    }

    public override bool Equals(object obj)
    {
        return obj is BlockPos pos &&
               x == pos.x &&
               y == pos.y &&
               z == pos.z;
    }

    public override int GetHashCode()
    {
        int hashCode = 373119288;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    // Operator overrides
    public static BlockPos operator -(BlockPos a) => new BlockPos(-a.x, -a.y, -a.z);
    public static BlockPos operator +(BlockPos a, BlockPos b) => new BlockPos(a.x + b.x, a.y + b.y, a.z + b.z);
    public static BlockPos operator -(BlockPos a, BlockPos b) => new BlockPos(a.x - b.x, a.y - b.y, a.z - b.z);
    public static BlockPos operator *(BlockPos p, int a) => new BlockPos(p.x * a, p.y * a, p.z * a);

    public int this[int key]
    {
        get
        {
            switch (key)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch (key)
            {
                case 0: x = value; break;
                case 1: y = value; break;
                case 2: z = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

    // Equality Comparison
    public static bool operator ==(BlockPos a, BlockPos b) => (a.x == b.x && a.y == b.y && a.z == b.z);
    public static bool operator !=(BlockPos a, BlockPos b) => (a.x != b.x || a.y != b.y || a.z != b.z);
}
