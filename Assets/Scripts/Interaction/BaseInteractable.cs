using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseInteractable : MonoBehaviour
{
    public float lookRange = 3f;
    public InputActionReference interactAction;
    public LayerMask interactMask = ~0;
    public bool requireExamineMode = false;
    public bool showHint = true;

    protected Coroutine _currentCoroutine;
    protected Camera _cam;
    private bool _isLookedAt = false;
    private Highlightable _highlightable;

    /// <summary>Bloquea la interacción sin deshabilitar el componente ni el InputAction.</summary>
    public bool isLocked = false;

    [Tooltip("Si está marcado, la puerta/interactable empieza bloqueado al iniciar la escena.")]
    public bool startLocked = false;

    protected virtual void Awake()
    {
        isLocked = startLocked;
    }

    protected virtual void OnEnable()  => interactAction?.action?.Enable();
    protected virtual void OnDisable() => interactAction?.action?.Disable();

    protected virtual void Start()
    {
        PlayerLook player = FindAnyObjectByType<PlayerLook>();
        _cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
        ConfigureDefaultMask();
        _highlightable = GetComponentInChildren<Highlightable>();
        Init();
    }

    protected virtual void Update()
    {
        CheckLook();
        TryInteract();
    }

    protected abstract void Init();

    protected abstract IEnumerator Interact();

    protected virtual bool TryGetOpenState(out bool isOpen)
    {
        isOpen = false;
        return false;
    }

    private void CheckLook()
    {
        if (_cam == null) return;
        if (requireExamineMode && !ExamineManager.IsExamining)
        {
            SetLookState(false);
            return;
        }

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange, interactMask, QueryTriggerInteraction.Ignore))
        {
            bool hitThis = hit.collider.GetComponentInParent<BaseInteractable>() == this;
            SetLookState(hitThis);
        }
        else
        {
            SetLookState(false);
        }
    }

    private void SetLookState(bool isLookedAt)
    {
        if (_isLookedAt == isLookedAt) return;
        _isLookedAt = isLookedAt;
        _highlightable?.SetHighlighted(isLookedAt);

        if (showHint)
        {
            bool isOpen;
            bool hasState = TryGetOpenState(out isOpen);
            if (!hasState) isOpen = false;
            InputHintsUI.Instance?.SetDoorHint(this, isLookedAt, isOpen);
        }

        if (!isLookedAt) HintTextUI.Instance?.Hide(this);
    }

    protected void RefreshHint()
    {
        if (!_isLookedAt || !showHint) return;

        bool isOpen;
        bool hasState = TryGetOpenState(out isOpen);
        if (!hasState) isOpen = false;
        InputHintsUI.Instance?.SetDoorHint(this, true, isOpen);
    }

    private void TryInteract()
    {
        if (!_isLookedAt) return;
        if (isLocked) return;
        if (requireExamineMode && !ExamineManager.IsExamining) return;
        if (interactAction == null || !interactAction.action.WasPressedThisFrame()) return;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(Interact());
    }

    private void ConfigureDefaultMask()
    {
        if (interactMask.value != ~0) return;

        int examineLayer = LayerMask.NameToLayer("Examine");
        if (examineLayer >= 0)
        {
            interactMask = ~(1 << examineLayer);
        }
    }
}
