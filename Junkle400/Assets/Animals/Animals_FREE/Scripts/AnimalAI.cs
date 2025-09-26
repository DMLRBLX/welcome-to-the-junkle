using System.Collections;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CreatureMover))]
    public class AnimalAI : MonoBehaviour
    {
        
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float idleTimeMin = 2f;
        [SerializeField] private float idleTimeMax = 5f;
        [SerializeField] private float moveTimeMin = 3f;
        [SerializeField] private float moveTimeMax = 8f;
        [SerializeField] private float runChance = 0.2f; 
        
       
        [SerializeField] private bool canCauseDistraction = true;
        [SerializeField] private float distractionRadius = 5f;
        [SerializeField] private float distractionCooldown = 10f;
        [SerializeField] private LayerMask playerLayer = 1; 
        
       
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] animalSounds;
        
        private CreatureMover creatureMover;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Camera playerCamera;
        private float lastDistractionTime;
        
        private enum AIState
        {
            Idle,
            Moving,
            Distracted
        }
        
        private AIState currentState = AIState.Idle;
        private bool isRunning = false;
        
        private void Start()
        {
            creatureMover = GetComponent<CreatureMover>();
            startPosition = transform.position;
            targetPosition = transform.position;
            
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindObjectOfType<Camera>();
            
            StartCoroutine(AIBehaviorLoop());
            
            if (canCauseDistraction)
                StartCoroutine(CheckForDistractionOpportunity());
        }
        
        private void Update()
        {
            UpdateMovement();
        }
        
        private void UpdateMovement()
        {
            Vector2 moveAxis = Vector2.zero;
            Vector3 lookTarget = transform.position + transform.forward;
            
            if (playerCamera != null)
                lookTarget = playerCamera.transform.position;
            
            switch (currentState)
            {
                case AIState.Idle:
                    break;
                    
                case AIState.Moving:
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    moveAxis = new Vector2(direction.x, direction.z);
                    
                    if (Vector3.Distance(transform.position, targetPosition) < 1f)
                    {
                        currentState = AIState.Idle;
                    }
                    break;
                    
                case AIState.Distracted:
                    Vector3 playerPos = playerCamera ? playerCamera.transform.position : transform.position;
                    Vector3 toPlayer = (playerPos - transform.position).normalized;
                    moveAxis = new Vector2(toPlayer.x, toPlayer.z) * 0.5f; 
                    break;
            }
            
            creatureMover.SetInput(moveAxis, lookTarget, isRunning, false);
        }
        
        private IEnumerator AIBehaviorLoop()
        {
            while (true)
            {
                switch (currentState)
                {
                    case AIState.Idle:
                        yield return StartCoroutine(IdleBehavior());
                        break;
                        
                    case AIState.Moving:
                        yield return StartCoroutine(MovingBehavior());
                        break;
                        
                    case AIState.Distracted:
                        yield return StartCoroutine(DistractedBehavior());
                        break;
                }
            }
        }
        
        private IEnumerator IdleBehavior()
        {
            float idleTime = Random.Range(idleTimeMin, idleTimeMax);
            yield return new WaitForSeconds(idleTime);
            
            ChooseNewTarget();
            currentState = AIState.Moving;
        }
        
        private IEnumerator MovingBehavior()
        {
            isRunning = Random.value < runChance;
            
            float moveTime = Random.Range(moveTimeMin, moveTimeMax);
            float timer = 0f;
            
            while (timer < moveTime && Vector3.Distance(transform.position, targetPosition) > 1f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            
            currentState = AIState.Idle;
            isRunning = false;
        }
        
        private IEnumerator DistractedBehavior()
        {
            PlayRandomSound();
            
            // Trigger visual distraction effects
            DistractionEffects effects = GetComponent<DistractionEffects>();
            if (effects != null)
            {
                effects.TriggerDistraction();
            }
            
            float distractTime = Random.Range(2f, 4f);
            yield return new WaitForSeconds(distractTime);
            
            currentState = AIState.Idle;
            lastDistractionTime = Time.time;
        }
        
        private void ChooseNewTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            if (Physics.Raycast(targetPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                targetPosition = hit.point;
            }
        }
        
        private IEnumerator CheckForDistractionOpportunity()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); 
                
                if (Time.time - lastDistractionTime < distractionCooldown)
                    continue;
                    
                if (currentState == AIState.Distracted)
                    continue;
                
                Collider[] playersNearby = Physics.OverlapSphere(transform.position, distractionRadius, playerLayer);
                
                if (playersNearby.Length > 0)
                {
                    if (Random.value < 0.3f) 
                    {
                        currentState = AIState.Distracted;
                    }
                }
            }
        }
        
        private void PlayRandomSound()
        {
            if (audioSource != null && animalSounds != null && animalSounds.Length > 0)
            {
                AudioClip randomSound = animalSounds[Random.Range(0, animalSounds.Length)];
                audioSource.PlayOneShot(randomSound);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"{gameObject.name} is near the player!");
            }
        }
        
    }
}