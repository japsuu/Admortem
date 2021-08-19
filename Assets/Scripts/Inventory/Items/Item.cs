using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item : AbstractItem
{
    public Sprite m_InventorySprite;

    public Item(int iD, GameObject pickupPrefab, Sprite inventorySprite, int amount, bool isStackable) : base(iD, pickupPrefab, amount, isStackable, inventorySprite)
    {
        this.m_InventorySprite = inventorySprite;
        base.InventorySprite = this.m_InventorySprite;
        ID = iD;
        PickupPrefab = pickupPrefab;
        Amount = amount;
        IsStackable = isStackable;
    }
}
