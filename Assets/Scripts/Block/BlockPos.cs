using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public readonly struct BlockPos
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
    public readonly int x;
    public readonly int y;
    public readonly int z;

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
    public BlockPos offset(BlockFace face, int offset = 1)
    {
        switch (face)
        {
            case BlockFace.BOTTOM:
                return this.offset(0, -offset, 0);
            case BlockFace.TOP:
                return this.offset(0, offset, 0);
            case BlockFace.NORTH:
                return this.offset(0, 0, offset);
            case BlockFace.SOUTH:
                return this.offset(0, 0, -offset);
            case BlockFace.EAST:
                return this.offset(offset, 0, 0);
            case BlockFace.WEST:
                return this.offset(-offset, 0, 0);
        }
        return this;
    }


    // Figure out what chunk this block exists in
    public Vector3Int GetChunkPos()
    {
        // TODO make this not suck
        Vector3 temp = (((Vector3)this) / Chunk.CHUNK_SIZE);
        return new Vector3Int(Mathf.FloorToInt(temp.x), Mathf.FloorToInt(temp.y), Mathf.FloorToInt(temp.z));
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
    public static BlockPos operator /(BlockPos p, int a) => new BlockPos(p.x / a, p.y / a, p.z / a);
    public static BlockPos operator %(BlockPos p, int a) => new BlockPos(p.x % a, p.y % a, p.z % a);

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
    }

    // Equality Comparison
    public static bool operator ==(BlockPos a, BlockPos b) => (a.x == b.x && a.y == b.y && a.z == b.z);
    public static bool operator !=(BlockPos a, BlockPos b) => (a.x != b.x || a.y != b.y || a.z != b.z);

    // Conversion
    public static explicit operator Vector3(BlockPos pos) => new Vector3(pos.x,pos.y,pos.z);
    public static implicit operator Vector3Int(BlockPos pos) => new Vector3Int(pos.x,pos.y,pos.z);
    public static implicit operator BlockPos(Vector3Int pos) => new BlockPos(pos.x,pos.y,pos.z);
    public static explicit operator BlockPos(Vector3 pos) => new BlockPos((int)pos.x, (int)pos.y, (int)pos.z);

    // Iterate over all blocks in a specified volume
    // Deprecated due to performance issues
    /*public static IEnumerable<BlockPos> BlocksInVolume(BlockPos c1, BlockPos c2)
    {
        // Determine the two corners of our volume
        int minX = Math.Min(c1.x, c2.x);
        int minY = Math.Min(c1.y, c2.y);
        int minZ = Math.Min(c1.z, c2.z);

        int maxX = Math.Max(c1.x, c2.x);
        int maxY = Math.Max(c1.y, c2.y);
        int maxZ = Math.Max(c1.z, c2.z);

        // Do iteration
        for (int x=minX;x<maxX;x++)
        {
            for (int y=minY;y<maxY;y++)
            {
                for (int z=minZ;z<maxZ;z++)
                {
                    yield return new BlockPos(x,y,z);
                }
            }
        }
    }*/

    public override string ToString()
    {
        return string.Format("[{0}, {1}, {2}]",x,y,z);
    }
}
