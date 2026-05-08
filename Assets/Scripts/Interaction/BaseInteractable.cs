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

    public bool isLocked = false;

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

        if (isLookedAt)
            Debug.Log($"[BaseInteractable] Mirando '{gameObject.name}' | isLocked={isLocked} | requireExamine={requireExamineMode} | action={(interactAction == null ? "NULL" : interactAction.action.name)} | actionEnabled={interactAction?.action?.enabled}");

        if (showHint)
        {
            bool isOpen;
            bool hasState = TryGetOpenState(out isOpen);
            if (!hasState) isOpen = false;
            ShowInteractHint(isLookedAt, isOpen);
        }

        if (!isLookedAt) HintTextUI.Instance?.Hide(this);
    }

    protected virtual void ShowInteractHint(bool isLookedAt, bool isOpen)
    {
        InputHintsUI.Instance?.SetDoorHint(this, isLookedAt, isOpen);
    }

    protected void RefreshHint()
    {
        if (!_isLookedAt || !showHint) return;

        bool isOpen;
        bool hasState = TryGetOpenState(out isOpen);
        if (!hasState) isOpen = false;
        ShowInteractHint(true, isOpen);
    }

    private void TryInteract()
    {
        if (!_isLookedAt) return;
        if (isLocked)
        {
            if (interactAction != null && interactAction.action.WasPressedThisFrame())
                Debug.Log($"[BaseInteractable] '{gameObject.name}' BLOQUEADO (isLocked=true)");
            return;
        }
        if (requireExamineMode && !ExamineManager.IsExamining)
        {
            if (interactAction != null && interactAction.action.WasPressedThisFrame())
                Debug.Log($"[BaseInteractable] '{gameObject.name}' requiere ExamineMode");
            return;
        }
        if (interactAction == null)
        {
            Debug.LogError($"[BaseInteractable] '{gameObject.name}' — interactAction es NULL. Asígnalo en el Inspector.");
            return;
        }
        if (!interactAction.action.enabled)
        {
            Debug.LogWarning($"[BaseInteractable] '{gameObject.name}' — action '{interactAction.action.name}' está DISABLED.");
            return;
        }
        if (!interactAction.action.WasPressedThisFrame()) return;

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
