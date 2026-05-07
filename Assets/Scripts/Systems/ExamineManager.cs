using UnityEngine;
using UnityEngine.InputSystem;

public class ExamineManager : MonoBehaviour
{
    public static ExamineManager Instance { get; private set; }

    public InputActionReference examineAction;
    public InputActionReference exitAction;
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference zoomAction;

    public float lookRange = 3f;
    public float moveSpeed = 1.5f;
    public float lookSensitivity = 150f;
    public float minPitch = -30f;
    public float maxPitch = 30f;
    public float minYaw = -90f;
    public float maxYaw = 90f;
    public float zoomSpeed = 0.02f;
    public bool limitZoomDistance = true;
    public float minZoomDistance = 0.2f;
    public float maxZoomDistance = 1.5f;
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
    private float _yawCenter;
    private Transform _examineAnchor;
    private float _activeMinPitch;
    private float _activeMaxPitch;
    private float _activeMinYaw;
    private float _activeMaxYaw;
    private bool _clampPitchToBounds;

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
        zoomAction?.action?.Enable();
    }

    private void OnDisable()
    {
        examineAction?.action?.Disable();
        exitAction?.action?.Disable();
        moveAction?.action?.Disable();
        lookAction?.action?.Disable();
        zoomAction?.action?.Disable();
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
            HandleExamineZoom();
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
        ApplyLookLimits(target);
        SetLookedTarget(null);

        _originalParent = _camTransform.parent;
        _originalPosition = _camTransform.position;
        _originalRotation = _camTransform.rotation;

        Transform anchor = target.cameraAnchor != null ? target.cameraAnchor : target.transform;
        _examineAnchor = anchor;
        _camTransform.SetParent(null, true);
        _camTransform.position = anchor.position;
        _examinePitch = NormalizeAngle(anchor.eulerAngles.x);
        _examineYaw = anchor.eulerAngles.y;
        _examineRoll = anchor.eulerAngles.z;
        _yawCenter = _examineYaw;
        ClampPitchToBounds();
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
        _examineAnchor = null;
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
        ClampPitchToBounds();
        _camTransform.rotation = Quaternion.Euler(_examinePitch, _examineYaw, _examineRoll);
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
        _examineYaw += input.x * lookSensitivity * Time.deltaTime;

        float minYawAbs = _yawCenter + _activeMinYaw;
        float maxYawAbs = _yawCenter + _activeMaxYaw;
        _examineYaw = Mathf.Clamp(_examineYaw, minYawAbs, maxYawAbs);
        _examinePitch = Mathf.Clamp(_examinePitch, _activeMinPitch, _activeMaxPitch);
        ClampPitchToBounds();
        _camTransform.rotation = Quaternion.Euler(_examinePitch, _examineYaw, _examineRoll);
    }

    private void HandleExamineZoom()
    {
        float scroll = 0f;

        if (zoomAction != null && zoomAction.action != null)
        {
            scroll = zoomAction.action.ReadValue<float>();
        }
        else if (Mouse.current != null)
        {
            scroll = Mouse.current.scroll.ReadValue().y;
        }

        if (Mathf.Abs(scroll) < 0.01f) return;

        Vector3 nextPos = _camTransform.position + _camTransform.forward * (scroll * zoomSpeed);

        if (limitZoomDistance && _examineAnchor != null)
        {
            Vector3 fromAnchor = nextPos - _examineAnchor.position;
            float dist = fromAnchor.magnitude;
            if (dist > 0.0001f)
            {
                dist = Mathf.Clamp(dist, minZoomDistance, maxZoomDistance);
                nextPos = _examineAnchor.position + fromAnchor.normalized * dist;
            }
        }

        if (_currentTarget != null && _currentTarget.movementBounds != null)
        {
            nextPos = ClampToBounds(nextPos, _currentTarget.movementBounds);
        }

        _camTransform.position = nextPos;
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

    private void ClampPitchToBounds()
    {
        if (!_clampPitchToBounds) return;

        float minBound;
        float maxBound;
        if (!TryGetPitchLimitsFromBounds(out minBound, out maxBound)) return;

        if (minBound > maxBound)
        {
            float swap = minBound;
            minBound = maxBound;
            maxBound = swap;
        }

        _examinePitch = Mathf.Clamp(_examinePitch, minBound, maxBound);
    }

    private bool TryGetPitchLimitsFromBounds(out float minPitchOut, out float maxPitchOut)
    {
        minPitchOut = 0f;
        maxPitchOut = 0f;

        if (_currentTarget == null || _currentTarget.movementBounds == null) return false;
        if (_camTransform == null) return false;

        BoxCollider bounds = _currentTarget.movementBounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.size * 0.5f;

        Quaternion yawRoll = Quaternion.Euler(0f, _examineYaw, _examineRoll);
        Quaternion invYawRoll = Quaternion.Inverse(yawRoll);

        bool hasValue = false;

        for (int xi = -1; xi <= 1; xi += 2)
        for (int yi = -1; yi <= 1; yi += 2)
        for (int zi = -1; zi <= 1; zi += 2)
        {
            Vector3 localCorner = center + new Vector3(extents.x * xi, extents.y * yi, extents.z * zi);
            Vector3 worldCorner = bounds.transform.TransformPoint(localCorner);
            Vector3 dir = worldCorner - _camTransform.position;
            if (dir.sqrMagnitude < 0.0001f) continue;

            Vector3 localDir = invYawRoll * dir.normalized;
            if (localDir.z <= 0.01f) continue;

            float pitch = Mathf.Atan2(-localDir.y, localDir.z) * Mathf.Rad2Deg;

            if (!hasValue)
            {
                minPitchOut = pitch;
                maxPitchOut = pitch;
                hasValue = true;
            }
            else
            {
                minPitchOut = Mathf.Min(minPitchOut, pitch);
                maxPitchOut = Mathf.Max(maxPitchOut, pitch);
            }
        }

        return hasValue;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private void ApplyLookLimits(ExamineTarget target)
    {
        _activeMinPitch = minPitch;
        _activeMaxPitch = maxPitch;
        _activeMinYaw = minYaw;
        _activeMaxYaw = maxYaw;
        _clampPitchToBounds = true;

        if (target == null) return;

        if (target.useCustomYawLimits)
        {
            _activeMinYaw = target.minYaw;
            _activeMaxYaw = target.maxYaw;
        }

        if (target.useCustomPitchLimits)
        {
            _activeMinPitch = target.minPitch;
            _activeMaxPitch = target.maxPitch;
        }

        _clampPitchToBounds = target.clampPitchToBounds;
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
