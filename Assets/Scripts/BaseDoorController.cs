using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseDoorController : MonoBehaviour
{
    public float openSpeed = 2f;
    public float lookRange = 3f;
    public InputActionReference interactAction;

    protected Coroutine _currentCoroutine;
    protected Camera _cam;
    private bool _isLookedAt = false;

    private void OnEnable()  => interactAction?.action?.Enable();
    private void OnDisable() => interactAction?.action?.Disable();

    private void Start()
    {
        PlayerLook player = FindAnyObjectByType<PlayerLook>();
        _cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
        Init();
    }

    private void Update()
    {
        CheckLook();
        TryInteract();
    }

    protected abstract void Init();

    protected abstract IEnumerator Toggle();

    private void CheckLook()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            bool hitThisDoor = hit.collider.GetComponentInParent<BaseDoorController>() == this;

            if (true)
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
        _currentCoroutine = StartCoroutine(Toggle());
    }
}
