using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAnimalAI : MonoBehaviour
{
    [Header("Wandering Behavior")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float idleTimeMin = 2f;
    [SerializeField] private float idleTimeMax = 5f;
    [SerializeField] private float moveTimeMin = 3f;
    [SerializeField] private float moveTimeMax = 8f;
    [SerializeField] private float runChance = 0.2f; 
    
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    
    [Header("Mess Making")]
    [SerializeField] private bool canCauseMesses = true;
    [SerializeField] private float messChance = 0.1f; 
    [SerializeField] private float messRadius = 2f;
    [SerializeField] private GameObject[] messPrefabs; 
    [SerializeField] private int minMessCount = 1;
    [SerializeField] private int maxMessCount = 3;
    [SerializeField] private float messSpreadRadius = 1.5f; 
    
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private AnimalAnimationController animController;
    private AnimalInteractions interactions;
    
    public enum AnimalState
    {
        Idle,
        Walking,
        Running,
        Herded,
        MakingMess
    }
    
    [Header("Debug")]
    [SerializeField] private AnimalState currentState = AnimalState.Idle;
    
    public AnimalState CurrentState => currentState;
    public bool IsMoving => agent.velocity.magnitude > 0.1f;
    public float CurrentSpeed => agent.velocity.magnitude;
    public bool IsRunning => currentState == AnimalState.Running;
    
    private bool isBeingHerded = false;
    private Vector3 herdingTarget;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AnimalAnimationController>();
        interactions = GetComponent<AnimalInteractions>();
        
        startPosition = transform.position;
    }
    
    private void Start()
    {
        StartCoroutine(AIBehaviorLoop());
    }
    
    private void Update()
    {
        if (animController != null)
        {
            animController.UpdateAnimations(CurrentSpeed, IsRunning);
        }
    }
    
    private IEnumerator AIBehaviorLoop()
    {
        while (true)
        {
            if (currentState == AnimalState.Herded)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            
            switch (currentState)
            {
                case AnimalState.Idle:
                    yield return StartCoroutine(IdleBehavior());
                    break;
                    
                case AnimalState.Walking:
                case AnimalState.Running:
                    yield return StartCoroutine(MovingBehavior());
                    break;
                    
                case AnimalState.MakingMess:
                    yield return StartCoroutine(MessBehavior());
                    break;
            }
        }
    }
    
    private IEnumerator IdleBehavior()
    {
        agent.ResetPath();
        
        float idleTime = Random.Range(idleTimeMin, idleTimeMax);
        yield return new WaitForSeconds(idleTime);
        
        if (canCauseMesses && Random.value < messChance)
        {
            currentState = AnimalState.MakingMess;
        }
        else
        {
            StartWandering();
        }
    }
    
    private IEnumerator MovingBehavior()
    {
        float moveTime = Random.Range(moveTimeMin, moveTimeMax);
        float timer = 0f;
        
        while (timer < moveTime && agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        currentState = AnimalState.Idle;
    }
    
    private IEnumerator MessBehavior()
    {
        agent.ResetPath();
        
        CauseMess();
        
        yield return new WaitForSeconds(Random.Range(2f, 4f));
        
        currentState = AnimalState.Idle;
    }
    
    private void StartWandering()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            bool shouldRun = Random.value < runChance;
            
            if (shouldRun)
            {
                currentState = AnimalState.Running;
                agent.speed = runSpeed;
            }
            else
            {
                currentState = AnimalState.Walking;
                agent.speed = walkSpeed;
            }
            
            agent.SetDestination(hit.position);
        }
        else
        {
            currentState = AnimalState.Idle;
        }
    }
    
    private void CauseMess()
    {
        Debug.Log($"{gameObject.name} is causing a mess!");
        
        SpawnMessObjects();
        
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, messRadius);
        
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Knockable") || obj.name.ToLower().Contains("prop"))
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 forceDirection = (obj.transform.position - transform.position).normalized;
                    rb.AddForce(forceDirection * 5f, ForceMode.Impulse);
                }
            }
        }
    }
    
    private void SpawnMessObjects()
    {
        if (messPrefabs == null || messPrefabs.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} tried to make mess but no mess prefabs assigned!");
            return;
        }
        
        int messCount = Random.Range(minMessCount, maxMessCount + 1);
        
        for (int i = 0; i < messCount; i++)
        {
            GameObject messPrefab = messPrefabs[Random.Range(0, messPrefabs.Length)];
            
            Vector3 randomOffset = Random.insideUnitCircle * messSpreadRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
            
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }
            else
            {
                spawnPos = transform.position;
            }
            
            Quaternion prefabRotation = messPrefab.transform.rotation;
            Quaternion correctRotation = Quaternion.Euler(0, prefabRotation.eulerAngles.y, prefabRotation.eulerAngles.z);
            GameObject messObj = Instantiate(messPrefab, spawnPos, correctRotation);
            
            Debug.Log($"Spawned mess: {messPrefab.name} at {spawnPos}");
        }
    }
    
    public void StartHerding(Vector3 targetPosition, float duration = 5f)
    {
        if (currentState == AnimalState.Herded) return; // Already being herded
        
        StartCoroutine(HerdingBehavior(targetPosition, duration));
    }
    
    private IEnumerator HerdingBehavior(Vector3 targetPosition, float duration)
    {
        currentState = AnimalState.Herded;
        
        agent.speed = walkSpeed;
        agent.SetDestination(targetPosition);
        
        yield return new WaitForSeconds(duration);
        
        currentState = AnimalState.Idle;
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Show wander radius
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
        
        // Show mess radius
        if (canCauseMesses)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, messRadius);
        }
        
        // Show current destination
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawSphere(agent.destination, 0.5f);
        }
    }
}