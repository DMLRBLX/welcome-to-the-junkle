using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Usage:
    // - Create an Input Action asset with a Vector2 "Move" action (e.g., from WASD / left stick).
    // - Add a PlayerInput component to the player GameObject and assign the action asset.
    // - Set PlayerInput's Behavior to "Invoke C# Events" (or "Send Messages").
    // - Wire the Move action -> OnMove (this exact method name) in the PlayerInput events.
    // - Assign the scene camera to the `cameraTransform` field or leave empty to use Camera.main.
    // Notes:
    // - This script uses a CharacterController to move. Make sure the GameObject has one.
    // - Jump is provided as a stub `OnJump` event for later implementation.
    [Header("Movement")]
    [SerializeField] bool arduinoPlayer = false;
    [SerializeField] float moveSpeed = 5f;
    public float gravity = -9.81f;
    [Tooltip("Additional constant downward acceleration to ensure player falls off edges.")]
    [SerializeField] float downwardForce = -2f;
    [SerializeField] float rotationSmoothTime = 0.1f;
    [SerializeField] float jumpForce = 5f;

    // Stored camera input (can be set from external sources like Arduino)
    Vector2 externalCameraInput = Vector2.zero;
    // Cinemachine override state
    CinemachineCore.AxisInputDelegate previousCinemachineGetAxis = null;
    bool cinematineInputOverridden = false;

    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] CinemachineInputAxisController ciac;

    [Header("Ground Check")]
    [SerializeField] Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer = ~0; // default to everything

    [Header("Gizmos")]
    [SerializeField] bool drawGroundGizmo = true;
    [SerializeField] Color groundGizmoColor = Color.yellow;
    [SerializeField] bool wireframeGizmo = true;

    CharacterController cc;
    Vector3 velocity;
    float currentVelocityY;
    float rotationVelocity;
    public Vector2 moveInput = Vector2.zero;
    bool isGrounded = false;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
    }

    // Event-style callback expected by the new Input System when using the "Send Messages" or
    // "Invoke C# Events" (PlayerInput) behavior. Wire the Move action to this method.
    public void OnMove(InputAction.CallbackContext context)
    {
        // Keep raw 2D input (typically from a Vector2 action)
        if ((context.performed || context.canceled) && !arduinoPlayer)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    // Optional example: jump callback (not implemented as vertical physics here)
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            // Set upward velocity immediately using combined gravity (gravity + downwardForce)
            float effectiveGravity = gravity + downwardForce;
            // effectiveGravity should be negative; guard against zero
            if (Mathf.Approximately(effectiveGravity, 0f)) effectiveGravity = gravity;
            velocity.y = Mathf.Sqrt(jumpForce * -2f * effectiveGravity);
        }
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = IsGrounded();

        // Reset downward velocity when grounded
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f; // small negative to keep the controller grounded

        // Apply gravity plus constant downward force so player will fall off edges reliably
        velocity.y += (gravity + downwardForce) * Time.deltaTime;

        // Convert input (x: left/right, y: forward/back) into camera-relative direction on XZ plane
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 move = Vector3.zero;

        if (inputDir.sqrMagnitude > 0.0001f)
        {
            // Camera-forward and camera-right on XZ plane
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            move = (camRight * inputDir.x + camForward * inputDir.z).normalized * moveSpeed;

            // Smoothly rotate the player to face movement direction
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        Vector3 total = move + new Vector3(0f, velocity.y, 0f);
        cc.Move(total * Time.deltaTime);
    }

    // Ground detection using Physics.CheckSphere. Returns true when the sphere overlaps any collider on groundLayer.
    public bool IsGrounded()
    {
        Vector3 spherePos = transform.position + groundCheckOffset;
        return Physics.CheckSphere(spherePos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGroundGizmo)
            return;

        Gizmos.color = groundGizmoColor;
        Vector3 spherePos = transform.position + groundCheckOffset;
        if (wireframeGizmo)
        {
            Gizmos.DrawWireSphere(spherePos, groundCheckRadius);
        }
        else
        {
            Gizmos.DrawSphere(spherePos, groundCheckRadius);
        }
    }
}
