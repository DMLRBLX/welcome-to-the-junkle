// ...existing code...
using UnityEngine;

public class broomsweep : MonoBehaviour
{
    private Animator animator;
    private CharacterController cc;
    private PlayerController playerController;

    public KeyCode bKey = KeyCode.B;
    public KeyCode bButtonJoystick = KeyCode.JoystickButton1; // B on most controllers

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (animator == null) return;

        if (Input.GetKeyDown(bKey) || Input.GetKeyDown(bButtonJoystick))
        {
            animator.SetTrigger("sweep");
        }
    }
}