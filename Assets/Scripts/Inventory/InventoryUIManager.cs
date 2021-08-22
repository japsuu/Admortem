using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeMonkey.Utils;
using UnityEngine.EventSystems;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    public Transform hotbarTransform;
    public Transform inventoryTransform;
    public AbstractItem currentlySelectedItem { get; private set; }

    // Check item type, if not stackable and no slots, reject, if stackable and no slots, implement function to check if inventory has stack of item already to a dd the item in to.
    public List<int> emptyInventorySlots = new List<int>();

    public Inventory Inventory { get; private set; }

    Transform inventorySlotsTransform;
    Transform itemSlotTemplate = null;
    int lastSelectedSlotIndex = 0;
    static int UILayer;
    Color inventorySlotDefaultColor;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        inventorySlotsTransform = inventoryTransform.Find("Slots");
        inventorySlotDefaultColor = inventorySlotsTransform.GetChild(0).GetComponent<Image>().color;
    }

    private void Start()
    {
        UILayer = LayerMask.NameToLayer("UI");

        for (int i = 0; i < inventorySlotsTransform.childCount; i++)
        {
            emptyInventorySlots.Add(i);

            inventorySlotsTransform.GetChild(i).GetComponent<Button_UI>().ClickFunc = () =>
            {
                Inventory.SelectItemInInventory(null);
                BuildingController.SetSelectedBlock(null);
            };
        }
    }

    public void SetCurrentlySelectedItem(AbstractItem item)
    {
        inventorySlotsTransform.GetChild(lastSelectedSlotIndex).GetComponent<Image>().color = inventorySlotDefaultColor;

        if(item != null)
        {
            inventorySlotsTransform.GetChild(item.InventorySlotIndex).GetComponent<Image>().color = Color.white;
            lastSelectedSlotIndex = item.InventorySlotIndex;
        }
    }

    public void SetInventory(Inventory inventory)
    {
        this.Inventory = inventory;
        itemSlotTemplate = transform.GetChild(0).Find("ItemTemplate");

        inventory.OnItemListChanged += Inventory_OnItemListChanged;

        RefreshInventoryItems();
    }

    private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
    }

    public void RemoveItemFromUI(AbstractItem item)
    {
        emptyInventorySlots.Insert(item.InventorySlotIndex, item.InventorySlotIndex);

        foreach (Transform child in inventorySlotsTransform.GetChild(item.InventorySlotIndex))
        {
            Destroy(child.gameObject);
        }

        emptyInventorySlots.Sort();

        SetCurrentlySelectedItem(null);
    }

    void RefreshInventoryItems()
    {
        /*
        foreach (Transform child in inventorySlotsTransform)
        {
            if(child.childCount > 0)
            {
                Transform item = child.GetChild(0);

                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        foreach (Transform child in inventorySlotsTransform)
        {
            emptyInventorySlots.Add(child);
        }

        for (int i = 0; i < inventorySlotsTransform.childCount; i++)
        {
            emptyInventorySlots.Add(i);
        }

        foreach (AbstractItem item in Inventory.GetItemList())
        {
            Transform slot = emptyInventorySlots[0];

            RectTransform itemTransform = Instantiate(itemSlotTemplate, slot, true).GetComponent<RectTransform>();
            itemTransform.gameObject.SetActive(true);
            itemTransform.gameObject.name = "Hotbar Slot " + item.ID;

            itemTransform.localPosition = Vector3.zero;

            itemTransform.GetComponent<Button_UI>().ClickFunc = () =>
            {
                Inventory.SelectItemInInventory(item);
            };

            itemTransform.GetComponent<Button_UI>().MouseRightClickFunc = () =>
            {
                if (Input.GetKey(Settings.ItemDropModifier))
                {
                    AbstractItem duplicate = new AbstractItem(item.ID, item.PickupPrefab, item.Amount, item.IsStackable, item.InventorySprite);

                    Inventory.RemoveItem(item);
                    ItemPickup.DropItem(duplicate, PlayerMovementController.Instance);
                }
            };

            itemTransform.gameObject.GetComponent<Image>().sprite = item.InventorySprite;

            TextMeshProUGUI amountTxt = itemTransform.Find("AmountTxt").GetComponent<TextMeshProUGUI>();
            if(item.Amount > 1)
            {
                amountTxt.text = item.Amount + "x";
            }
            else
            {
                amountTxt.text = "";
            }

            emptyInventorySlots.Remove(slot);
        }
        */



        foreach (AbstractItem item in Inventory.GetItemList())
        {
            if(item.InventorySlotIndex == -1)
            {
                //Debug.Log("Adding item " + item + " with id: " + item.ID + " to inventory");
                item.InventorySlotIndex = emptyInventorySlots[0];

                emptyInventorySlots.RemoveAt(0);

                RectTransform itemTransform = Instantiate(itemSlotTemplate, inventorySlotsTransform.GetChild(item.InventorySlotIndex), true).GetComponent<RectTransform>();
                itemTransform.gameObject.SetActive(true);
                itemTransform.gameObject.name = "Item ID: " + item.ID;

                itemTransform.localPosition = Vector3.zero;

                itemTransform.GetComponent<Button_UI>().ClickFunc = () =>
                {
                    Inventory.SelectItemInInventory(item);
                };

                itemTransform.GetComponent<Button_UI>().MouseRightClickFunc = () =>
                {
                    if (Input.GetKey(Settings.ItemDropModifier))
                    {
                        AbstractItem duplicate = item.CreateDuplicate();

                        Inventory.RemoveItem(item);
                        ItemPickup.DropItem(duplicate, PlayerMovementController.Instance);
                    }
                };

                itemTransform.gameObject.GetComponent<Image>().sprite = item.InventorySprite;

                TextMeshProUGUI amountTxt = itemTransform.Find("AmountTxt").GetComponent<TextMeshProUGUI>();
                if (item.Amount > 1)
                {
                    amountTxt.text = item.Amount + "x";
                }
                else
                {
                    amountTxt.text = "";
                }

                item.AmountText = amountTxt;
            }
            else
            {
                if (item.Amount > 1)
                {
                    item.AmountText.text = item.Amount + "x";
                }
                else
                {
                    item.AmountText.text = "";
                }
            }
        }
    }


    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
