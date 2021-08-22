using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController Instance;

    public InventoryUIManager inventoryUI;
    Inventory inventory;
    public GameObject testPickupPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        inventory = new Inventory();
        inventoryUI.SetInventory(inventory);
    }

    public void PickUpItem(ItemPickup pickup)
    {
        if (pickup != null)
        {
            if (inventory.AddItem(pickup.GetItem()))
            {
                pickup.DestroySelf();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemPickup pickup = collision.GetComponent<ItemPickup>();

        pickup.attractionTarget = transform;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemPickup pickup = collision.GetComponent<ItemPickup>();

        if (pickup != null)
        {
            pickup.attractionTarget = null;
        }
    }
}
