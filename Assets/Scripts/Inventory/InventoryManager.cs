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
    public bool winOnKeyCollect = false;

    public bool IsOpen { get; private set; }
    public bool IsBlocked { get; private set; }
    private bool _keyWinTriggered;

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
                CheckKeyWin(itemData);
                return;
            }
        }
        inventory.Add(new Item { itemData = itemData, itemQuantity = quantity });
        CheckKeyWin(itemData);
    }

    private void CheckKeyWin(ItemData itemData)
    {
        if (_keyWinTriggered || !winOnKeyCollect || itemData == null) return;
        if (itemData.itemType != ItemData.ItemType.Key) return;

        _keyWinTriggered = true;
        GameManager.Instance?.WinGame();
    }
}
