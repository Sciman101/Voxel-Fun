﻿using UnityEngine;

public static class Blocks
{
    private static Block[] blockTypes = new Block[256];
    private static byte numBlocks = 0;


    public static readonly Block AIR;
    public static readonly Block DIRT;
    public static readonly Block STONE;

    // Set up all the blocks
    static Blocks()
    {
        AIR = AddBlock(new Block("Air", Vector2.zero, true));
        DIRT = AddBlock(new Block("Dirt"));
        STONE = AddBlock(new Block("Stone",new Vector2(1f/16,0)));
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