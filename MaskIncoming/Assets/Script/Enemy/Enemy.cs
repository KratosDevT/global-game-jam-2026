using System.Collections;
using Script.Enums;
using UnityEngine;

public class Enemy : MonoBehaviour
{    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.0f;
    [SerializeField] private float chaseRadius = 3.0f;
    
    //[Header("Combat")]
    // [SerializeField] private int damage = 10;
    // [SerializeField] private float attackCooldown = 2.0f;
    //[SerializeField] private float attackRange = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject player;
    private Vector2 movement;

    private Tile initialTile;
    private Tile currentTile;
    private Tile targetTile;

    private Vector2 startPosition;
    private Vector2 finalPosition;
    private Vector2 targetPosition;
    private const float arrivalThreshold = 0.1f;
    
    private enum EnemyState { Patrol, Chase, Idle }

    private enum Direction { Nord, Est, Sud, Ovest }

    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato su " + gameObject.name);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player non trovato! Assicurati che abbia il tag 'Player'");
            return;
        }
        
        startPosition = transform.position;
        Debug.Log($"Enemy " + gameObject.name + " inizializzato. Start: {startPosition}");
    }

    void Update()
    {
        Vector2 currentPos = transform.position;
        float distanceToPlayer = Vector2.Distance(currentPos, player.transform.position);
        
        if (distanceToPlayer <= chaseRadius)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior(currentPos);
                moveSpeed = patrolSpeed;
                break;
                
                case EnemyState.Idle:
                IdleBehaviour(currentPos);
                moveSpeed = 0.0f;
                movement = Vector2.zero;
                break;
                
            case EnemyState.Chase:
                ChaseBehavior(currentPos);
                moveSpeed = chaseSpeed;
                break;
            
            default:
                break;
        }
        
        UpdateAnimator();
    }

    void PatrolBehavior(Vector2 currentPos)
    {
        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;

        float distanceToTarget = Vector2.Distance(currentPos, targetPosition);
        
        if (distanceToTarget < arrivalThreshold)
        {
            currentTile = targetTile;
            currentState = EnemyState.Idle;
        }
    }

    void IdleBehaviour(Vector2 currentPos)
    {
        Tile nextTile = new Tile(1,1);
        currentTile.HasPath(((int)Direction.Nord));
        targetTile = nextTile;

        targetPosition = Vector3.zero;
        currentState = EnemyState.Patrol;
    }

    void ChaseBehavior(Vector2 currentPos)
    {
        //casistica che va verso il player

        // // Insegue il player
        // Vector2 direction = (player.transform.position - (Vector3)currentPos).normalized;
        // movement = direction;
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
            //attack player
        }
    }

    public void SetSpawningTile(Tile tile)
    {
        initialTile = tile;
        currentTile = tile;
    }
}