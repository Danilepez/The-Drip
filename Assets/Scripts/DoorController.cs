using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    public float openAngle = -90f;
    public float openSpeed = 2f;
    public bool isOpen = false;
    public float lookRange = 3f;
    public InputActionReference interactAction;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    private Camera _cam;
    private bool _isLookedAt = false;

    private void OnEnable()
    {
        interactAction?.action?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.action?.Disable();
    }

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        PlayerLook player = FindAnyObjectByType<PlayerLook>();
        _cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
    }

    void Update()
    {
        CheckLook();
        TryInteract();
    }

    private void CheckLook()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            bool hitThisDoor = hit.collider.GetComponentInParent<DoorController>() == this;

            if (hitThisDoor)
            {
                if (!_isLookedAt)
                {
                    _isLookedAt = true;
                    HintTextUI.Instance?.Show(this, "F  Abrir / Cerrar");
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
        else
        {
            if (_isLookedAt)
            {
                _isLookedAt = false;
                HintTextUI.Instance?.Hide(this);
            }
        }
    }

    private void TryInteract()
    {
        if (!_isLookedAt) return;
        if (interactAction == null || !interactAction.action.WasPressedThisFrame()) return;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
}
