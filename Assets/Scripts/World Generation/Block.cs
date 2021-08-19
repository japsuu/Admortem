using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Block : AbstractItem
{
    public RuleTile Tile;

    public Block(int iD, GameObject pickupPrefab, int amount, bool isStackable, RuleTile tile) : base(iD, pickupPrefab, amount, isStackable, tile.m_DefaultSprite)
    {
        Tile = tile;
    }

    public Block(Block block, int amount) : base(block.ID, block.PickupPrefab, amount, block.IsStackable, block.Tile.m_DefaultSprite)
    {
        Tile = block.Tile;
    }

    public static Block CreateDebugDuplicate(Block block, int amount)
    {
        Block duplicate = new Block(block, amount);

        return duplicate;
    }

    public override AbstractItem CreateDuplicate(int amount = -1)
    {
        Block duplicate;

        if (amount > 0)
            duplicate = new Block(this, amount);
        else
            duplicate = new Block(this, Amount);

        return duplicate;
    }

    public override bool OnClickedInInventory()
    {
        BuildingController.SetSelectedBlock(this);

        return false;
    }

    //TODO: Only expose a public Texture2D for the textureSheet, and use the autoruletile to generate a ruletile for it.
}
