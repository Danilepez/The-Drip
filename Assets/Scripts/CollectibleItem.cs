using UnityEngine;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;
    public InputActionReference collectAction;
    public float lookRange = 3f;

    private Camera _cam;
    private bool _isLookedAt;

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        _cam = pl != null ? pl.GetComponentInChildren<Camera>() : Camera.main;
        Debug.Log($"[Collectible] '{gameObject.name}' Start. cam={(_cam == null ? "NULL" : _cam.name)} | collectAction={( collectAction == null ? "NULL" : collectAction.action.name)} | collider={GetComponent<Collider>() != null}");
    }

    private void Update()
    {
        CheckLook();
        TryCollect();
    }

    private void CheckLook()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * lookRange, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!_isLookedAt)
                {
                    _isLookedAt = true;
                    Debug.Log($"[Collectible] Mirando '{gameObject.name}'");
                    HintTextUI.Instance?.Show(this, $"F  Recoger {itemData.itemName}");
                }
            }
            else
            {
                if (_isLookedAt)
                {
                    Debug.Log($"[Collectible] Rayo golpeo '{hit.collider.gameObject.name}', no el item.");
                    _isLookedAt = false;
                    HintTextUI.Instance?.Hide(this);
                }
            }
        }
        else
        {
            if (_isLookedAt)
            {
                _isLookedAt = false;
                HintTextUI.Instance?.Hide(this);
            }
        }
    }

    private void TryCollect()
    {
        if (!_isLookedAt) return;

        if (collectAction == null)
        {
            Debug.LogError("[Collectible] collectAction es NULL.");
            return;
        }

        if (collectAction.action.WasPressedThisFrame())
        {
            Debug.Log("[Collectible] F presionado → recogiendo.");
            HintTextUI.Instance?.Hide(this);
            InventoryManager.Instance?.AddItem(itemData, quantity);
            Destroy(gameObject);
        }
    }
}
