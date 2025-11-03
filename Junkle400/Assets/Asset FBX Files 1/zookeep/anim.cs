using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animcontroller : MonoBehaviour
{
    private Animator animator;
    private CharacterController cc;
    private PlayerController playerController;


    public float moveThreshold = 0.1f;

   
    public KeyCode sweepKey = KeyCode.F;
    [Tooltip("Joystick button for sweep (JoystickButton0..JoystickButton19)")]
    public KeyCode sweepJoystickButton = KeyCode.JoystickButton1;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode jumpJoystickButton = KeyCode.JoystickButton0;

   

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (animator == null)
            return;

        // Movement detection:
        bool moving = false;
        if (playerController != null)
        {
            moving = playerController.moveInput.sqrMagnitude > moveThreshold * moveThreshold;
        }
        else if (cc != null)
        {
            Vector3 horizontalVel = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
            moving = horizontalVel.magnitude > moveThreshold;
        }
        else
        {
            // Fallback: keyboard / joystick axes (legacy)
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            moving = new Vector2(h, v).sqrMagnitude > moveThreshold * moveThreshold;
        }

        animator.SetBool("valk", moving);

        // Sweep action (keyboard OR joystick button)
        if (Input.GetKeyDown(sweepKey) || Input.GetKeyDown(sweepJoystickButton))
        {
            animator.SetTrigger("sweep");
        }

       
        if (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpJoystickButton))
        {
            animator.SetTrigger("jump");
        }

      
    }

    // Optional helper so other scripts (Input System actions) can trigger sweep/jump directly:
    public void TriggerSweep() => animator?.SetTrigger("sweep");
    public void TriggerJump() => animator?.SetTrigger("jump");
}