using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimalAnimationController : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private string verticalParameterName = "Vert";
    [SerializeField] private string stateParameterName = "State";
    [SerializeField] private string jumpParameterName = "Jump";
    
    [Header("Animation Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private float speedThreshold = 0.1f; // Minimum speed to consider "moving"
    
    private Animator animator;
    private SimpleAnimalAI animalAI;
    
    // Animation smoothing
    private float currentVertical;
    private float currentState;
    private float verticalVelocity;
    private float stateVelocity;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        animalAI = GetComponent<SimpleAnimalAI>();
    }
    
    public void UpdateAnimations(float currentSpeed, bool isRunning)
    {
        // Calculate target animation values
        float targetVertical = 0f;
        float targetState = 0f;
        
        if (currentSpeed > speedThreshold)
        {
            // Normalize speed to 0-1 range for animation
            float maxSpeed = isRunning ? 5f : 2f; // These should match your AI speed settings
            targetVertical = Mathf.Clamp01(currentSpeed / maxSpeed);
            
            // State: 0 = walk, 1 = run
            targetState = isRunning ? 1f : 0f;
        }
        
        // Smooth the animation parameters
        currentVertical = Mathf.SmoothDamp(currentVertical, targetVertical, ref verticalVelocity, animationSmoothTime);
        currentState = Mathf.SmoothDamp(currentState, targetState, ref stateVelocity, animationSmoothTime);
        
        // Set animator parameters
        animator.SetFloat(verticalParameterName, currentVertical);
        animator.SetFloat(stateParameterName, currentState);
        
        // Handle special states
        if (animalAI != null)
        {
            switch (animalAI.CurrentState)
            {
                case SimpleAnimalAI.AnimalState.MakingMess:
                    // Could trigger a special "mess making" animation here
                    // animator.SetTrigger("MakeMess");
                    break;
                    
                case SimpleAnimalAI.AnimalState.Herded:
                    // Could modify animations for herded state
                    // Maybe slower, more docile movements
                    break;
            }
        }
    }
    
    // Call this for jump animations (if needed)
    public void TriggerJump()
    {
        if (animator != null && !string.IsNullOrEmpty(jumpParameterName))
        {
            animator.SetTrigger(jumpParameterName);
        }
    }
    
    // Call this for any special animation triggers
    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
    
    // Set a boolean parameter
    public void SetAnimationBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
    }
    
    // Debug info
    private void OnGUI()
    {
        if (Application.isPlaying && Debug.isDebugBuild)
        {
            GUILayout.BeginArea(new Rect(10, 100, 200, 100));
            GUILayout.Label($"Vertical: {currentVertical:F2}");
            GUILayout.Label($"State: {currentState:F2}");
            if (animalAI != null)
            {
                GUILayout.Label($"AI State: {animalAI.CurrentState}");
                GUILayout.Label($"Speed: {animalAI.CurrentSpeed:F2}");
            }
            GUILayout.EndArea();
        }
    }
}