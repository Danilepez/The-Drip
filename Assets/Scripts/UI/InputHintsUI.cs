using UnityEngine;

public class InputHintsUI : MonoBehaviour
{
    public static InputHintsUI Instance { get; private set; }

    [Header("Examine")]
    public GameObject enterExamineHint;
    public GameObject examineExitHint;

    [Header("Doors / Drawers")]
    public GameObject doorOpenHint;
    public GameObject doorCloseHint;

    [Header("Pickup")]
    public GameObject pickupHint;

    [Header("Blood")]
    public GameObject bloodHint;

    [Header("Recorder")]
    public GameObject recorderHint;

    [Header("Inventory")]
    public GameObject inventoryOpenHint;
    public GameObject inventoryCloseHint;

    private object _doorOwner;
    private object _enterExamineOwner;
    private object _pickupOwner;
    private object _bloodOwner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetActive(enterExamineHint, false);
        SetActive(examineExitHint, false);
        SetActive(doorOpenHint, false);
        SetActive(doorCloseHint, false);
        SetActive(pickupHint, false);
        SetActive(bloodHint, false);
        SetActive(recorderHint, false);
        SetActive(inventoryOpenHint, false);
        SetActive(inventoryCloseHint, false);
    }

    public void SetEnterExamineHint(object owner, bool show)
    {
        if (show)
        {
            _enterExamineOwner = owner;
            SetActive(enterExamineHint, true);
        }
        else
        {
            if (_enterExamineOwner != owner) return;
            _enterExamineOwner = null;
            SetActive(enterExamineHint, false);
        }
    }

    public void SetExamineExitHint(bool isExamining)
    {
        SetActive(examineExitHint, isExamining);
    }

    public void SetDoorHint(object owner, bool show, bool isOpen)
    {
        if (show)
        {
            _doorOwner = owner;
            SetActive(doorOpenHint, !isOpen);
            SetActive(doorCloseHint, isOpen);
        }
        else
        {
            if (_doorOwner != owner) return;
            _doorOwner = null;
            SetActive(doorOpenHint, false);
            SetActive(doorCloseHint, false);
        }
    }

    public void SetPickupHint(object owner, bool show)
    {
        if (show)
        {
            _pickupOwner = owner;
            SetActive(pickupHint, true);
        }
        else
        {
            if (_pickupOwner != owner) return;
            _pickupOwner = null;
            SetActive(pickupHint, false);
        }
    }

    public void SetRecorderHint(object owner, bool show)
    {
        if (show)
        {
            _doorOwner = owner;
            SetActive(recorderHint, true);
            SetActive(doorOpenHint, false);
            SetActive(doorCloseHint, false);
        }
        else
        {
            if (_doorOwner != owner) return;
            _doorOwner = null;
            SetActive(recorderHint, false);
        }
    }

    public void SetBloodHint(object owner, bool show)
    {
        if (show)
        {
            _bloodOwner = owner;
            SetActive(bloodHint, true);
        }
        else
        {
            if (_bloodOwner != owner) return;
            _bloodOwner = null;
            SetActive(bloodHint, false);
        }
    }

    public void SetInventoryHint(bool isOpen, bool isBlocked)
    {
        if (isBlocked)
        {
            SetActive(inventoryOpenHint, false);
            SetActive(inventoryCloseHint, false);
            return;
        }

        SetActive(inventoryOpenHint, !isOpen);
        SetActive(inventoryCloseHint, isOpen);
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }
}
