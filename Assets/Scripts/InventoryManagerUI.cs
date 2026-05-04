using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManagerUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform itemContainer;

    [Header("Paginacion")]
    public int itemsPerPage = 8;
    public Button prevPageButton;
    public Button nextPageButton;
    public TMP_Text pageIndicator;

    private int _currentPage = 0;

    private void Awake()
    {
        prevPageButton?.onClick.AddListener(PrevPage);
        nextPageButton?.onClick.AddListener(NextPage);
    }

    public void RefreshInventoryUI()
    {
        _currentPage = 0;
        DrawPage();
    }

    private void DrawPage()
    {
        foreach (Transform t in itemContainer)
            Destroy(t.gameObject);

        List<Item> inventory = InventoryManager.Instance.inventory;
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)inventory.Count / itemsPerPage));
        _currentPage = Mathf.Clamp(_currentPage, 0, totalPages - 1);

        int start = _currentPage * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, inventory.Count);

        for (int i = start; i < end; i++)
        {
            Item item = inventory[i];
            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI slotUI = slot.GetComponent<ItemSlotUI>();
            slotUI.itemIconImage.sprite = item.itemData.itemIcon;
            slotUI.itemName.text = item.itemData.itemName;
            slotUI.itemQuantity.text = "x" + item.itemQuantity;
            slotUI.Setup(item.itemData);
        }

        if (pageIndicator != null)
            pageIndicator.text = $"{_currentPage + 1} / {totalPages}";

        if (prevPageButton != null) prevPageButton.interactable = _currentPage > 0;
        if (nextPageButton != null) nextPageButton.interactable = _currentPage < totalPages - 1;
    }

    private void NextPage() { _currentPage++; DrawPage(); }
    private void PrevPage() { _currentPage--; DrawPage(); }
}
