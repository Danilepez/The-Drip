using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleDoorController : MonoBehaviour
{
    public Transform leftPivot;
    public Transform rightPivot;

    public float leftOpenAngle = -90f;
    public float rightOpenAngle = 90f;

    public float openSpeed = 2f;
    public float lookRange = 3f;
    public InputActionReference interactAction;

    private Quaternion _leftClosed, _leftOpen;
    private Quaternion _rightClosed, _rightOpen;

    private Coroutine _currentCoroutine;
    private Camera _cam;
    private bool _isLookedAt = false;
    private bool _isOpen = false;

    private void OnEnable()  => interactAction?.action?.Enable();
    private void OnDisable() => interactAction?.action?.Disable();

    private void Start()
    {
        _leftClosed  = leftPivot.rotation;
        _leftOpen    = Quaternion.Euler(leftPivot.eulerAngles  + new Vector3(0, leftOpenAngle,  0));

        _rightClosed = rightPivot.rotation;
        _rightOpen   = Quaternion.Euler(rightPivot.eulerAngles + new Vector3(0, rightOpenAngle, 0));

        PlayerLook player = FindAnyObjectByType<PlayerLook>();
        _cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
    }

    private void Update()
    {
        CheckLook();
        TryInteract();
    }

    private void CheckLook()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            bool hitThisDoor = hit.collider.GetComponentInParent<DoubleDoorController>() == this;

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
        _currentCoroutine = StartCoroutine(ToggleDoors());
    }

    private IEnumerator ToggleDoors()
    {
        Quaternion leftTarget  = _isOpen ? _leftClosed  : _leftOpen;
        Quaternion rightTarget = _isOpen ? _rightClosed : _rightOpen;
        _isOpen = !_isOpen;

        while (Quaternion.Angle(leftPivot.rotation, leftTarget) > 0.01f ||
               Quaternion.Angle(rightPivot.rotation, rightTarget) > 0.01f)
        {
            leftPivot.rotation  = Quaternion.Lerp(leftPivot.rotation,  leftTarget,  Time.deltaTime * openSpeed);
            rightPivot.rotation = Quaternion.Lerp(rightPivot.rotation, rightTarget, Time.deltaTime * openSpeed);
            yield return null;
        }

        leftPivot.rotation  = leftTarget;
        rightPivot.rotation = rightTarget;
    }
}
