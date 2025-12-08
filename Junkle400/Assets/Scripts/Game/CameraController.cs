using UnityEngine;
using UnityEngine.InputSystem;

// First-person camera controller that supports both InputSystem look input and external Arduino input.
[AddComponentMenu("Game/Camera Controller")]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // usually the player root
    public Vector3 headOffset = new Vector3(0f, 0f, 0f); // camera offset from target position (head height)

    [Header("Rotation")]
    public float sensitivity = 5f;
    public float smoothing = 0.05f;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    public bool rotateTargetYaw = true; // apply yaw to target transform (so player rotates)

    [Header("Input")]
    public bool useArduinoInput = false;
    public UnityEngine.InputSystem.InputActionReference lookAction;

    // Target and smoothed angles (degrees)
    float targetYaw;
    float targetPitch;
    float currentYaw;
    float currentPitch;
    float yawVelocity;   // for SmoothDampAngle
    float pitchVelocity; // for SmoothDampAngle

    void Start()
    {
        // Initialize with current rotation
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
        targetYaw = currentYaw;
        targetPitch = currentPitch;
    }

    void Update()
    {
        // Poll the look action so sticks produce continuous input while held
        if (!useArduinoInput && lookAction != null && lookAction.action != null && lookAction.action.enabled)
        {
            Vector2 pv = lookAction.action.ReadValue<Vector2>();
            // Treat polled input as per-second input (sticks), so multiply by deltaTime
            targetYaw += pv.x * sensitivity * Time.deltaTime;
            targetPitch += pv.y * sensitivity * Time.deltaTime * -1f; // invert Y
        }
    }

    void LateUpdate()
    {
        // Clamp target pitch first
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Smooth toward target angles using angle-aware damping
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothing);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothing);
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Apply yaw to player target if requested
        if (target != null && rotateTargetYaw)
        {
            Vector3 tEuler = target.eulerAngles;
            tEuler.y = currentYaw;
            target.eulerAngles = tEuler;
        }

        // Set camera rotation around head
        Quaternion rot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 headPos = (target != null) ? target.position + headOffset : headOffset;
        transform.SetPositionAndRotation(headPos, rot);
    }

    // Input System event callback (wire to a Vector2 look action)
    public void OnLook(InputAction.CallbackContext context)
    {
        if (!useArduinoInput)
        {
            // Read input and apply sensitivity. For mouse, treat value as raw delta; for gamepad/other devices
            // multiply by Time.deltaTime so sticks produce smooth rotation.
            Vector2 v = context.ReadValue<Vector2>();
            bool isMouse = context.control != null && context.control.device is UnityEngine.InputSystem.Mouse;
            float dt = isMouse ? 1f : Time.deltaTime;
            targetYaw += v.x * sensitivity * dt;
            targetPitch += v.y * sensitivity * dt * -1f; // invert Y
        }
    }

    // Called by external code (Arduino) to set the look input absolute values or deltas.
    // Expect values in -1..1 range; multiply in caller if needed.
    public void SetArduinoLook(Vector2 v)
    {
        // Treat Arduino input as deltas in -1..1 and apply sensitivity.
        // Update the target angles; smoothing is applied in LateUpdate.
        targetYaw += v.x * sensitivity;
        targetPitch += v.y * sensitivity * -1f; // invert Y
    }
}
