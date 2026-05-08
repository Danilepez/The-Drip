using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 150f;
    public Transform playerCamera;
    public InputActionReference lookAction;

    public static bool IsFrozen = false;

    private float xRotation = 0f;
    private Vector2 mouseInput;

    private void OnEnable()
    {
        lookAction?.action?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action?.Disable();
    }

    void Start()
    {
        IsFrozen = false;

        if (playerCamera == null)
        {
            Camera cameraComponent = GetComponentInChildren<Camera>();
            if (cameraComponent != null)
            {
                playerCamera = cameraComponent.transform;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (lookAction != null && lookAction.action != null)
        {
            mouseInput = lookAction.action.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            mouseInput = Mouse.current.delta.ReadValue();
        }

        LookAround();
    }

    public void OnLook(InputValue data)
    {
        mouseInput = data.Get<Vector2>();
    }

    public void LookAround()
    {
        if (playerCamera == null || IsFrozen)
        {
            return;
        }

        xRotation -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseInput.x * mouseSensitivity * Time.deltaTime);
    }

    public static void ForceLookAt(Vector3 worldTarget)
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl == null || pl.playerCamera == null) return;

        Vector3 toTarget = worldTarget - pl.transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.001f)
            pl.transform.rotation = Quaternion.LookRotation(toTarget);

        Vector3 toCam = worldTarget - pl.playerCamera.position;
        float pitch = -Mathf.Atan2(toCam.y, new Vector2(toCam.x, toCam.z).magnitude) * Mathf.Rad2Deg;
        pl.xRotation = Mathf.Clamp(pitch, -90f, 90f);
        pl.playerCamera.localRotation = Quaternion.Euler(pl.xRotation, 0f, 0f);
    }
}