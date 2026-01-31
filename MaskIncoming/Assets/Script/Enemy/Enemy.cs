using System.Collections;
using Script.Enums;
using Script.Level;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.0f;
    [SerializeField] private float chaseRadius = 3.0f;

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject player;
    private Vector2 movement;

    private Tile initialTile;
    private Tile currentTile;
    private Tile targetTile;

    private IMaze maze;

    private Vector2 startPosition;
    private Vector2 finalPosition;
    private Vector2 targetPosition;
    private const float arrivalThreshold = 0.1f;
    private bool choosingPath;

    private enum EnemyState { Patrol, Chase, Idle }

    private enum Direction { Nord, Est, Sud, Ovest }

    private EnemyState currentState;

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
        currentState = EnemyState.Idle;
        Debug.Log($"Enemy " + gameObject.name + " inizializzato. Start: {startPosition}");
    }

    void Update()
    {
        if (maze == null)
        {
            Debug.Log($"Maze not initialized!");
            return;
        }
        Vector2 currentEnemyPosition = transform.position;
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior(currentEnemyPosition);
                moveSpeed = patrolSpeed;
                break;

            case EnemyState.Idle:
                IdleBehaviour(currentEnemyPosition);
                break;

            case EnemyState.Chase:
                ChaseBehavior(currentEnemyPosition);
                moveSpeed = chaseSpeed;
                break;

            default:
                break;
        }
        //         if (!choosingPath)
        // {

        //     // if (distanceToPlayer <= chaseRadius)
        //     // {
        //     //     currentState = EnemyState.Chase;
        //     // }
        //     // else
        //     // {

        //     // }
        //     currentState = EnemyState.Patrol;
        // }
        //         Vector2 currentPos = transform.position;
        // float distanceToPlayer = Vector2.Distance(currentPos, player.transform.position);


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
            choosingPath = true;
        }
    }

    void IdleBehaviour(Vector2 currentPos)
    {
        moveSpeed = 0.0f;
        movement = Vector2.zero;

        CalculateTargetTile();

        currentState = EnemyState.Patrol;
        choosingPath = false;
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

    void CalculateTargetTile()
    {
        if (currentTile.HasPath((int)Direction.Nord))
        {
            Tile _tile = maze.GetNeighbor(currentTile, (int)Direction.Nord);
            targetTile = _tile;
            targetPosition = maze.TileToWorld(targetTile);
        }
        else if (currentTile.HasPath((int)Direction.Sud))
        {
            Tile _tile = maze.GetNeighbor(currentTile, (int)Direction.Sud);
            targetTile = _tile;
            targetPosition = maze.TileToWorld(targetTile);
        }
        else if (currentTile.HasPath((int)Direction.Est))
        {
            Tile _tile = maze.GetNeighbor(currentTile, (int)Direction.Est);
            targetTile = _tile;
            targetPosition = maze.TileToWorld(targetTile);
        }
        else if (currentTile.HasPath((int)Direction.Ovest))
        {
            Tile _tile = maze.GetNeighbor(currentTile, (int)Direction.Ovest);
            targetTile = _tile;
            targetPosition = maze.TileToWorld(targetTile);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //attack player
        }
    }

    public void InitializeMazeData(IMaze _maze)
    {
        initialTile = _maze.GetTile((int)transform.position.x, (int)transform.position.y);
        currentTile = initialTile;
        maze = _maze;
    }
}