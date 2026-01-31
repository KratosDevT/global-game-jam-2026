using UnityEngine;

public class Enemy : MonoBehaviour
{    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float patrolSpeed = 1.0f; // Velocità durante patrol
    [SerializeField] private float chaseSpeed = 3.0f;  // Velocità durante inseguimento
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float chaseRadius = 3.0f; // Raggio oltre il quale insegue
    [SerializeField] private float squareSize = 3.0f;
    
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private float attackRange = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private Vector2 movement;
    private float lastAttackTime;
    
    // Patrol
    private Vector2 startPosition;
    private Vector2 finalPosition;
    private int currentCorner = 0;
    private Vector2[] corners;
    private Vector2 targetPosition;
    private const float arrivalThreshold = 0.1f;
    
    // Stati
    private enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Trova il player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player non trovato! Assicurati che abbia il tag 'Player'");
        }
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato su " + gameObject.name);
            return;
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Setup patrol
        startPosition = transform.position;
        finalPosition = startPosition + new Vector2(squareSize, 0);
        corners = new Vector2[2]
        {
            finalPosition,
            startPosition
        };
        targetPosition = finalPosition;
        
        Debug.Log($"Enemy inizializzato. Start: {startPosition}, Target: {targetPosition}");
    }

    void Update()
    {
        if (rb == null) return;
        
        Vector2 currentPos = transform.position;
        
        // Determina stato in base alla distanza dal player
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(currentPos, player.position);
            
            if (distanceToPlayer <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer > chaseRadius && distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
            else if (distanceToPlayer > detectionRange)
            {
                currentState = EnemyState.Patrol;
            }
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
        
        // Comportamento in base allo stato
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior(currentPos);
                moveSpeed = patrolSpeed;
                break;
                
            case EnemyState.Chase:
                ChaseBehavior(currentPos);
                moveSpeed = chaseSpeed;
                break;
                
            case EnemyState.Attack:
                AttackBehavior();
                break;
        }
        
        UpdateAnimator();
    }

    void PatrolBehavior(Vector2 currentPos)
    {
        // Movimento patrol (va avanti e indietro)
        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;
        
        float distanceToTarget = Vector2.Distance(currentPos, targetPosition);
        
        if (distanceToTarget < arrivalThreshold)
        {
            currentCorner = (currentCorner + 1) % corners.Length;
            targetPosition = corners[currentCorner];
        }
    }

    void ChaseBehavior(Vector2 currentPos)
    {
        // Insegue il player
        Vector2 direction = (player.position - (Vector3)currentPos).normalized;
        movement = direction;
    }

    void AttackBehavior()
    {
        // Fermo e attacca
        movement = Vector2.zero;
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // TODO: Trigger animazione attacco
            // animator.SetTrigger("Attack");
            
            // PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damage);
            //     Debug.Log($"Vampiro attacca! Player HP: {playerHealth.currentHealth}");
            // }
            
            lastAttackTime = Time.time;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        
        animator.SetFloat("Speed", movement.magnitude);
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                // if (playerHealth != null)
                // {
                //     playerHealth.TakeDamage(damage);
                //     lastAttackTime = Time.time;
                // }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Visualizza range detection (giallo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Visualizza chase radius (arancione)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        
        // Visualizza attack range (rosso)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Visualizza patrol path (ciano)
        if (corners != null && corners.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 nextCorner = corners[(i + 1) % corners.Length];
                Gizmos.DrawLine(corners[i], nextCorner);
                Gizmos.DrawWireSphere(corners[i], 0.2f);
            }
        }
        
        // Target corrente (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
    }
}