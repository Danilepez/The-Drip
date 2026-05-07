using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static bool IsFrozen = false;

    public float movementSpeed = 5f;
    public float sprintSpeed = 7.5f;
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    public AudioClip footSteps;
    private AudioSource footstepSource;

    public AudioClip sprintSound;
    private AudioSource sprintSource;

    private Vector2 movementInput;
    private Rigidbody rb;

    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    private float jumpForce = 5f;

    private bool isSprinting;

    private void OnEnable()
    {
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
        sprintAction?.action?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action?.Disable();
        jumpAction?.action?.Disable();
        sprintAction?.action?.Disable();
    }

    void Start()
    {
        IsFrozen = false;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requiere un Rigidbody en el objeto Player.");
            enabled = false;
            return;
        }
        

        footstepSource = gameObject.AddComponent<AudioSource>();
        sprintSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footSteps;
        sprintSource.clip = sprintSound;

        footstepSource.loop = true;
        footstepSource.playOnAwake = false;

        sprintSource.loop = true;
        sprintSource.playOnAwake = false;
        sprintSource.volume = 0.9f;

        footstepSource.spatialBlend = 1f;
        footstepSource.volume = 0.9f;
        footstepSource.pitch = 1f;

        footstepSource.minDistance = 1f;
        footstepSource.maxDistance = 15f;

    }

    void Update()
    {
        if (IsFrozen)
        {
            movementInput = Vector2.zero;
            if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
            if (sprintSource   != null && sprintSource.isPlaying)   sprintSource.Stop();
            return;
        }

        if (moveAction != null && moveAction.action != null)
        {
            movementInput = moveAction.action.ReadValue<Vector2>();
        }

        isSprinting = sprintAction != null && sprintAction.action != null && sprintAction.action.IsPressed();

        if (jumpAction != null && jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
        {
            TryJump();
        }

        bool isMoving = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).sqrMagnitude > 0.01f;

        if (isGrounded && isMoving)
        {
            if (isSprinting)
            {
                if (footstepSource.isPlaying) footstepSource.Stop();

                if (!sprintSource.isPlaying)
                {
                    Debug.Log("Playing sprint sound");
                    sprintSource.Play();
                }
            }
            else
            { 
                if (sprintSource.isPlaying) sprintSource.Stop();

                if (!footstepSource.isPlaying)
                {
                    Debug.Log("Playing footstep sound");
                    footstepSource.Play();
                }
            }
        }
        else
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            if (sprintSource.isPlaying) sprintSource.Stop();
        }
    }

    private void FixedUpdate()
    {
        checkPlayerIsGrounded();
        MovePlayer();
    }

    public void OnMovement(InputValue data)
    {
        movementInput = data.Get<Vector2>();
    }

    public void OnJump(InputValue data)
    {
        if (data.isPressed)
        {
            TryJump();
        }
    }

    private void TryJump()
    {
        if (!isGrounded)
        {
            return;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
    }

    public void MovePlayer()
    {
        if (IsFrozen)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }
        Vector3 direction = transform.right * movementInput.x  + transform.forward * movementInput.y;
        float currentSpeed = isSprinting ? sprintSpeed : movementSpeed;
        rb.linearVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
    }

    private void checkPlayerIsGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            return;
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance + 0.2f, groundMask, QueryTriggerInteraction.Ignore);
    }
}