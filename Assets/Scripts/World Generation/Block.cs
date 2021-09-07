using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Block : AbstractItem
{
    public RuleTile Tile;

    public int Durability;
    
    [EventRef] [SerializeField] private string PlacedEvent;
    [EventRef] [SerializeField] private string MinedEvent;
    [EventRef] [SerializeField] private string BrokenEvent;

    public Block(int iD, GameObject pickupPrefab, int amount, bool isStackable, RuleTile tile, int durability) : base(iD, pickupPrefab, amount, isStackable, tile.m_DefaultSprite)
    {
        Tile = tile;
        Durability = durability;
    }

    public Block(Block block, int amount) : base(block.ID, block.PickupPrefab, amount, block.IsStackable, block.Tile.m_DefaultSprite)
    {
        Tile = block.Tile;
        Durability = block.Durability;
    }

    public string GetPlacedEvent()
    {
        string holderEvent = ItemHolder.BlockDictionary[Tile].PlacedEvent;
        
        if (string.IsNullOrEmpty(holderEvent))
        {
            return AudioManager.Instance.genericPlacedEvent;
        }

        return holderEvent;
    }

    public string GetMinedEvent()
    {
        string holderEvent = ItemHolder.BlockDictionary[Tile].MinedEvent;
        
        if (string.IsNullOrEmpty(holderEvent))
        {
            return AudioManager.Instance.genericMinedEvent;
        }

        return holderEvent;
    }

    public string GetBrokenEvent()
    {
        string holderEvent = ItemHolder.BlockDictionary[Tile].BrokenEvent;
        
        if (string.IsNullOrEmpty(holderEvent))
        {
            return AudioManager.Instance.genericBrokenEvent;
        }

        return holderEvent;
    }
    
    /*
    public static Block CreateDuplicate(Block block, int amount)
    {
        if (block == null) return null;
        
        Block duplicate = new Block(block, amount);
        
        Debug.Log("Created duplicate of block with " + block.Durability + "durability");

        return duplicate;
    }*/

    public override AbstractItem CreateDuplicate(bool fullDurability = false, int amount = -1)
    {
        Block duplicate = amount > 0 ? new Block(this, amount) : new Block(this, Amount);

        if (fullDurability) duplicate.Durability = ItemHolder.BlockDictionary[duplicate.Tile].Durability;
        
        return duplicate;
    }

    public override bool OnClickedInInventory()
    {
        BuildingController.SetSelectedBlock(this);

        return false;
    }
}
