using UnityEngine;

public class Enemy : MonoBehaviour
{    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float detectionRange = 5.0f;
    [SerializeField] private float squareSize = 3.0f;
    
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 2.0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private float lastAttackTime;
    
    private Vector2 startPosition;
    private int currentCorner = 0;
    private Vector2[] corners;
    private Vector2 targetPosition;
    private const float arrivalThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato su " + gameObject.name);
            return;
        }
        
        // Assicurati che sia configurato correttamente
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        startPosition = transform.position;
        
        corners = new Vector2[4]
        {
            startPosition + new Vector2(squareSize, 0),
            startPosition + new Vector2(squareSize, squareSize),
            startPosition + new Vector2(0, squareSize),
            startPosition
        };
        
        targetPosition = corners[0];
        
        Debug.Log($"Enemy inizializzato. Start: {startPosition}, Target: {targetPosition}");
    }

    void Update()
    {
        if (rb == null) return;
        
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;
        
        float distanceToTarget = Vector2.Distance(currentPos, targetPosition);
        
        // Debug ogni 60 frame (circa 1 secondo)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Pos: {currentPos}, Target: {targetPosition}, Distance: {distanceToTarget:F2}, Movement: {movement}");
        }
        
        if (distanceToTarget < arrivalThreshold)
        {
            currentCorner = (currentCorner + 1) % corners.Length;
            targetPosition = corners[currentCorner];
            Debug.Log($"Raggiunto angolo! Nuovo target: {targetPosition}");
        }
        
        UpdateAnimator();
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
                // TakeDamage logic
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (corners != null && corners.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 nextCorner = corners[(i + 1) % corners.Length];
                Gizmos.DrawLine(corners[i], nextCorner);
                Gizmos.DrawWireSphere(corners[i], 0.2f);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);
        }
        else if (Application.isPlaying)
        {
            // Mostra posizione corrente durante il play
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}