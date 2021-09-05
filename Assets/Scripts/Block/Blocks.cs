using UnityEngine;

public static class Blocks
{
    private static Block[] blockTypes = new Block[256];
    private static byte numBlocks = 0;


    public static readonly Block AIR;
    public static readonly Block DIRT;
    public static readonly Block STONE;
    public static readonly Block GRASS;
    public static readonly Block LOG;
    public static readonly Block GRASS_PLANT;
    public static readonly Block FLOWER;
    public static readonly Block WATER;

    // Set up all the blocks
    static Blocks()
    {
        AIR = AddBlock(new Block("Air", Vector2.zero, true));
        DIRT = AddBlock(new Block("Dirt"));
        STONE = AddBlock(new Block("Stone",new Vector2(1f/16,0)));
        GRASS = AddBlock(new BlockGrass("Grass"));
        LOG = AddBlock(new BlockLog("Log"));
        GRASS_PLANT = AddBlock(new BlockGrassPlant("Tall Grass",new Vector2(6f/16,0)));
        FLOWER = AddBlock(new BlockGrassPlant("Flower",new Vector2(7f/16,0)));
        WATER = AddBlock(new Block("Water",new Vector2(8f/16,0),true));
    }

    public static Block FromId(byte id)
    {
        return blockTypes[id];
    }

    // Add a new block to the registry
    private static Block AddBlock(Block block)
    {
        if (numBlocks <= 255)
        {
            block.SetId(numBlocks);
            blockTypes[numBlocks++] = block;
            return block;
        }
        else
        {
            throw new System.Exception("Cannot register any more blocks!");
        }
    }
}
