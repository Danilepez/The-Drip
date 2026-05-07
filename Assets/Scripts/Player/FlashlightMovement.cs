using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightMovement : MonoBehaviour
{
    public Animator flashlight;

    public InputActionReference flashlightWalkAction;
    public InputActionReference flashlightSprintAction;

    void Start()
    {
        flashlight = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        flashlightSprintAction.action.Enable();
        flashlightWalkAction.action.Enable();
    }

    private void OnDisable()
    {
        flashlightSprintAction.action.Disable();
        flashlightWalkAction.action.Disable();
    }

    void Update()
    {
        Vector2 moveInput = flashlightWalkAction.action.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isSprinting = flashlightSprintAction.action.IsPressed();

        if (isMoving)
        {
            if (isSprinting)
            {
                flashlight.ResetTrigger("walk");
                flashlight.SetTrigger("sprint");
            }
            else
            {
                flashlight.ResetTrigger("sprint");
                flashlight.SetTrigger("walk");
            }
        }
        else
        {
            flashlight.ResetTrigger("sprint");
            flashlight.ResetTrigger("walk");
        }

        if (isSprinting && !isMoving)
        {
            flashlight.ResetTrigger("sprint");
            flashlight.SetTrigger("walk");
        }
    }
}
