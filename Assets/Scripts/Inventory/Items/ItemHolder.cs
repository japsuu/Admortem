using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance;

    public List<Block> Blocks;
    public List<Item> Items;

    public static Dictionary<TileBase, Block> BlockDictionary = new Dictionary<TileBase, Block>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        foreach (Block block in Blocks)
        {
            BlockDictionary.Add(block.Tile, block);
        }
    }
}

public enum BlockType
{
    Filler,
    Ore,
    Treasure,
}