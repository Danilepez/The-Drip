using UnityEngine;
using UnityEngine.UI;

public class InventoryManagerUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform itemContainer;

    public void RefreshInventoryUI()
    {
        //0. Limpiar elementos que ya tengamos
        foreach (Transform t in itemContainer)
        {
            Destroy(t.gameObject);
        }
        
        //1. Crear UI para cada item del inventario
        foreach (Item item in InventoryManager.Instance.inventory)
        {
            GameObject newItemSlot = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI itemSlotUI = newItemSlot.GetComponent<ItemSlotUI>();
            itemSlotUI.itemIconImage.sprite = item.itemData.itemIcon;
            itemSlotUI.itemName.text = item.itemData.itemName;
            itemSlotUI.itemQuantity.text = "x" + item.itemQuantity.ToString();
        }
    }
}
