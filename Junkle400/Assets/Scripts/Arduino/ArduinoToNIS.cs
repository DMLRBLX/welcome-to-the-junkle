using UnityEngine;
using UnityEngine.InputSystem;

public class ArduinoToNIS : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction moveAction;
    private Vector3 movement;
    private float moveSpeed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        jumpAction.Enable();
        moveAction.Enable();
    }

    public void OnJump()
    {
        playerController.Jump();
    }

    public void OnMove()
    {
        Vector2 wasdMovement = moveAction.ReadValue<Vector2>();
        playerController.moveInput = wasdMovement;
    }

    public void SetPlayerMovement(Vector2 movementVector)
    {
        movement = new Vector3(movementVector.x, 0f, movementVector.y);
    }


}
