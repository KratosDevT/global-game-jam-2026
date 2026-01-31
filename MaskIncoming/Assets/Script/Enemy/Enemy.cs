using System.Collections;
using Script.Enums;
using Script.Level;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    private Tile previousTile = null;
    private Tile currentTile;
    private Tile targetTile;

    private IMaze maze;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 targetPosition;

    private const float arrivalThreshold = 0.1f;

    private enum EnemyState { Patrol, Chase, Idle }

    private enum Direction { Nord, Est, Sud, Ovest }

    private EnemyState currentState;

    float distanceToPlayer = 0.0f;
    float distanceToTarget = 0.0f;

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
        targetPosition = startPosition;
        currentState = EnemyState.Idle;
        Debug.Log($"Enemy " + gameObject.name + " inizializzato. Start: {startPosition}");
    }

    void Update()
    {
        if (maze == null)
        {
            Debug.LogError($"Maze not setted into enemy!!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player non trovato! Assicurati che abbia il tag 'Player'");
            return;
        }

        currentPosition = transform.position;
        distanceToPlayer = Vector2.Distance(currentPosition, player.transform.position);
        distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior(currentPosition);
                break;

            case EnemyState.Idle:
                IdleBehaviour(currentPosition);
                break;

            case EnemyState.Chase:
                ChaseBehavior(currentPosition);
                break;

            default:
                break;
        }
        UpdateAnimator();
    }

    void PatrolBehavior(Vector2 currentPos)
    {
        moveSpeed = patrolSpeed;
        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;

        if (distanceToPlayer <= chaseRadius)
        {
            //currentState = EnemyState.Chase;
            // calculateTile to Player
            return;
        }

        if (distanceToTarget < arrivalThreshold)
        {
            previousTile = currentTile;
            currentTile = targetTile;
            currentState = EnemyState.Idle;
        }
    }

    void IdleBehaviour(Vector2 currentPos)
    {
        moveSpeed = 0.0f;
        movement = Vector2.zero;

        CalculateTargetTile();

        currentState = EnemyState.Patrol;
    }

    void ChaseBehavior(Vector2 currentPos)
    {
        moveSpeed = chaseSpeed;
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
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato in FixedUpdate su " + gameObject.name);
            return;
        }

        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void CalculateTargetTile()
    {
        List<Tile> list = maze.GetNeighborsMinusPrevious(currentTile, previousTile).ToList();

        if (list.Count > 0)
        {
            targetTile = list[Random.Range(0, list.Count)];
            targetPosition = maze.TileToWorld(targetTile);
        }
        else
        {
            targetTile = currentTile;
            targetPosition = currentPosition;
            Debug.LogError("La lista per il prossimo Tile è vuota per l'enemy: " + gameObject.name);
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
        currentTile = _maze.GetTile((int)transform.position.x, (int)transform.position.y);
        maze = _maze;
    }
}