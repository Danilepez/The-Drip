using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public List<Item> inventory = new List<Item>();

    [Header("Debug")]
    public ItemData testItemData;
    public ItemData testItemData2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AddItem(testItemData, 5);
        AddItem(testItemData2, 3);

        GetComponent<InventoryManagerUI>().RefreshInventoryUI();
    }

    public void AddItem(ItemData itemData, int quantity)
    {
        foreach (Item item in inventory)
        {
            if (item.itemData.itemName == itemData.itemName)
            {
                item.itemQuantity += quantity;
                return;
            }
        }
        
        inventory.Add(new Item { itemData = itemData, itemQuantity = quantity });
    }
}
