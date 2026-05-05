using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public List<Item> inventory = new List<Item>();

    public GameObject inventoryPanel;
    public InputActionReference toggleAction;

    [Header("Win Condition")]
    public bool winOnPaperCount = true;
    public int paperCountToWin = 5;

    public bool IsOpen { get; private set; }
    public bool IsBlocked { get; private set; }
    private bool _paperWinTriggered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(Instance.gameObject); }
        Instance = this;
    }

    private void OnEnable()  => toggleAction?.action?.Enable();
    private void OnDisable() => toggleAction?.action?.Disable();

    private void Start()
    {
        inventoryPanel.SetActive(false);
        InputHintsUI.Instance?.SetInventoryHint(IsOpen, IsBlocked);
    }

    private void Update()
    {
        if (IsBlocked) return;
        if (toggleAction != null && toggleAction.action.WasPressedThisFrame())
            Toggle();
    }

    public void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
        if (IsBlocked && IsOpen) Close();
        InputHintsUI.Instance?.SetInventoryHint(IsOpen, IsBlocked);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        IsOpen = true;
        inventoryPanel.SetActive(true);
        GetComponent<InventoryManagerUI>().RefreshInventoryUI();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerLook.IsFrozen = true;
        PlayerMovement.IsFrozen = true;
        InputHintsUI.Instance?.SetInventoryHint(IsOpen, IsBlocked);
    }

    public void Close()
    {
        IsOpen = false;
        ItemDetailUI.Instance.Hide();
        inventoryPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerLook.IsFrozen = false;
        PlayerMovement.IsFrozen = false;
        InputHintsUI.Instance?.SetInventoryHint(IsOpen, IsBlocked);
    }

    public void AddItem(ItemData itemData, int quantity)
    {
        foreach (Item item in inventory)
        {
            if (item.itemData == itemData)
            {
                item.itemQuantity += quantity;
                CheckPaperWin(itemData);
                return;
            }
        }
        inventory.Add(new Item { itemData = itemData, itemQuantity = quantity });
        CheckPaperWin(itemData);
    }

    private void CheckPaperWin(ItemData itemData)
    {
        if (_paperWinTriggered || !winOnPaperCount || itemData == null) return;
        if (itemData.itemType != ItemData.ItemType.Paper) return;

        int total = GetTotalQuantity(ItemData.ItemType.Paper);
        if (total >= paperCountToWin)
        {
            _paperWinTriggered = true;
            GameManager.Instance?.WinGame();
        }
    }

    private int GetTotalQuantity(ItemData.ItemType type)
    {
        int total = 0;
        foreach (Item item in inventory)
        {
            if (item.itemData != null && item.itemData.itemType == type)
            {
                total += item.itemQuantity;
            }
        }
        return total;
    }
}
