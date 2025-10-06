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

    Vector2 currentLook = Vector2.zero; // x = yaw, y = pitch
    Vector2 lookVelocity = Vector2.zero;

    // External input from Arduino
    Vector2 arduinoLook = Vector2.zero;

    void Start()
    {
        // Initialize with current rotation
        currentLook.x = transform.eulerAngles.y;
        currentLook.y = transform.eulerAngles.x;
    }

    void FixedUpdate()
    {
        // Poll the look action so sticks produce continuous input while held
        if (!useArduinoInput && lookAction != null && lookAction.action != null && lookAction.action.enabled)
        {
            Vector2 pv = lookAction.action.ReadValue<Vector2>();
            // Treat polled input as per-second input (sticks), so multiply by deltaTime
            currentLook.x += pv.x * sensitivity * Time.deltaTime;
            currentLook.y += pv.y * sensitivity * Time.deltaTime * -1f; // invert Y
        }

        Vector2 desiredLook = currentLook;
        if (useArduinoInput)
            desiredLook = arduinoLook;

        // Smooth toward desired look
        Vector2 smoothed = Vector2.SmoothDamp(currentLook, desiredLook, ref lookVelocity, smoothing);
        currentLook = smoothed;

        // currentLook stores angles (degrees). Inputs update these angles in OnLook/SetArduinoLook.
        float yaw = currentLook.x;
        float pitch = currentLook.y;

        // Clamp pitch
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply yaw to player target if requested
        if (target != null && rotateTargetYaw)
        {
            Vector3 tEuler = target.eulerAngles;
            tEuler.y = yaw;
            target.eulerAngles = tEuler;
        }

        // Set camera rotation around head
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
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
            // Y is typically inverted for look controls (push up = look up), invert if needed
            currentLook.x += v.x * sensitivity * dt;
            currentLook.y += v.y * sensitivity * dt * -1f;
        }
    }

    // Called by external code (Arduino) to set the look input absolute values or deltas.
    // Expect values in -1..1 range; multiply in caller if needed.
    public void SetArduinoLook(Vector2 v)
    {
        // Treat Arduino input as deltas. Multiply by sensitivity (and optionally by fixed delta time if you
        // want frame-rate independence). Here we assume v is in -1..1 representing a per-frame delta.
        currentLook.x += v.x * sensitivity;
        currentLook.y += v.y * sensitivity * -1f; // invert Y
        arduinoLook = currentLook;
    }
}
