using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    public Transform doorPanel;
    public Transform secondPanel;
    public InputActionReference interactAction;
    public float openAngle = 90f;
    public float openSpeed = 60f;
    public float interactionDistance = 3f;
    public bool flipHinge = false;
    public float hingePivotInset = 0.03f;

    private bool _isOpen;
    private bool _isMoving;
    private float _currentAngle;
    private bool _interactPressed;
    private bool _isLookedAt;
    private Vector3 _hingePivot;
    private Vector3 _secondHinge;
    private Camera _cam;

    private readonly List<Transform> _leftGroup  = new List<Transform>();
    private readonly List<Transform> _rightGroup = new List<Transform>();

    private void Awake()
    {
        if (doorPanel == null)
            doorPanel = transform.GetChild(0);
    }

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl != null) _cam = pl.GetComponentInChildren<Camera>();
        if (_cam == null) _cam = Camera.main;

        Renderer r  = doorPanel.GetComponent<Renderer>();
        Renderer r2 = secondPanel != null ? secondPanel.GetComponent<Renderer>() : null;

        if (r != null)
        {
            Vector3 reference = r2 != null ? r2.bounds.center : transform.position;
            _hingePivot = (flipHinge && r2 == null)
                ? ClosestCorner(r.bounds, reference)
                : FarthestCorner(r.bounds, reference);
            Inset(ref _hingePivot, r.bounds.center);
            ForceDynamic(doorPanel);
        }
        else { _hingePivot = doorPanel.position; }

        if (r2 != null)
        {
            _secondHinge = FarthestCorner(r2.bounds, r != null ? r.bounds.center : transform.position);
            Inset(ref _secondHinge, r2.bounds.center);
            ForceDynamic(secondPanel);
        }

        _leftGroup.Add(doorPanel);
        if (secondPanel != null) _rightGroup.Add(secondPanel);

        Vector3 leftCenter  = r  != null ? r.bounds.center  : doorPanel.position;
        Vector3 rightCenter = r2 != null ? r2.bounds.center : (secondPanel != null ? secondPanel.position : leftCenter);

        foreach (Transform child in transform)
        {
            if (child == doorPanel || child == secondPanel) continue;
            if (child.name.Contains("Frame")) continue;

            Renderer cr = child.GetComponent<Renderer>();
            if (cr == null) continue;

            float dLeft  = Vector3.Distance(cr.bounds.center, leftCenter);
            float dRight = secondPanel != null ? Vector3.Distance(cr.bounds.center, rightCenter) : float.MaxValue;

            if (dLeft <= dRight) { _leftGroup.Add(child);  ForceDynamic(child); }
            else                 { _rightGroup.Add(child); ForceDynamic(child); }
        }
    }

    private void Inset(ref Vector3 pivot, Vector3 center)
    {
        if (hingePivotInset <= 0f) return;
        Vector3 d = center - pivot; d.y = 0f;
        if (d.sqrMagnitude > 0.0001f) pivot += d.normalized * hingePivotInset;
    }

    private static void ForceDynamic(Transform t)
    {
        foreach (var mf in t.GetComponentsInChildren<MeshFilter>(true))
            if (mf.sharedMesh != null) { var _ = mf.mesh; }
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.started += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.started -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext ctx) => _interactPressed = true;

    private void Update()
    {
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        _isLookedAt = Physics.Raycast(ray, out RaycastHit hit, interactionDistance)
                      && IsDoorTransform(hit.transform);

        if (_interactPressed)
        {
            _interactPressed = false;
            if (_isLookedAt && !_isMoving)
                StartCoroutine(_isOpen ? Close() : Open());
        }
    }

    private bool IsDoorTransform(Transform t)
    {
        foreach (var m in _leftGroup)  if (t == m || t.IsChildOf(m)) return true;
        foreach (var m in _rightGroup) if (t == m || t.IsChildOf(m)) return true;
        return false;
    }

    private IEnumerator Open()
    {
        _isMoving = true;
        float remaining = openAngle - _currentAngle;

        while (remaining > 0.01f)
        {
            float step = Mathf.Min(openSpeed * Time.deltaTime, remaining);
            RotateGroup(_leftGroup,  _hingePivot,  -step);
            RotateGroup(_rightGroup, _secondHinge,  step);
            _currentAngle += step;
            remaining -= step;
            yield return null;
        }

        float diff = openAngle - _currentAngle;
        RotateGroup(_leftGroup,  _hingePivot,  -diff);
        RotateGroup(_rightGroup, _secondHinge,  diff);
        _currentAngle = openAngle;
        _isOpen = true;
        _isMoving = false;
    }

    private IEnumerator Close()
    {
        _isMoving = true;
        float remaining = _currentAngle;

        while (remaining > 0.01f)
        {
            float step = Mathf.Min(openSpeed * Time.deltaTime, remaining);
            RotateGroup(_leftGroup,  _hingePivot,   step);
            RotateGroup(_rightGroup, _secondHinge, -step);
            _currentAngle -= step;
            remaining -= step;
            yield return null;
        }

        RotateGroup(_leftGroup,  _hingePivot,   _currentAngle);
        RotateGroup(_rightGroup, _secondHinge, -_currentAngle);
        _currentAngle = 0f;
        _isOpen = false;
        _isMoving = false;
    }

    private static void RotateGroup(List<Transform> group, Vector3 pivot, float degrees)
    {
        foreach (var t in group) t.RotateAround(pivot, Vector3.up, degrees);
    }

    private static Vector3 FarthestCorner(Bounds b, Vector3 r)
    {
        Vector3 best = b.min; float bestD = -1f;
        foreach (var c in Corners(b))
        {
            float d = (c.x - r.x) * (c.x - r.x) + (c.z - r.z) * (c.z - r.z);
            if (d > bestD) { bestD = d; best = c; }
        }
        return best;
    }

    private static Vector3 ClosestCorner(Bounds b, Vector3 r)
    {
        Vector3 best = b.min; float bestD = float.MaxValue;
        foreach (var c in Corners(b))
        {
            float d = (c.x - r.x) * (c.x - r.x) + (c.z - r.z) * (c.z - r.z);
            if (d < bestD) { bestD = d; best = c; }
        }
        return best;
    }

    private static Vector3[] Corners(Bounds b) => new[]
    {
        new Vector3(b.min.x, b.min.y, b.min.z), new Vector3(b.min.x, b.min.y, b.max.z),
        new Vector3(b.max.x, b.min.y, b.min.z), new Vector3(b.max.x, b.min.y, b.max.z),
    };
}
