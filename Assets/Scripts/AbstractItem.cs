using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class AbstractItem
{
    public int ID;
    public GameObject PickupPrefab;
    public int Amount;
    public bool IsStackable;

    [HideInInspector] public TextMeshProUGUI AmountText = null;
    [HideInInspector] public int InventorySlotIndex = -1;
    [HideInInspector] public Sprite InventorySprite;

    public AbstractItem(int iD, GameObject pickupPrefab, int amount, bool isStackable, Sprite inventorySprite)
    {
        ID = iD;
        PickupPrefab = pickupPrefab;
        Amount = amount;
        IsStackable = isStackable;
        InventorySprite = inventorySprite;
    }

    /// <summary>
    /// Called when the user clicks the item in the inventory.
    /// </summary>
    /// <returns>True, if the item was consumed in the process.</returns>
    public virtual bool OnClickedInInventory()
    {
        Debug.Log("Tried to use an item with no use behaviour!");

        return false;
    }

    public virtual AbstractItem CreateDuplicate(int amount = -1)
    {
        Debug.Log("Tried to duplicate item with no overridden duplicate behaviour!");

        return null;
    }
}
