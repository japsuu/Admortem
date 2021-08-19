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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Debug spawn");

            ItemPickup.SpawnItemPickup(Block.CreateDebugDuplicate(ItemHolder.Instance.Blocks[Random.Range(0, ItemHolder.Instance.Blocks.Count)], Random.Range(1, 100)), transform.position + new Vector3(0, 10), Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
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
