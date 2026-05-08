using System.Collections;
using UnityEngine;

public class KeyDoor : BaseInteractable
{
    [Header("Llave requerida")]
    public ItemData keyItemData;

    protected override void Init() { }

    protected override IEnumerator Interact()
    {
        if (!HasKey())
        {
            Debug.Log("[KeyDoor] Necesitas la llave para abrir esta puerta.");
            yield break;
        }

        Debug.Log("[KeyDoor] Puerta abierta con llave — ¡Victoria!");
        GameManager.Instance?.WinGame();
    }

    private bool HasKey()
    {
        if (InventoryManager.Instance == null) return false;

        foreach (Item item in InventoryManager.Instance.inventory)
        {
            if (item.itemData == null) continue;
            if (keyItemData != null && item.itemData == keyItemData) return true;
            if (keyItemData == null && item.itemData.itemType == ItemData.ItemType.Key) return true;
        }

        return false;
    }
}
