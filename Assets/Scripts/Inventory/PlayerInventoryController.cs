using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController Instance;

    private InventoryUIManager inventoryUI;
    Inventory inventory;
    public GameObject testPickupPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        inventoryUI = FindObjectOfType<InventoryUIManager>();
    }

    private void Start()
    {
        inventory = new Inventory();
        inventoryUI.SetInventory(inventory);
    }

    public void PickUpItem(ItemPickup pickup)
    {
        if (pickup == null) return;
        
        if (inventory.AddItem(pickup.GetItem()))
        {
            pickup.DestroySelf();
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
