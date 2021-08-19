using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public event EventHandler OnItemListChanged;
    public List<AbstractItem> GetItemList() { return itemList; }

    readonly int inventorySize = 18;
    List<AbstractItem> itemList;

    bool InventoryIsFull { get => itemList.Count >= inventorySize; }

    public Inventory()
    {
        itemList = new List<AbstractItem>();
    }

    public bool AddItem(AbstractItem item)
    {
        bool pickedUp = false;

        if (item.IsStackable)
        {
            bool inventoryHasItem = false;

            foreach (AbstractItem inventoryItem in itemList)
            {
                if(inventoryItem.ID == item.ID)
                {
                    inventoryItem.Amount += item.Amount;

                    inventoryHasItem = true;
                    pickedUp = true;
                }
            }

            if (!inventoryHasItem && !InventoryIsFull)
            {
                itemList.Add(item);

                pickedUp = true;
            }
        }
        else if(!InventoryIsFull)
        {
            itemList.Add(item);

            pickedUp = true;
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);

        return pickedUp;
    }

    public void RemoveItem(AbstractItem item)
    {
        if (item.IsStackable)
        {
            AbstractItem itemInInventory = null;

            foreach (AbstractItem inventoryItem in itemList)
            {
                if(inventoryItem.ID == item.ID)
                {
                    inventoryItem.Amount -= item.Amount;
                    itemInInventory = inventoryItem;
                }
            }

            if(itemInInventory != null && itemInInventory.Amount <= 0)
            {
                SafeRemove(itemInInventory);
            }
        }
        else
        {
            SafeRemove(item);
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    void SafeRemove(AbstractItem item)
    {
        itemList.Remove(item);
        InventoryUIManager.Instance.RemoveItemFromUI(item);
    }

    public void SelectItemInInventory(AbstractItem item)
    {
        InventoryUIManager.Instance.SetCurrentlySelectedItem(item);

        if (item != null && item.OnClickedInInventory())
        {
            RemoveItem(item);
        }
        
    }
}
