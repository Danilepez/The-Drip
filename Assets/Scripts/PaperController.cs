using UnityEngine;
using UnityEngine.InputSystem;

public class PaperController : MonoBehaviour
{
    public InputActionReference collectAction;
    public float lookRange = 3f;

    private Camera _cam;
    private bool _isLookedAt;

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        _cam = pl != null ? pl.GetComponentInChildren<Camera>() : Camera.main;
    }

    private void Update()
    {
        CheckLook();
        TryCollect();
    }

    private void CheckLook()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange) && hit.collider.gameObject == gameObject)
        {
            if (!_isLookedAt)
            {
                _isLookedAt = true;
                HintTextUI.Instance?.Show(this, "F  Recoger");
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
        if (collectAction == null || !collectAction.action.WasPressedThisFrame()) return;

        HintTextUI.Instance?.Hide(this);
        PaperInventory.Instance?.AddOne();
        Destroy(gameObject);
    }
}




