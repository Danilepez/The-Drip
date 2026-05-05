using UnityEngine;
using UnityEngine.InputSystem;

public class ExamineManager : MonoBehaviour
{
    public static ExamineManager Instance { get; private set; }

    public InputActionReference examineAction;
    public InputActionReference exitAction;
    public InputActionReference moveAction;
    public InputActionReference lookAction;

    public float lookRange = 3f;
    public float moveSpeed = 1.5f;
    public float lookSensitivity = 150f;
    public float minPitch = -30f;
    public float maxPitch = 30f;
    public LayerMask examineMask = ~0;

    private Camera _cam;
    private Transform _camTransform;
    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isExamining;
    private ExamineTarget _currentTarget;
    private Renderer[] _playerRenderers;
    private Collider[] _playerColliders;
    private ExamineTarget _lookedTarget;
    private float _examinePitch;
    private float _examineYaw;
    private float _examineRoll;

    public static bool IsExamining => Instance != null && Instance._isExamining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        examineAction?.action?.Enable();
        exitAction?.action?.Enable();
        moveAction?.action?.Enable();
        lookAction?.action?.Enable();
    }

    private void OnDisable()
    {
        examineAction?.action?.Disable();
        exitAction?.action?.Disable();
        moveAction?.action?.Disable();
        lookAction?.action?.Disable();
    }

    private void Start()
    {
        PlayerLook player = FindAnyObjectByType<PlayerLook>();
        _cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
        _camTransform = _cam != null ? _cam.transform : null;

        GameObject playerRoot = null;
        PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
        if (pm != null) playerRoot = pm.gameObject;
        else if (player != null) playerRoot = player.gameObject;

        if (playerRoot != null)
        {
            _playerRenderers = playerRoot.GetComponentsInChildren<Renderer>(true);
            _playerColliders = playerRoot.GetComponentsInChildren<Collider>(true);
        }
    }

    private void Update()
    {
        if (_camTransform == null) return;

        if (!_isExamining)
        {
            ExamineTarget looked = GetLookedExamineTarget();
            SetLookedTarget(looked);

            if (looked != null && examineAction != null && examineAction.action.WasPressedThisFrame())
            {
                EnterExamine(looked);
            }
        }
        else
        {
            HandleExamineLook();
            HandleExamineMovement();
            TryExitExamine();
        }
    }

    private void TryExitExamine()
    {
        if (exitAction == null || !exitAction.action.WasPressedThisFrame()) return;
        ExitExamine();
    }

    private void EnterExamine(ExamineTarget target)
    {
        _isExamining = true;
        _currentTarget = target;
        SetLookedTarget(null);

        _originalParent = _camTransform.parent;
        _originalPosition = _camTransform.position;
        _originalRotation = _camTransform.rotation;

        Transform anchor = target.cameraAnchor != null ? target.cameraAnchor : target.transform;
        _camTransform.SetParent(null, true);
        _camTransform.position = anchor.position;
        _examinePitch = NormalizeAngle(anchor.eulerAngles.x);
        _examineYaw = anchor.eulerAngles.y;
        _examineRoll = anchor.eulerAngles.z;
        _camTransform.rotation = Quaternion.Euler(_examinePitch, _examineYaw, _examineRoll);

        SetPlayerVisible(false);
        SetBoundsCollisionIgnored(true);
        InputHintsUI.Instance?.SetExamineExitHint(true);
        InventoryManager.Instance?.SetBlocked(true);
        PlayerLook.IsFrozen = true;
        PlayerMovement.IsFrozen = true;
    }

    private void ExitExamine()
    {
        _isExamining = false;
        SetBoundsCollisionIgnored(false);
        _currentTarget = null;
        SetLookedTarget(null);

        _camTransform.SetParent(_originalParent, true);
        _camTransform.position = _originalPosition;
        _camTransform.rotation = _originalRotation;

        SetPlayerVisible(true);
        InputHintsUI.Instance?.SetExamineExitHint(false);
        InventoryManager.Instance?.SetBlocked(false);
        PlayerLook.IsFrozen = false;
        PlayerMovement.IsFrozen = false;
    }

    private void HandleExamineMovement()
    {
        if (moveAction == null || moveAction.action == null) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 right = _camTransform.right;
        Vector3 up = Vector3.up;

        Vector3 delta = (right * input.x + up * input.y) * (moveSpeed * Time.deltaTime);
        Vector3 nextPos = _camTransform.position + delta;

        if (_currentTarget != null && _currentTarget.movementBounds != null)
        {
            nextPos = ClampToBounds(nextPos, _currentTarget.movementBounds);
        }

        _camTransform.position = nextPos;
    }

    private ExamineTarget GetLookedExamineTarget()
    {
        Ray ray = new Ray(_camTransform.position, _camTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, lookRange, examineMask, QueryTriggerInteraction.Collide))
        {
            return hit.collider.GetComponentInParent<ExamineTarget>();
        }
        return null;
    }

    private void SetLookedTarget(ExamineTarget target)
    {
        if (_lookedTarget == target) return;

        if (_lookedTarget != null)
        {
            _lookedTarget.SetHighlighted(false);
            InputHintsUI.Instance?.SetEnterExamineHint(_lookedTarget, false);
        }
        _lookedTarget = target;
        if (_lookedTarget != null)
        {
            _lookedTarget.SetHighlighted(true);
            InputHintsUI.Instance?.SetEnterExamineHint(_lookedTarget, true);
        }
    }

    private void HandleExamineLook()
    {
        Vector2 input = Vector2.zero;

        if (lookAction != null && lookAction.action != null)
        {
            input = lookAction.action.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            input = Mouse.current.delta.ReadValue();
        }

        if (input == Vector2.zero) return;

        _examinePitch -= input.y * lookSensitivity * Time.deltaTime;
        _examinePitch = Mathf.Clamp(_examinePitch, minPitch, maxPitch);
        _camTransform.rotation = Quaternion.Euler(_examinePitch, _examineYaw, _examineRoll);
    }

    private void SetPlayerVisible(bool visible)
    {
        if (_playerRenderers == null) return;

        foreach (Renderer r in _playerRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void SetBoundsCollisionIgnored(bool ignore)
    {
        if (_currentTarget == null || _currentTarget.movementBounds == null) return;
        if (_playerColliders == null) return;

        Collider[] boundsColliders = _currentTarget.movementBounds.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in _playerColliders)
        {
            if (c == null) continue;
            foreach (Collider b in boundsColliders)
            {
                if (b != null) Physics.IgnoreCollision(c, b, ignore);
            }
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private Vector3 ClampToBounds(Vector3 pos, BoxCollider bounds)
    {
        Vector3 local = bounds.transform.InverseTransformPoint(pos);
        Vector3 half = bounds.size * 0.5f;
        Vector3 center = bounds.center;
        Vector3 min = center - half;
        Vector3 max = center + half;

        local = new Vector3(
            Mathf.Clamp(local.x, min.x, max.x),
            Mathf.Clamp(local.y, min.y, max.y),
            Mathf.Clamp(local.z, min.z, max.z)
        );

        return bounds.transform.TransformPoint(local);
    }
}
